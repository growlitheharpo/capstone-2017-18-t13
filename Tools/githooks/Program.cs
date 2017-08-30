using System;
using System.Linq;
using System.Threading.Tasks;

namespace precommit
{
	public class Program
	{
		private static int Main()
		{
			var filesStatus = GitAsyncUtil.Git("status --p -uno").Result.Split('\n');
			using (var res =
				Task.WhenAll(
					UserPolicy.DoCheckUser(),
					FilePolicy.EnforceAssetFileNamingPatterns(filesStatus),
					FilePolicy.RejectCapitalizedFolders(filesStatus),
					FilePolicy.EnsureNewFilesHaveMeta(filesStatus),
					FilePolicy.RejectProjectVersionTxt(filesStatus),
					FilePolicy.RejectDefaultMaterialFiles(filesStatus)
				))
			{
				res.Wait();

				bool failed = res.Result.Any(x => x.Failed);

				if (failed)
				{
					Console.Error.WriteLine("************************************************************");
					Console.Error.WriteLine("*********     Firing Squad Commit Check Failed     *********\n");

					var failures = res.Result.Where(x => x.Failed).ToArray();
					for (int i = 0; i < failures.Length; i++)
					{
						Console.Error.WriteLine("Failure {0}:", i + 1);
						Console.Error.WriteLine(failures[i].Message + "\n");
					}
					Console.Error.WriteLine("\n************************************************************");
				}

#if DEBUG
				Console.Write("Press any key to continue...");
				Console.ReadKey(true);
#endif

				return failed ? -1 : 0;
			}
		}
	}
}
