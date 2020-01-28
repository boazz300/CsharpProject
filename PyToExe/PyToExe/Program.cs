using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PyToExe
{
    class Program
    {
        public static List<string> imports;
        public static string commandToExe = "";
        public static string fileName = "";
        public static string fileNameList = "";
        public static List<string> listFiles;
        public static string exeName = "";
        public static string workingDirectory = @"C:\temp\";

        static void Main(string[] args)
        {
            parser(args.ToList());
            imports = new List<string>();
            listFiles = new List<string>();

            if (fileName != "")
            {
                readFileImports(fileName);
                createExe(fileName, exeName);
            }
            if(fileNameList != "")
            {
                string[] paths = System.IO.File.ReadAllLines(fileNameList);
                foreach (string line in paths)
                {
                    readFileImports(line);
                    Console.WriteLine("Enter Exe Name:");
                    exeName = Console.ReadLine();
                    createExe(line, exeName);
                }
            }
            // Keep the console window open in debug mode.
            Console.WriteLine("Press any key to exit.");
            System.Console.ReadKey();
        }

        private static void createExe(string path, string name)
        {
            commandToExe = "pyinstaller --name " + name + " --onefile -y ";
            foreach (string item in imports)
            {
                commandToExe = commandToExe + "--hidden-import " + item + " ";
                Console.WriteLine(item);
            }
            commandToExe = commandToExe + path;
            imports.Clear();
            Console.WriteLine(commandToExe);
            runTheCommand();
            copyExe(path, name);
            removeFiles(name);
        }

        private static void removeFiles(string name)
        {
            string dir = workingDirectory + @"dist\";
            Directory.Delete(dir, true);
            dir = workingDirectory + @"build\";
            Directory.Delete(dir, true);
            if (File.Exists(workingDirectory + name + ".spec"))
            {
                File.Delete(workingDirectory + name + ".spec");
            }
        }

        private static void copyExe(string path, string name)
        {
            string dir = System.IO.Path.GetDirectoryName(path);
            dir = dir + @"\" + @"exe\";
            System.IO.Directory.CreateDirectory(dir);
            File.Copy(workingDirectory + @"dist\" + name + ".exe", dir + name + ".exe");
        }

        private static void runTheCommand()
        {
            System.IO.Directory.CreateDirectory(workingDirectory);
            Process cmd = new Process();

            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.WorkingDirectory = @"C:\temp\";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;

            cmd.Start();

            cmd.StandardInput.WriteLine(commandToExe);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
        }

        private static void readFileImports(string path)
        {
            string[] lines = System.IO.File.ReadAllLines(path);
            foreach (string line in lines)
            {
                if (line.Contains("import"))
                {
                    string[] words = line.Split("import ");
                    imports.Add(words[1]);
                }
            }
        }

        private static void parser(List<string> args)
        {
            var opt = new Dictionary<string, string>();
            opt.Add("-file", "fileName");
            opt.Add("-list", "fileNameList");
            opt.Add("-name", "exeName");


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
                                case "fileName":
                                    fileName = args[index];
                                    break;
                                case "fileNameList":
                                    fileNameList = args[index];
                                    break;
                                case "exeName":
                                    exeName = args[index];
                                    break;
                            }
                        }
                    }
                    catch { continue; }
                }
            }
            try
            {
                if(args.Count == 0)
                {
                    ShowHelp();
                }
            }
            catch { }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Options:");
            Console.WriteLine("-file = Path to file.");
            Console.WriteLine("-list = Path to file containing several files paths.");
            Console.WriteLine("-name = Name for the exe file.");
        }
    }
}
