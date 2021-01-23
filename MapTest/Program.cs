﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace MapTest
{
    class Program
    {

        public static string workingDir = Directory.GetCurrentDirectory();
        public static string confFile = Path.Combine(workingDir, "openjk.conf");

        static void Main(string[] args)
        {

            string choice;
            int mapChoice = 0;
            string map;
            int i = 1;
            List<string> maps = new List<string>();
            Boolean configured = false;

            string openjkPath;
            string mbiiPath;
            string dedicatedEXE;
            string clientEXE;
            string serverConfig;

            string serverConfigData;

            do
            {

                /* Setup conf file so we have a save of OpenJK Directory */
                if (!File.Exists(confFile))
                {

                    Console.WriteLine("Please enter your OpenJK GameData Path");
                    Console.WriteLine("----------------------------");
                    openjkPath = Console.ReadLine();


                    if (!File.Exists(Path.Combine(openjkPath, "mbiided.x86.exe")))
                    {
                        Console.WriteLine("Unable to find mbiided.x86.exe in your GameData Path");
                        configured = false;
                    }

                    if (!Directory.Exists(Path.Combine(openjkPath, "MBII")))
                    {
                        Console.WriteLine("Unable to find MBII Directory in your GameData Path");
                        configured = false;
                    }

                    using (StreamWriter sw = File.CreateText(confFile))
                    {
                        sw.WriteLine(openjkPath);
                        configured = true;
                    }

                }
                else
                {
                    openjkPath = File.ReadAllText(confFile).Replace("\n", "").Replace("\r", "");
                    configured = true;
                }

            }
            while (configured == false);

            /* Now some Checking */


            dedicatedEXE = Path.Combine(openjkPath, "mbiided.x86.exe");
            clientEXE = Path.Combine(openjkPath, "mbii.x86.exe");
            mbiiPath = Path.Combine(openjkPath, "MBII");
            serverConfig = Path.Combine(mbiiPath, "server_config_default.cfg");


            if (!File.Exists(dedicatedEXE))
            {
                Console.WriteLine($"Unable to find {dedicatedEXE}");
                Console.ReadLine();
                Environment.Exit(0);
            }

            if (!File.Exists(clientEXE))
            {
                Console.WriteLine($"Unable to find {clientEXE}");
                Console.ReadLine();
                Environment.Exit(0);
            }

            if (!Directory.Exists(mbiiPath))
            {
                Console.WriteLine($"Unable to find {mbiiPath}");
                Console.ReadLine();
                Environment.Exit(0);
            }

            if (!File.Exists(serverConfig))
            {
                Console.WriteLine($"Unable to find {serverConfig}");
                Console.ReadLine();
                Environment.Exit(0);
            }
            


            // Force MBMODE 2
            serverConfigData = File.ReadAllText(serverConfig);
            serverConfigData = serverConfigData.Replace("g_Authenticity \"0\"", "g_Authenticity \"2\"");
            File.WriteAllText(serverConfig, serverConfigData);

            Console.WriteLine("----------------------------");
            Console.WriteLine($"OpenJK GameData Path: {openjkPath}");
            Console.WriteLine($"Dedicated Server Application: {serverConfig}");
            Console.WriteLine($"Client Application: {clientEXE}");
            Console.WriteLine("----------------------------");
            Console.WriteLine(" ");
            Console.WriteLine("Available Custom Maps");
            Console.WriteLine("----------------------------");
            Console.WriteLine(" ");

            foreach (string folder in Directory.GetDirectories(workingDir))
            {


                if (!Path.GetFileName(folder).StartsWith("."))
                {
                    maps.Add(folder);
                    Console.WriteLine($"{i}. {Path.GetFileName(folder)}");
                    i++;
                }
                
            }

            do
            {
                Console.Write("Please Choose Map: ");
                choice = Console.ReadLine();

                int.TryParse(choice, out mapChoice);

                if (mapChoice == 0  || mapChoice  > maps.Count)
                {
                    Console.WriteLine($"Invalid Selection");
                }

            }
            while (mapChoice == 0 || mapChoice > maps.Count);

            map = maps[mapChoice-1];

            Console.WriteLine($"Launching {Path.GetFileName(map)}");

            Console.WriteLine($"Creating PK3 For {Path.GetFileName(map)}");

            var finalDestination = Path.Combine(openjkPath, "MBII", Path.GetFileName(map) + ".pk3");

            if (File.Exists(finalDestination))
            {
                File.Delete(finalDestination);
            }

            ZipFile.CreateFromDirectory(map, finalDestination) ;

            if (Directory.Exists(Path.GetDirectoryName(finalDestination)))
            {

         
                Console.WriteLine($"Launching Dedicated Server");


                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    System.Diagnostics.Process.Start(clientEXE, "+ set fs_game \"MBII\" +connect 127.0.0.1:29071");
                }).Start();

                var startinfo = new ProcessStartInfo();
                startinfo.FileName = dedicatedEXE;
                startinfo.Arguments = $"+set dedicated 2 +set net_port 29071 +set fs_game \"MBII\" + exec \"server_config_default.cfg\" + set fs_direbeforepak \"1\" +set mbmode 2 +mbmode \"2\" +map \"{Path.GetFileName(map)}\"";

                var process = new Process();
                process.StartInfo = startinfo;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.UseShellExecute = false;

                process.Start();

                Thread.Sleep(20);

                // Return to MBMODE 0
                //serverConfigData = File.ReadAllText(serverConfig);
                //serverConfigData = serverConfigData.Replace("g_Authenticity \"2\"", "g_Authenticity \"0\"");
                //File.WriteAllText(serverConfig, serverConfigData);

                Console.ReadLine();

            }
            else
            {
                Console.WriteLine($"Directory Not found: {finalDestination}");

            }

            Console.ReadLine();

        }

        



    }
}
