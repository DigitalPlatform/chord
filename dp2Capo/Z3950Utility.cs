using DigitalPlatform.Text;
using DigitalPlatform.Z3950;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace dp2Capo
{
    public class Z3950Utility
    {
        // 根据RPN创建XML检索式
        // 本函数要递归调用，检索数据库并返回结果集
        // parameters:
        //		node    RPN 结构的根结点
        //		strXml[out] 返回局部XML检索式
        // return:
        //      -1  出错
        //      0   数据库没有找到
        //      1   成功
        public static int BuildQueryXml(
            ZHostInfo zhost,
            List<string> dbnames,
            BerNode node,
            Encoding searchTermEncoding,
            out string strQueryXml,
            out string strError)
        {
            strQueryXml = "";
            strError = "";
            int nRet = 0;

            if (node == null)
            {
                strError = "node == null";
                return -1;
            }

            if (0 == node.m_uTag)
            {
                // operand node

                // 检索得到 saRecordID
                if (node.ChildrenCollection.Count < 1)
                {
                    strError = "bad RPN structure";
                    return -1;
                }

                BerNode pChild = node.ChildrenCollection[0];

                if (102 == pChild.m_uTag)
                {
                    // AttributesPlusTerm
                    long nAttributeType = -1;
                    long nAttributeValue = -1;
                    string strTerm = "";

                    nRet = DecodeAttributeAndTerm(
                        searchTermEncoding,
                        pChild,
                        out nAttributeType,
                        out nAttributeValue,
                        out strTerm,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // return:
                    //      -1  出错
                    //      0   数据库没有找到
                    //      1   成功
                    nRet = BuildOneXml(
                        zhost,
                        dbnames,
                        strTerm,
                        nAttributeValue,
                        out strQueryXml,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                        return 0;

                    return 1;
                }
                else if (31 == pChild.m_uTag)
                {
                    // 是结果集参预了检索
                    string strResultSetID = pChild.GetCharNodeData();

                    strQueryXml = "<item><resultSetName>" + strResultSetID + "</resultSetName></item>";
                    /*
                    //
                    // 为了避免在递归运算时删除了以前保留的结果集，copy 一份
                    if (!FindAndCopyExistResultSet(strResultSetID, pResult)) {
                        throw_exception(0, _T("referred resultset not exist"));
                    }
                    //
                     * */
                }
                else
                {
                    //
                    strError = "Unsurported RPN structure";
                }

            }
            else if (1 == node.m_uTag)
            { 
                // rpnRpnOp
                //
                if (3 != node.ChildrenCollection.Count)
                {
                    strError = "bad RPN structure";
                    return -1;
                }
                //
                int nOperator = -1;

                nRet = BuildQueryXml(
                    zhost,
                    dbnames,
                    node.ChildrenCollection[0],
                    searchTermEncoding,
                    out string strXmlLeft,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    return nRet;

                nRet = BuildQueryXml(
                    zhost,
                    dbnames,
                    node.ChildrenCollection[1],
                    searchTermEncoding,
                    out string strXmlRight,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    return nRet;

                //	and     [0] 
                //	or      [1] 
                //	and-not [2] 
                nOperator = DecodeRPNOperator(node.ChildrenCollection[2]);
                if (nOperator == -1)
                {
                    strError = "DecodeRPNOperator() return -1";
                    return -1;
                }

                switch (nOperator)
                {
                    case 0: // and
                        strQueryXml = "<group>" + strXmlLeft + "<operator value='AND' />" + strXmlRight + "</group>";
                        break;
                    case 1: // or 
                        strQueryXml = "<group>" + strXmlLeft + "<operator value='OR' />" + strXmlRight + "</group>";
                        break;
                    case 2: // and-not
                        strQueryXml = "<group>" + strXmlLeft + "<operator value='SUB' />" + strXmlRight + "</group>";
                        break;
                    default:
                        // 不支持的操作符
                        strError = "unsurported operator";
                        return -1;
                }
            }
            else
            {
                strError = "bad RPN structure";
            }

            return 1;
        }

        // 构造一个检索词的XML检索式局部
        // 本函数不递归
        // return:
        //      -1  出错
        //      0   数据库没有找到
        //      1   成功
        static int BuildOneXml(
            ZHostInfo zhost,
            List<string> dbnames,
            string strTerm,
            long lAttritueValue,
            out string strQueryXml,
            out string strError)
        {
            strQueryXml = "";
            try
            {
                strError = "";

                if (dbnames.Count == 0)
                {
                    strError = "一个数据库名也未曾指定";
                    return -1;
                }

                // string strFrom = "";    // 根据nAttributeType nAttributeValue得到检索途径名


                // 先评估一下，是不是每个数据库都有一样的maxResultCount参数。
                // 如果是，则可以把这些数据库都组合为一个<target>；
                // 如果不是，则把相同的挑选出来成为一个<target>，然后多个<target>用OR组合起来

                // 为此，可以先把数据库属性对象按照maxResultCount参数排序，以便聚合是用<target>。
                // 但是这带来一个问题：最后发生的检索库的先后顺序，就不是用户要求的那个顺序了。
                // 看来，还得按照用户指定的数据库顺序来构造<item>。那么，就不得不降低聚合的可能，
                // 而仅仅聚合相邻的、maxResultCount值相同的那些

                int nPrevMaxResultCount = -1;   // 前一个MaxResultCount参数值
                List<List<BiblioDbProperty>> prop_groups = new List<List<BiblioDbProperty>>();

                List<BiblioDbProperty> props = new List<BiblioDbProperty>();
                for (int i = 0; i < dbnames.Count; i++)
                {
                    string strDbName = dbnames[i];

                    BiblioDbProperty prop = zhost.GetDbProperty(strDbName,
                        true);
                    if (prop == null)
                    {
                        strError = "数据库 '" + strDbName + "' 不存在";
                        return 0;
                    }

                    // 如果当前库的MaxResultCount参数和前面紧邻的不一样了，则需要推入当前正在使用的props，新起一个props
                    if (prop.MaxResultCount != nPrevMaxResultCount
                        && props.Count != 0)
                    {
                        Debug.Assert(props.Count > 0, "不为空的props才能推入 (1)");
                        prop_groups.Add(props);
                        props = new List<BiblioDbProperty>();   // 新增加一个props
                    }

                    props.Add(prop);

                    nPrevMaxResultCount = prop.MaxResultCount;
                }

                Debug.Assert(props.Count > 0, "不为空的props才能推入 (2)");
                prop_groups.Add(props); // 将最后一个props加入到group数组中

                for (int i = 0; i < prop_groups.Count; i++)
                {
                    props = prop_groups[i];

                    string strTargetListValue = "";
                    int nMaxResultCount = -1;
                    for (int j = 0; j < props.Count; j++)
                    {
                        BiblioDbProperty prop = props[j];

                        string strDbName = prop.DbName;
#if DEBUG
                        if (j != 0)
                        {
                            Debug.Assert(prop.MaxResultCount == nMaxResultCount, "props内的每个数据库都应当有相同的MaxResultCount参数值");
                        }
#endif

                        if (j == 0)
                            nMaxResultCount = prop.MaxResultCount;  // 只取第一个prop的值即可

                        string strFrom = zhost.GetFromName(strDbName,
                            lAttritueValue,
                            out string strOutputDbName,
                            out strError);
                        if (strFrom == null)
                            return -1;  // 寻找from名的过程发生错误

                        if (strTargetListValue != "")
                            strTargetListValue += ";";

                        Debug.Assert(strOutputDbName != "", "");

                        strTargetListValue += strOutputDbName + ":" + strFrom;
                    }

                    if (i != 0)
                        strQueryXml += "<operator value='OR' />";
                    strQueryXml += "<target list='" + strTargetListValue + "'>"
                    + "<item><word>"
                    + SecurityElement.Escape(strTerm)
                    + "</word><match>left</match><relation>=</relation><dataType>string</dataType>"
                    + "<maxCount>" + nMaxResultCount.ToString() + "</maxCount></item>"
                    + "<lang>zh</lang></target>";
                }

                // 如果有多个props，则需要在检索XML外面包裹一个<target>元素，以作为一个整体和其他部件进行逻辑操作
                if (prop_groups.Count > 1)
                    strQueryXml = "<target>" + strQueryXml + "</target>";

                return 1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
        }

        // 解析出search请求中的 数据库名列表
        static int DecodeElementSetNames(BerNode root,
            out List<string> elementset_names,
            out string strError)
        {
            elementset_names = new List<string>();
            strError = "";

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];
                /*
                if (node.m_uTag == 105)
                {
                    dbnames.Add(node.GetCharNodeData());
                }
                 * */
                // TODO: 这里需要看一下PDU定义，看看是否需要判断m_uTag
                elementset_names.Add(node.GetCharNodeData());
            }

            return 0;
        }

        // 获得search请求中的RPN根节点
        static BerNode GetRPNStructureRoot(BerNode root,
            out string strError)
        {
            strError = "";

            if (root == null)
            {
                strError = "query root is null";
                return null;
            }

            if (root.ChildrenCollection.Count < 1)
            {
                strError = "no query item";
                return null;
            }

            BerNode RPNRoot = root.ChildrenCollection[0];
            if (1 != RPNRoot.m_uTag) // type-1 query
            {
                strError = "not type-1 query. unsupported query type";
                return null;
            }

            string strAttributeSetId = ""; //attributeSetId OBJECT IDENTIFIER
            // string strQuery = "";


            for (int i = 0; i < RPNRoot.ChildrenCollection.Count; i++)
            {
                BerNode node = RPNRoot.ChildrenCollection[i];
                switch (node.m_uTag)
                {
                    case 6: // attributeSetId (OBJECT IDENTIFIER)
                        strAttributeSetId = node.GetOIDsNodeData();
                        if (strAttributeSetId != "1.2.840.10003.3.1") // bib-1
                        {
                            strError = "support bib-1 only";
                            return null;
                        }
                        break;
                    // RPNStructure (CHOICE 0, 1)
                    case 0:
                    case 1:
                        return node; // this is RPN Stucture root
                }
            }

            strError = "not found";
            return null;
        }

        // 解析出search请求中的 数据库名列表
        static int DecodeDbnames(BerNode root,
            out List<string> dbnames,
            out string strError)
        {
            dbnames = new List<string>();
            strError = "";

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];
                if (node.m_uTag == 105)
                {
                    dbnames.Add(node.GetCharNodeData());
                }
            }

            return 0;
        }


        // 解析出init请求中的 鉴别信息
        // parameters:
        //      nAuthentType 0: open(simple) 1:idPass(group)
        static int DecodeAuthentication(
            BerNode root,
            out string strGroupId,
            out string strUserId,
            out string strPassword,
            out int nAuthentType,
            out string strError)
        {
            strGroupId = "";
            strUserId = "";
            strPassword = "";
            nAuthentType = 0;
            strError = "";

            if (root == null)
            {
                strError = "root == null";
                return -1;
            }

            string strOpen = ""; // open mode authentication


            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];
                switch (node.m_uTag)
                {
                    case BerNode.ASN1_SEQUENCE:

                        nAuthentType = 1;   //  "GROUP";
                        for (int k = 0; k < node.ChildrenCollection.Count; k++)
                        {
                            BerNode nodek = node.ChildrenCollection[k];
                            switch (nodek.m_uTag)
                            {
                                case 0: // groupId
                                    strGroupId = nodek.GetCharNodeData();
                                    break;
                                case 1: // userId
                                    strUserId = nodek.GetCharNodeData();
                                    break;
                                case 2: // password
                                    strPassword = nodek.GetCharNodeData();
                                    break;
                            }
                        }

                        break;
                    case BerNode.ASN1_VISIBLESTRING:
                    case BerNode.ASN1_GENERALSTRING:
                        nAuthentType = 0; //  "SIMPLE";
                        strOpen = node.GetCharNodeData();
                        break;
                }
            }

            if (nAuthentType == 0)
            {
                int nRet = strOpen.IndexOf("/");
                if (nRet != -1)
                {
                    strUserId = strOpen.Substring(0, nRet);
                    strPassword = strOpen.Substring(nRet + 1);
                }
                else
                {
                    strUserId = strOpen;
                }
            }

            return 0;
        }

        // 解码RPN结构中的Attribute + Term结构
        static int DecodeAttributeAndTerm(
            Encoding term_encoding,
            BerNode pNode,
            out long lAttributeType,
            out long lAttributeValue,
            out string strTerm,
            out string strError)
        {
            lAttributeType = 0;
            lAttributeValue = 0;
            strTerm = "";
            strError = "";

            if (pNode == null)
            {
                strError = "node == null";
                return -1;
            }

            if (pNode.ChildrenCollection.Count < 2) //attriblist + term
            {
                strError = "bad RPN query";
                return -1;
            }

            BerNode pAttrib = pNode.ChildrenCollection[0]; // attriblist
            BerNode pTerm = pNode.ChildrenCollection[1]; // term

            if (44 != pAttrib.m_uTag) // Attributes
            {
                strError = "only support Attributes";
                return -1;
            }

            if (45 != pTerm.m_uTag) // Term
            {
                strError = "only support general Term";
                return -1;
            }

            // get attribute type and value
            if (pAttrib.ChildrenCollection.Count < 1) //attribelement
            {
                strError = "bad RPN query";
                return -1;
            }

            pAttrib = pAttrib.ChildrenCollection[0];
            if (16 != pAttrib.m_uTag) //attribelement (SEQUENCE) 
            {
                strError = "only support Attributes";
                return -1;
            }

            for (int i = 0; i < pAttrib.ChildrenCollection.Count; i++)
            {
                BerNode pTemp = pAttrib.ChildrenCollection[i];
                switch (pTemp.m_uTag)
                {
                    case 120: // attributeType
                        lAttributeType = pTemp.GetIntegerNodeData();
                        break;
                    case 121: // attributeValue
                        lAttributeValue = pTemp.GetIntegerNodeData();
                        break;
                }
            }

            // get term
            strTerm = pTerm.GetCharNodeData(term_encoding);

            if (-1 == lAttributeType
                || -1 == lAttributeValue
                || String.IsNullOrEmpty(strTerm) == true)
            {
                strError = "bad RPN query";
                return -1;
            }

            return 0;
        }

        static int DecodeRPNOperator(BerNode pNode)
        {
            if (pNode == null)
                return -1;

            if (46 == pNode.m_uTag)
            {
                if (pNode.ChildrenCollection.Count > 0)
                {
                    return pNode.ChildrenCollection[0].m_uTag;
                }
            }

            return -1;
        }
    }
}
