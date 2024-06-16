using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GECV
{
    public static class CryptUtils
    {

        public static string GetMD5HashFromBytes(byte[] arr)
        {
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(arr);
                

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("X2"));
                }
                return sb.ToString();
            
        }

        public static string GetSHA256HashFromBytes(byte[] arr)
        {
            SHA256 sha = SHA256.Create();
            byte[] retVal = sha.ComputeHash(arr);


            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("X2"));
            }
            return sb.ToString();

        }

    }
}
