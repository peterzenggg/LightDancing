using System;
using System.Collections.Generic;
using System.IO;

namespace AutoPackage
{
    public class TaskRunnerScript
    {
        private readonly string _configPath;

        private readonly List<string> _args;

        public TaskRunnerScript(string configPath, List<string> args)
        {
            _configPath = configPath;
            _args = args;
        }

        public void Create()
        {
            string[] config = GetConfigs();
            File.WriteAllLines(_configPath, config);
			Console.WriteLine("Config-created");
        }

		/// <summary>
		/// Create
		/// </summary>
		/// <returns></returns>
        private string[] GetConfigs()
        {
			List<string> result = new List<string>();
			string path = AppDomain.CurrentDomain.BaseDirectory;
			string name = AppDomain.CurrentDomain.FriendlyName;
			List<string> header = new List<string>
			{
				"{",
				"   \"commands\":",
				"   {"
			};
			List<string> bottom = new List<string>
			{
				"   }",
				"}"
			};
			List<string> body = new List<string>();
			foreach (string arg in _args)
			{
				body.Add(string.Format("       \"{0}\": ", arg));
				body.Add("       {");
				body.Add(string.Format("           \"fileName\": \"\\\"{0}{1}.exe\\\"\",", path.Replace("\\", "\\\\"), name));
				body.Add("           \"workingDirectory\": \".\",");
				body.Add(string.Format("\t\t    \"arguments\": \"\\\"{0}\\\"\"", arg));
				body.Add("       },");
			}
			result.AddRange(header);
			result.AddRange(body);
			result.AddRange(bottom);
			return result.ToArray();
		}
    }
}
