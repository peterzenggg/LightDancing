using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutoPackage
{
    class Program
    {
        private const string SCRIPT_CREATE_CONFIG = "auto.package.taskrunner.config";
        private const string SCRIPT_UPLOAD_NUGET = "auto.package.nugetpackage.upload";

        static void Main(string[] args)
        {
            string solutionPath = GetSolutionPath();

            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case SCRIPT_CREATE_CONFIG:
                        CreateTaskRunnerScripts(solutionPath);
                        break;

                    case SCRIPT_UPLOAD_NUGET:
                        string dllPath = string.Format("{0}\\LightDancing", solutionPath);
                        NugetHelper nugetHelper = new NugetHelper(dllPath);
                        nugetHelper.Upload();

                        break;

                    default:
                        Console.WriteLine(string.Format("The args of {0}, not hande...", args[0]));
                        break;
                }
            }
            else
            {
                CreateTaskRunnerScripts(solutionPath);
            }           
        }

        private static void CreateTaskRunnerScripts(string solutionPath)
        {           
            string configPath = string.Format("{0}\\AutoPackage\\commands.json", solutionPath);
            List<string> argsName = new List<string>
            {
                SCRIPT_CREATE_CONFIG,
                SCRIPT_UPLOAD_NUGET,
            };

            TaskRunnerScript script = new TaskRunnerScript(configPath, argsName);
            script.Create();
        }

        private static string GetSolutionPath(string currentPath = null)
        {
            DirectoryInfo directory = new DirectoryInfo(currentPath ?? Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any<FileInfo>())
            {
                directory = directory.Parent;
            }
            return directory.FullName;
        }
    }
}
