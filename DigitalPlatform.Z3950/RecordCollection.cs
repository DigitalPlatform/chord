using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalPlatform.Z3950
{
    public class Record
    {
        public byte[] m_baRecord = null;   // 原始形态的数据
        public string m_strSyntaxOID = "";
        public string m_strDBName = "";
        public string m_strElementSetName = ""; // B / F

        // 诊断信息
        public int m_nDiagCondition = 0;    // 0表示没有诊断信息
        public string m_strDiagSetID = "";
        public string m_strAddInfo = "";

        public string AutoDetectedSyntaxOID = "";   // 自动识别的OID，后期使用

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            if (this.m_baRecord != null)
                text.Append("Content=" + ByteArray.GetHexTimeStampString(this.m_baRecord) + "\r\n");
            if (string.IsNullOrEmpty(this.m_strSyntaxOID) == false)
                text.Append("SyntaxOID" + this.m_strSyntaxOID);
            if (string.IsNullOrEmpty(this.m_strDBName) == false)
                text.Append("DbName" + this.m_strDBName);
            if (string.IsNullOrEmpty(this.m_strElementSetName) == false)
                text.Append("ElementSetName" + this.m_strElementSetName);
            if (m_nDiagCondition != 0)
                text.Append("DiagCondition" + this.m_nDiagCondition);
            if (string.IsNullOrEmpty(this.m_strDiagSetID) == false)
                text.Append("DiagSetID" + this.m_strDiagSetID);
            if (string.IsNullOrEmpty(this.m_strAddInfo) == false)
                text.Append("AddInfo" + this.m_strAddInfo);
            if (string.IsNullOrEmpty(this.AutoDetectedSyntaxOID) == false)
                text.Append("AutoDetectedSyntaxOID" + this.AutoDetectedSyntaxOID);

            return text.ToString();
        }
    }


    public class RecordCollection : List<Record>
    {
        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach(Record record in this)
            {
                text.Append((i+1).ToString() + ") ===\r\n" + record.ToString());
                i++;
            }
            return text.ToString();
        }
    }
}

