using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kea.Extensions;
namespace GitBackup
{
    /// <summary>
    /// Create a backup for a git repository
    /// </summary>
    class Program
    {
        public static int Git(string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("git.exe");

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.Arguments = arguments;

            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            process.WaitForExit();

            Console.ForegroundColor = ConsoleColor.Yellow;

            string lineVal = process.StandardOutput.ReadLine();

            while (lineVal != null)
            {
                Console.WriteLine(lineVal);
                lineVal = process.StandardOutput.ReadLine();
            }

            Console.ForegroundColor = ConsoleColor.Gray;

            return process.ExitCode;
        }

        private static Dictionary<string, string> ExtractParams(string[] args)
        {
            Dictionary<string, string> Values = new Dictionary<string, string>();
            foreach (var a in args.SelectLookahead((last, current, next, index, pos) => new { last, current, next, index, pos }))
            {
                if (a.current.StartsWith("-"))
                {
                    Values.Add(a.current, a.next);
                }
            }
            return Values;
        }

        static int Main(string[] args)
        {
            var Values = ExtractParams(args);
            if (args.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("gitbu " + Assembly.GetEntryAssembly().GetName().Version.ToString());

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("backup");
                Console.WriteLine("b [-p OutputDirectory]");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(" - Create a backup of the current git repository");


                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine("restore");
                Console.WriteLine("r");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(" - Run this on a folder that contains a  single backup file where the restored repository will be created");

            }
            else if (args[0] == "backup" || args[0] == "b")
            {


                var TempName = Guid.NewGuid().ToString();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Backup to {TempName}");
                Console.ForegroundColor = ConsoleColor.Gray;

                Git($"bundle create \"{TempName}\" --all");

                DateTime Now = DateTime.Now;
                string ProjectName = Path.GetFileName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory));
                string BackupName = $"{ProjectName} {Now.Year} {Now.Month} {Now.Day}.bundle";

                if (Values.ContainsKey("-p"))
                {
                    BackupName = Path.Combine(Values["-p"], BackupName);
                    Directory.CreateDirectory(Values["-p"]);
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Move to {BackupName}");
                Console.ForegroundColor = ConsoleColor.Gray;
                System.IO.File.Move(TempName, BackupName);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Succeed :)");
                Console.ForegroundColor = ConsoleColor.Gray;


            }
            else if (args[0] == "restore" || args[0] == "r")
            {
                //Encuentra el archivo de backup
                var C = AppDomain.CurrentDomain.BaseDirectory;
                var Files = System.IO.Directory.GetFiles(C).Where(x => Path.GetExtension(x).ToLowerInvariant() == ".bundle").ToList();
                if (Files.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No files are found with extension .bundle");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return 1;
                }
                else if (Files.Count > 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("There are more than one files with extension .bundle");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return 2;
                }

                var File = System.IO.Path.GetFileName(Files.First());
                Console.WriteLine($"file {File}");


                var Verify = Git($"bundle verify \"{File}\"");
                if (Verify != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("bundle verification failed");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return 4;
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("bundle verify succeed");
                Console.ForegroundColor = ConsoleColor.Gray;

                var result = Git($"clone \"{File}\" \"{System.IO.Path.GetFileNameWithoutExtension(File)}\"");
                if (result != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("clone failed");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return 5;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("clone succeed");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("wrong command");
                Console.ForegroundColor = ConsoleColor.Gray;
                return 3;
            }
            return 0;
        }
    }
}
