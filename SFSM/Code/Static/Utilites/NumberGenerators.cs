using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFServerManager.Code
{
    public class NumberGenerators
    {
        static Random random = new Random();

        public static string GetRandomHexNumber(int digits)
        {
            byte[] buffer = new byte[digits / 2];
            random.NextBytes(buffer);
            string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            if (digits % 2 == 0)
                return result;
            return result + random.Next(16).ToString("X");
        }

        public static string GetNewUUID()
        {
            return Guid.NewGuid().ToString();
        }

    }
}
