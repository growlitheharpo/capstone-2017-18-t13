using System.Diagnostics;
using System.Threading.Tasks;

namespace precommit
{
	internal static class GitAsyncUtil
	{
		public static async Task<string> Git(string args, int bufferSize = 1024 * 64)
		{
			using (Process p = new Process())
			{
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.FileName = "git";
				p.StartInfo.Arguments = args;
				p.Start();

				var buffer = new char[bufferSize];
				await p.StandardOutput.ReadAsync(buffer, 0, bufferSize);
				p.WaitForExit();

				return new string(buffer).TrimEnd('\n', '\0');
			}
		}
	}
}
