using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SlackNotifier
{
	class Program
	{
		static int Main(string[] args)
		{
			// GENERAL: https://hooks.slack.com/services/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
			// PRIVATE: https://hooks.slack.com/services/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
			const string url = "https://hooks.slack.com/services/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";

			Console.WriteLine("Enter the tag number: ");
			string data = Console.ReadLine();

			var task = Git("show --quiet " + data, 8000);
			task.Wait();

			var gitResult = task.Result;
			var lines = gitResult.Split('\n');

			var tagger = lines[1];
			var tagTitle = lines[4];

			List<string> otherData = new List<string>();
			for (int i = 5; i < lines.Length; i++)
			{
				if (lines[i].ToLower().StartsWith("commit"))
					break;
				otherData.Add(lines[i]);
			}

			var text = string.Format(
				"*New tag created: _{3}: {0}_*\n" +
				"{1}\n" +
				"\n{2}\n",
				tagTitle, tagger, string.Join("\n", otherData), data);

			Console.WriteLine("Preview:");
			Console.WriteLine(text);
			Console.Write("Send? (y/n) ");
			var send = Console.ReadLine();
			if (send.ToLower() != "y" && send.ToLower() != "yes")
				return 0;

			var json =
				"{\"text\":\""+ text + "\",\"username\":\"git\",\"icon_emoji\":\":git:\"}";

			var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "POST";

			using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{
				streamWriter.Write(json);
				streamWriter.Flush();
				streamWriter.Close();
			}

			var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
			{
				var result = streamReader.ReadToEnd();
			}

			return 0;
		}

		public static async Task<string> Git(string args, int bufferSize = 2048)
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
