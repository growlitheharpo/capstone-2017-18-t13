using System.Diagnostics;

namespace UnityEditor
{
	/// <summary>
	/// Launch a git process from within the Unity Editor.
	/// </summary>
	public static class GitProcess
	{
		/// <summary>
		/// Create a git process at the current application path.
		/// </summary>
		/// <param name="args">The command and its arguments. Example: "commit -m \"A message.\""</param>
		/// <param name="bufferSize">The buffer size for scanning STDOUT of the git process.</param>
		/// <returns>What the git process outputted to STDOUT, as one string.</returns>
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

		/// <summary>
		/// Create a git process at the current application path.
		/// </summary>
		/// <param name="path">The working directory to launch the git process.</param>
		/// <param name="args">The command and its arguments. Example: "commit -m \"A message.\""</param>
		/// <param name="bufferSize">The buffer size for scanning STDOUT of the git process.</param>
		/// <returns>What the git process outputted to STDOUT, as one string.</returns>
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
