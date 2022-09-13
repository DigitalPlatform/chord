using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Text
{
#if REMOVED
    /// <summary>
    /// 和加密有关的函数
    /// </summary>
    public class Cryptography
    {
        // 由于获得的结果byte[] 中可能包含不可显示的字符，所以用base64编码为字符串
        static public string GetSHA1(string strPlainText)
        {
            byte[] source = Encoding.UTF8.GetBytes(strPlainText);
            byte[] result = null;

            SHA1 sha = new SHA1CryptoServiceProvider();
            // This is one implementation of the abstract class SHA1.
            result = sha.ComputeHash(source);
            return Convert.ToBase64String(result);
        }

        /// <remarks>
        /// Depending on the legal key size limitations of a specific CryptoService provider
        /// and length of the private key provided, padding the secret key with space character
        /// to meet the legal size of the algorithm.
        /// </remarks>
        private static byte[] GetLegalKey(string Key)
        {
            //具有随机密钥的 DES 实例
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            int len = des.LegalKeySizes[0].MinSize / 8;

            string sTemp = Key;

            if (sTemp.Length > len)
                sTemp = sTemp.Substring(0, len);
            else
            {
                while (sTemp.Length < len)
                    sTemp += ' ';
            }

            // convert the secret key to byte array
            return ASCIIEncoding.ASCII.GetBytes(sTemp);
        }

        public static string Encrypt(string strSource, string strKey)
        {
            Byte[] baInput = Encoding.Unicode.GetBytes(strSource);

            //具有随机密钥的 DES 实例
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            byte[] bytKey = GetLegalKey(strKey);

            // set the private key
            des.Key = bytKey;
            des.IV = bytKey;

            //从此实例创建 DES 加密器
            ICryptoTransform desencrypt = des.CreateEncryptor();

            Byte[] baOutput = desencrypt.TransformFinalBlock(baInput, 0, baInput.Length);

            return Convert.ToBase64String(baOutput);
        }

        public static string Decrypt(string strSource, string strKey)
        {
            if (string.IsNullOrEmpty(strSource) == true)
                return "";

            byte[] baInput = Convert.FromBase64String(strSource);
            //具有随机密钥的 DES 实例
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            byte[] bytKey = GetLegalKey(strKey);

            // set the private key
            des.Key = bytKey;
            des.IV = bytKey;

            //从此 des 实例创建 DES 解密器
            ICryptoTransform desdecrypt = des.CreateDecryptor();
            Byte[] baOutput = desdecrypt.TransformFinalBlock(baInput, 0, baInput.Length);
            return Encoding.Unicode.GetString(baOutput);
        }
    }

#endif
}
