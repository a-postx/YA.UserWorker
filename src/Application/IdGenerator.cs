using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace YA.TenantWorker.Application
{
    public static class IdGenerator
    {
        public static Guid Create(string userId)
        {
            Guid result;

            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(userId));
                result = new Guid(hash);
            }

            ////using (SHA256 sha256 = SHA256.Create())
            ////{
            ////    byte[] hash = sha256.ComputeHash(Encoding.Default.GetBytes(userId));
            ////    result = new Guid(hash.Take(16).ToArray());
            ////}

            return result;
        }
    }
}
