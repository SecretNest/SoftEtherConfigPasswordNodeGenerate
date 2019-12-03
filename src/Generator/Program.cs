using System;
using System.Text;

namespace Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("HashedPassword: " + GetHashedPassword("JohnDoe", "P@ssw0rd"));

            Console.WriteLine("NtlmHash: " + GetNtlmHash("P@ssw0rd"));
        }

        static string GetHashedPassword(string userName, string password)
        {
            byte[] source = Encoding.ASCII.GetBytes(password + userName.ToUpper());
            byte[] result;
            using (SHA0 sha0 = new SHA0())
            {
                result = sha0.ComputeHash(source);
            }
            return Convert.ToBase64String(result);
        }

        static string GetNtlmHash(string password)
        {
            byte[] source = Encoding.Unicode.GetBytes(password);
            byte[] result;
            using (MD4 sha0 = new MD4())
            {
                result = sha0.ComputeHash(source);
            }
            return Convert.ToBase64String(result);
        }
    }
}
