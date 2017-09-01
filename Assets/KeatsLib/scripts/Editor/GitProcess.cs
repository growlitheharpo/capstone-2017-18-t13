using System.Diagnostics;

namespace UnityEditor
{
	public static class GitProcess
	{
		public static string Launch(string args, int bufferSize = 2048)
		{
			using (Process p = new Process())
			{
				p.StartInfo.CreateNoWindow = true;
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.FileName = "git";
				p.StartInfo.Arguments = args;
				p.Start();

				var buffer = new char[bufferSize];
				p.StandardOutput.Read(buffer, 0, bufferSize);
				p.WaitForExit();

				return new string(buffer).TrimEnd('\n', '\0');
			}
		}
		
		public static string Launch(string path, string args, int bufferSize = 2048)
		{
			using (Process p = new Process())
			{
				p.StartInfo.CreateNoWindow = true;
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.WorkingDirectory = path;
				p.StartInfo.FileName = "git";
				p.StartInfo.Arguments = args;
				p.Start();

				var buffer = new char[bufferSize];
				p.StandardOutput.Read(buffer, 0, bufferSize);
				p.WaitForExit();

				return new string(buffer).TrimEnd('\n', '\0');
			}
		}
	}
}
