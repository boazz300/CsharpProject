using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {


            try
            {
                string localpath = args[0];
                string remote = args[1];
                string remoteip = remote.Split('@')[1];

                string remoteusername = remote.Split('@')[0].Split(':')[0];
                string remotepassword = remote.Split('@')[0].Split(':')[1];
                string remotepath = args[2].Replace(':', '$');
                using (NetworkShareAccesser.Access(remoteip, remoteusername, remotepassword))
                {
                    DateTime start = DateTime.Now;
                    File.Copy(@localpath, @"\\" + remoteip + @"\" + @remotepath, true);
                    DateTime end = DateTime.Now;
                    Console.WriteLine("total time: "+(end-start).Seconds+" seconds");

                }
                Console.WriteLine("File copied");
            }
            catch(Exception e) {
                Console.WriteLine("args incorrect. use this format:");
                Console.WriteLine("path root:1234@127.0.0.1 remotepath");
                Console.WriteLine(e.Message);

                return;

            }




        }
    }
}
