using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LizhiRedBaoFiddlerPlugin
{
    public class DesUtil
    {
        private static readonly string encryptKey = "0fe3";

        //字符串加密
        public static string Encrypt(string str)
        {
            var descsp = new DESCryptoServiceProvider();
            var key = Encoding.Unicode.GetBytes(encryptKey);
            var data = Encoding.Unicode.GetBytes(str);
            var MStream = new MemoryStream();
            var CStream = new CryptoStream(MStream, descsp.CreateEncryptor(key, key), CryptoStreamMode.Write);
            CStream.Write(data, 0, data.Length);
            CStream.FlushFinalBlock();
            return Convert.ToBase64String(MStream.ToArray());
        }

        //字符串解密
        public static string Decrypt(string str)
        {
            var descsp = new DESCryptoServiceProvider();
            var key = Encoding.Unicode.GetBytes(encryptKey);
            var data = Convert.FromBase64String(str);
            var MStream = new MemoryStream();
            var CStram = new CryptoStream(MStream, descsp.CreateDecryptor(key, key), CryptoStreamMode.Write);
            CStram.Write(data, 0, data.Length);
            CStram.FlushFinalBlock();
            return Encoding.Unicode.GetString(MStream.ToArray());
        }
    }
}
