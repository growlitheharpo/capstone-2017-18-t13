using System;
using System.Linq;
using System.Threading.Tasks;

namespace precommit
{
	public static class UserPolicy
	{
		private static readonly string[] POSSIBLE_NAMES =
		{
			"Charlie Carucci",
			"Charles Carucci",
			"James Keats",
			"Max Sanel",
			"Tyler Bolster"
		};

		private static readonly string[] POSSIBLE_EMAILS =
		{
			"charles.carucci@mymail.champlain.edu",
			"james.keats@mymail.champlain.edu",
			"max.sanel@mymail.champlain.edu",
			"tyler.bolster@mymail.champlain.edu"
		};

		public static async Task<PrehookResult> DoCheckUser()
		{
			var res = Task.WhenAll(
				GitAsyncUtil.Git("config --get user.name"),
				GitAsyncUtil.Git("config --get user.email"));
			await res;

			string name = res.Result[0];
			string email = res.Result[1];

			bool allowed = POSSIBLE_EMAILS.Any(e => string.Equals(e, email, StringComparison.CurrentCultureIgnoreCase)) &&
							POSSIBLE_NAMES.Any(n => string.Equals(n, name, StringComparison.CurrentCulture));

			return allowed
				? new PrehookResult(true)
				: new PrehookResult(false,
					"Your email or user name for git is invalid!\n" +
					"Run git config --local user.name \"Your Name\" and\n"
					+ "git config --local user.email your.name@mymail.champlain.edu");
		}
	}
}
