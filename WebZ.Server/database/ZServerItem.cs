using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebZ.Server.database
{
    public class ZServerItem
    {
        

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get;  set; }


        //==========
        //Z39.50服务器配置字段
        //==========
        public string name { get; set; } // 服务器名称
        public string addr { get; set; } // 服务器地址
        public string port { get; set; }  // 端口
        public string homepage { get; set; } //主页

        public string dbnames { get; set; } // 数据库名

        public int authmethod { get; set; }    // 权限验证方式 open:0,ID/PASS:1
        public string groupid { get; set; } // groud id
        public string username { get; set; }  // 用户名  
        public string password { get; set; } // 密码  //加密todo

        //===
        // 其它配置字段
        public int recsperbatch { get; set; }
        public string defaultMarcSyntaxOID { get; set; }
        public string defaultElementSetName { get; set; }
        public int firstfull { get; set; }
        public int detectmarcsyntax { get; set; }
        public int ignorereferenceid { get; set; }

        public int isbn_force13 { get; set; }
        public int isbn_force10 { get; set; }
        public int isbn_addhyphen { get; set; }
        public int isbn_removehyphen { get; set; }
        public int isbn_wild { get; set; }

        public string queryTermEncoding { get; set; }
        public string defaultEncoding { get; set; }
        public string recordSyntaxAndEncodingBinding { get; set; } //先不加
        public int charNegoUtf8 { get; set; }
        public int charNego_recordsInSeletedCharsets { get; set; }

        //converteacc  //0/1   dp2catalog界面上隐藏了该字段

        //==========
        //其它辅助字段：创建用户，创建时间，状态，审核人，审核时间
        //==========
        public string creatorPhone { get; set; } //创建者手机号
        public string creatorIP { get; set; } //前期无帐户时，存IP地址
        public string createTime { get; set; }
        public int state { get; set; }//状态，0 未审核，1审核通过，2审核不通过
        public string verifier { get; set; } //审核人帐号
        public string lastModifyTime { get; set; } //最后修改时间
        public string remark { get; set; } //备注


        public string Dump()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("id=" + id);

            // 主要字段
            text.AppendLine("name=" + name);
            text.AppendLine("addr=" + addr);
            text.AppendLine("port=" + port);
            text.AppendLine("homepage=" + homepage); //homepage

            text.AppendLine("dbnames=" + dbnames);
            text.AppendLine("authmethod=" + authmethod);
            text.AppendLine("groupid=" + groupid);
            text.AppendLine("username=" + username);
            text.AppendLine("password=" + password);

            // 其它这段
            text.AppendLine("recsperbatch=" + recsperbatch);
            text.AppendLine("defaultMarcSyntaxOID=" + defaultMarcSyntaxOID);
            text.AppendLine("defaultElementSetName=" + defaultElementSetName);
            text.AppendLine("firstfull=" + firstfull);
            text.AppendLine("detectmarcsyntax=" + detectmarcsyntax);
            text.AppendLine("ignorereferenceid=" + ignorereferenceid);


            text.AppendLine("isbn_force13=" + isbn_force13);
            text.AppendLine("isbn_force10=" + isbn_force10);
            text.AppendLine("isbn_addhyphen=" + isbn_addhyphen);
            text.AppendLine("isbn_removehyphen=" + isbn_removehyphen);
            text.AppendLine("isbn_wild=" + isbn_wild);

            text.AppendLine("queryTermEncoding=" + queryTermEncoding);
            text.AppendLine("defaultEncoding=" + defaultEncoding);
            text.AppendLine("recordSyntaxAndEncodingBinding=" + recordSyntaxAndEncodingBinding);
            text.AppendLine("charNegoUtf8=" + charNegoUtf8);
            text.AppendLine("charNego_recordsInSeletedCharsets=" + charNego_recordsInSeletedCharsets);


        // 辅助字段
        text.AppendLine("creatorPhone=" + creatorPhone);
            text.AppendLine("creatorIP=" + creatorIP);
            text.AppendLine("createTime=" + createTime);
            text.AppendLine("state=" + state);
            text.AppendLine("verifier=" + verifier);
            text.AppendLine("lastModifyTime=" + lastModifyTime);
            text.AppendLine("remark=" + remark);

            return text.ToString();
        }
    }
}
