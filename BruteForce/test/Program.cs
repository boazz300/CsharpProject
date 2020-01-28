using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace test
{
    class Program
    {
        public static List<string> usernamesList;
        public static List<string> passwordsList;
        public static List<string> ipsList;
        public static List<connection> ConnectWinSuccessList;
        public static List<connection> ConnectWinErrorList;
        public static List<connection> ConnectLinuxSuccessList;
        public static List<connection> ConnectLinuxErrorList;
        public static int counterWin;
        public static int counterLinux;
        public static bool run_win;
        public static bool run_lin;


        static void Main(string[] args)
        {
            counterWin = 0;
            counterLinux = 0;
            usernamesList = new List<string>();
            passwordsList = new List<string>();
            ipsList = new List<string>();
            ConnectWinSuccessList = new List<connection>();
            ConnectWinErrorList = new List<connection>();
            ConnectLinuxSuccessList = new List<connection>();
            ConnectLinuxErrorList = new List<connection>();
            parser(args.ToList());
            loopToAll();
            saveToJason();
            if(args.Length >= 6)
            {
                printResults();
            }
        }

        private static void printResults()
        {
            Console.WriteLine("Done");
            Console.WriteLine("");
            if (run_win)
            {
                Console.WriteLine("Attempt Connect success Win : " + (counterWin - ConnectWinErrorList.Count));
                Console.WriteLine("Attempt Connect error : " + (counterWin - ConnectWinSuccessList.Count));
                foreach (connection con in ConnectWinSuccessList)
                {
                    Console.WriteLine(con.ToString());
                }
            }
            if (run_lin)
            {
                Console.WriteLine("Attempt Connect success Linux : " + (counterLinux - ConnectLinuxErrorList.Count));
                Console.WriteLine("Attempt Connect error : " + (counterWin - ConnectLinuxSuccessList.Count));
                foreach (connection con in ConnectLinuxSuccessList)
                {
                    Console.WriteLine(con.ToString());
                }
            }
            Console.WriteLine("Exit");
        }

        private static void saveToJason()
        {
            string pathWinSuccess = "SuccessWinJson.json";
            string pathWinError = "ErrorWinJson.json";
            string pathLinuxSuccess = "SuccessLinuxJson.json";
            string pathLinuxError = "ErrorLinuxJson.json";
            string res = mySerializer(ConnectWinSuccessList);
            System.IO.File.WriteAllText(pathWinSuccess, res);
            res = mySerializer(ConnectWinErrorList);
            System.IO.File.WriteAllText(pathWinError, res);
            res = mySerializer(ConnectLinuxSuccessList);
            System.IO.File.WriteAllText(pathLinuxSuccess, res);
            res = mySerializer(ConnectLinuxErrorList);
            System.IO.File.WriteAllText(pathLinuxError, res);
        }
        public static string mySerializer(List<connection> conList)
        {
            string res = "[";
            foreach (connection con in conList)
            {
                res = res + "{";
                res = res + @"""_ip""" + ": " + @"""" + con._ip + @"""" + ",";
                res = res + @"""_user""" + ": " + @"""" + con._user + @"""" + ",";
                res = res + @"""_pass""" + ": " + @"""" + con._pass + @"""" + ",";
                res = res + @"""_connectError""" + ": " + con._connectError;
                res = res + "},";
            }
            if (res[res.Length - 1].ToString() == ",")
            {
                res = res.Remove(res.Length - 1);
            }
            res = res + "]";
            return res;
        }
        private static void loopToAll()
        {
            int find;
            foreach (string ip in ipsList)
            {
                foreach (string username in usernamesList)
                {
                    find = 1;
                    foreach (string password in passwordsList)
                    {
                        if (run_win)
                        {
                            find = ConnectWin(ip, username, password);
                        }
                        if (run_lin)
                        {
                            if (find != 0)
                            {
                                find = ConnectLinux(ip, username, password);
                            }
                        }
                        if(find == 0) { break; }
                    }
                }
            }
        }

        private static void parser(List<string> args)
        {
            run_win = args.Contains("-win");
            run_lin = args.Contains("-lin");
            if (!run_lin && !run_win)
                run_lin = run_win = true;

            string remoteusername = "0";
            string remotepassword = "0";
            string remoteip = "0";
            string ipFile = "0";
            string userFile = "0";
            string passFile = "0";

            var opt = new Dictionary<string, string>();
            opt.Add("-u", "remotrusername");
            opt.Add("-uf", "userFile");
            opt.Add("-p", "remotepassword");
            opt.Add("-pf", "passFile");
            opt.Add("-i", "remoteip");
            opt.Add("-if", "ipFile");

            foreach (var o in opt)
            {
                if (args.Contains(o.Key))
                {
                    int index = args.IndexOf(o.Key) + 1;
                    try
                    {
                        if (args[index][0].ToString() != "-" && args[index][0].ToString() != "")
                        {
                            switch (o.Value)
                            {
                                case "remotrusername":
                                    remoteusername = args[index];
                                    break;
                                case "userFile":
                                    userFile = args[index];
                                    break;
                                case "remotepassword":
                                    remotepassword = args[index];
                                    break;
                                case "passFile":
                                    passFile = args[index];
                                    break;
                                case "remoteip":
                                    remoteip = args[index];
                                    break;
                                case "ipFile":
                                    ipFile = args[index];
                                    break;
                            }
                        }
                    }
                    catch { continue; }
                }
            }
            try
            {
                if (args.Count < 6)
                {
                    ShowHelp();
                }
                else
                {
                    if (remoteip != "0")
                    {
                        ipsList.Add(remoteip);
                    }
                    if (remoteusername != "0")
                    {
                        usernamesList.Add(remoteusername);
                    }
                    if (remotepassword != "0")
                    {
                        passwordsList.Add(remotepassword);
                    }
                    if (ipFile != "0")
                    {
                        addToListFromFile(ipFile, "ipsList");
                    }
                    if (userFile != "0")
                    {
                        addToListFromFile(userFile, "usernamesList");
                    }
                    if (passFile != "0")
                    {
                        addToListFromFile(passFile, "passwordsList");
                    }
                }
            }
            catch (Exception e)
            {
                ShowHelp();
                return;
            }
        }
        private static void addToListFromFile(string path, string nameList)
        {
            string line;
            if (File.Exists(path))
            {
                System.IO.StreamReader file = new System.IO.StreamReader(path);
                while ((line = file.ReadLine()) != null)
                {
                    switch (nameList)
                    {
                        case "ipsList":
                            ipsList.Add(line);
                            break;
                        case "usernamesList":
                            usernamesList.Add(line);
                            break;
                        case "passwordsList":
                            passwordsList.Add(line);
                            break;
                    }
                }
                file.Close();
            }
        }
        private static int ConnectWin(string remoteip, string remoteusername, string remotepassword)
        {
            var connect = new NetworkShareAccesser().TryConnect(remoteip, remoteusername, remotepassword);
            Console.WriteLine("Attempt Win Connect " + counterWin + ": " + remoteusername + ":" + remotepassword + "@" + remoteip + " - " + connect);
            connection con = new connection(remoteip, remoteusername, remotepassword, connect, "Windows");
            counterWin++;
            if (connect == 0)
            {
                ConnectWinSuccessList.Add(con);
            }
            else
            {
                ConnectWinErrorList.Add(con);
            }
            return connect;
        }
        private static int ConnectLinux(string remoteip, string remoteusername, string remotepassword)
        {
            int connect = 1;
            try
            {
                using (var client = new SshClient(remoteip, remoteusername, remotepassword))
                {
                    //Accept Host key
                    client.HostKeyReceived += delegate (object sender, HostKeyEventArgs e)
                    {
                        e.CanTrust = true;
                    };
                    //Start the connection
                    client.Connect();
                    if(client.IsConnected)
                    {
                        connect = 0;
                    }
                    client.Disconnect();
                }
            }
            catch { connect = 1; }

            Console.WriteLine("Attempt Linux Connect " + counterLinux + ": " + remoteusername + ":" + remotepassword + "@" + remoteip + " - " + connect);
            connection con = new connection(remoteip, remoteusername, remotepassword, connect, "Linux");
            if (connect == 0)
            {
                ConnectLinuxSuccessList.Add(con);
            }
            else
            {
                ConnectLinuxErrorList.Add(con);
            }
            counterLinux++;
            return connect;
        }
        static void ShowHelp()
        {
            Console.WriteLine("Options:");
            Console.WriteLine("-u = User name to connect with.");
            Console.WriteLine("-uf = File of users to connect with.");
            Console.WriteLine("-p = Password to connect with.");
            Console.WriteLine("-pf = File of password to connect with.");
            Console.WriteLine("-i = IP to connect with.");
            Console.WriteLine("-if = File of IP to connect with.");
            Console.WriteLine("-win = Run for Windows only.");
            Console.WriteLine("-lin = Run for Linux only");
        }
    }
}
