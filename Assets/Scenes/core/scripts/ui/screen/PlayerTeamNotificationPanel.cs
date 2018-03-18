using FiringSquad.Data;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// Utility UI class to show the player which team they've been assigned to.
	/// </summary>
	public class PlayerTeamNotificationPanel : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private UIText mLabel1;
		[SerializeField] private UIText mTeamName;
		[SerializeField] private UIText mLabel2;

		/// <summary>
		/// Unity's Start function
		/// </summary>
		private void Start()
		{
			EventManager.LocalGUI.OnLocalPlayerAssignedTeam += OnLocalPlayerAssignedTeam;
			gameObject.SetActive(false);
		}

		/// <summary>
		/// Unity's OnDestroy function
		/// </summary>
		private void OnDestroy()
		{
			EventManager.LocalGUI.OnLocalPlayerAssignedTeam -= OnLocalPlayerAssignedTeam;
		}

		/// <summary>
		/// EVENT HANDLER: LocalGUI.OnLocalPlayerAssignedTeam
		/// </summary>
		private void OnLocalPlayerAssignedTeam(CltPlayer localPlayer)
		{
			GameData.PlayerTeam team = localPlayer.playerTeam;
			mTeamName.text = team.ToString().ToUpper();

			// If we're in deathmatch mode, replace "YOU'RE ON THE (x) TEAM"
			// with "THIS IS A DEATHMATCH GAME"
			if (team == GameData.PlayerTeam.Deathmatch)
			{
				mLabel1.text = "THIS IS A";
				mLabel2.text = "GAME";
			}
		}
	}
}
