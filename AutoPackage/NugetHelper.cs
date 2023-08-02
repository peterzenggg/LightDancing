using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace AutoPackage
{
    public class NugetHelper
    {
        private readonly string _dllPath;

        public NugetHelper(string dllPath)
        {
            _dllPath = dllPath;
        }

        public void Upload()
        {
            DeletePreviousPackage();
            CreateNugetPackage();

            FTPPackage pack = new FTPPackage
            {
                Server = "ftp://waws-prod-dm1-167.ftp.azurewebsites.windows.net/site/wwwroot/Packages",
                File = GetPackagePath(),
                Credentials = new NetworkCredential("apps-nuget\\$apps-nuget", "kGQtbtKoRugR9ib6g0wcPrxRpYPT2ruEY5tabaLTKKXd3q232S5xSyJ3jkxx")
            };
            pack.Upload();
        }

        /// <summary>
        /// Delete previous package
        /// </summary>
        private void DeletePreviousPackage()
        {
            List<string> fileFolders = new List<string> { "bin", "obj" };

            foreach (string fileFolder in fileFolders)
            {
                string binFilePath = string.Format("{0}\\{1}", _dllPath, fileFolder);
                if (Directory.Exists(binFilePath))
                {
                    DirectoryInfo binFile = new DirectoryInfo(binFilePath);
                    binFile.Delete(true);
                    Console.WriteLine(string.Format("Delete previous {0} file", fileFolder));
                }
            }
        }

        private void CreateNugetPackage()
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.WorkingDirectory = _dllPath;
            cmd.Start();
            cmd.StandardInput.WriteLine("dotnet pack");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
        }

        private FileInfo GetPackagePath()
        {
            string savePath = string.Format("{0}\\bin\\Debug", _dllPath);

            if (!Directory.Exists(savePath))
            {
                throw new Exception(string.Format("The directory not found in: {0}", savePath));
            }

            DirectoryInfo info = new DirectoryInfo(savePath);
            FileInfo files = info.GetFiles().FirstOrDefault((FileInfo x) => x.Name.Contains(".nupkg"));

            if (files != null)
                return files;

            throw new Exception(string.Format("The nupkg not found in: {0}", savePath));
        }
    }
}
