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
        public string id { get; private set; }

        //==========
        //Z39.50服务器配置字段：服务器地址，端口号，数据库，用户名，密码
        //==========
        public string hostName { get; set; } // 服务器地址
        public string port { get; set; }  // 端口
        public string dbNames { get; set; } // 数据库名
        public string authenticationMethod { get; set; }    // 权限验证方式

        //这些私有信息在服务器端不配置，在本地设置？
        public string groupID { get; set; } // groud id
        public string userName { get; set; }  // 用户名
        public string password { get; set; } // 密码

        //==========
        //其它字段：创建用户，创建时间，状态，审核人，审核时间
        //==========
        public string creatorPhone { get; set; } //创建者手机号
        public string creatorId { get; set; } //前期无帐户时，存cookieid 
        public string createTime { get; set; }

        public int state { get; set; }//状态，0 未审核，1审核通过，2审核不通过
        public int verifier { get; set; } //审核人
        public string verifyTime { get; set; } //审核时间


        public string Dump()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("id=" + id);
            text.AppendLine("hostName=" + hostName);
            text.AppendLine("port=" + port);
            text.AppendLine("dbNames=" + dbNames);
            text.AppendLine("authenticationMethod=" + authenticationMethod);

            text.AppendLine("creatorPhone=" + creatorPhone);
            text.AppendLine("creatorId=" + creatorId);
            text.AppendLine("createTime=" + createTime);

            text.AppendLine("state=" + state);
            text.AppendLine("verifier=" + verifier);
            text.AppendLine("verifyTime=" + verifyTime);

            return text.ToString();
        }
    }
}
