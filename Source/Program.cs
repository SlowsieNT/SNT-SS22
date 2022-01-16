using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SNTSS22
{
    internal class Program
    {
#pragma warning disable IDE0060 // Remove unused parameter
        static void Main(string[] args)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            HttpSilverSpark.Load("useragents.txt");
            var vSpark = new HttpSilverSpark("localhost", 81) {
                Body = "msg#05=#1a",
                Method = "POST",
                ReceiveDataLength = 1133
            };
            vSpark.Headers.Add("Content-Type: application/x-www-form-urlencoded");
            vSpark.OnSent += VSpark_OnSent;
            vSpark.OnReceive += VSpark_OnReceive;
            vSpark.OnError += VSpark_OnError;
            vSpark.Spark();
            Console.ReadLine();
        }

        private static void VSpark_OnReceive(object aSender, byte[] aBytes, HttpSilverSparkHandle aHSSH)
        {
            Console.WriteLine("Received:\r\n");
            Console.WriteLine(Encoding.UTF8.GetString(aBytes));
        }

        private static void VSpark_OnSent(object aSender, int aValue, HttpSilverSparkHandle aHSSH)
        {
            Console.WriteLine("Sent Length: " + aValue);
            Console.WriteLine("Sent:\r\n");
            Console.WriteLine(aHSSH.SentData);
        }

        private static void VSpark_OnError(object aSender, Exception aException)
        {
            Console.WriteLine(aException.ToString());
        }
    }
}
