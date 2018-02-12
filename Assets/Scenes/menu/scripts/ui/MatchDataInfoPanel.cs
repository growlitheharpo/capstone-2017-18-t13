using UnityEngine;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	public class MatchDataInfoPanel : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private Text mMatchNameText;
		[SerializeField] private Text mMatchPlayerCountText;
		[SerializeField] private Button mJoinMatchButton;

		/// <summary>
		/// The name of the match for this panel.
		/// </summary>
		public string matchName { get { return mMatchNameText.text; } set { mMatchNameText.text = value; } }

		/// <summary>
		/// The current number of players in the match for this panel.
		/// </summary>
		public int matchCurrentPlayers { get; set; }

		/// <summary>
		/// The maximum number of players in the match for this panel.
		/// </summary>
		public int matchMaxPlayers { get; set; }

		/// <summary>
		/// The "Join Match" button for the match for this panel.
		/// </summary>
		public Button joinMatchButton { get { return mJoinMatchButton; } }

		/// <summary>
		/// Call after setting current and max players to refresh the UI element.
		/// </summary>
		public void RefreshPlayerString()
		{
			mMatchPlayerCountText.text = string.Format("{0}/{1} Players", matchCurrentPlayers, matchMaxPlayers);
		}
	}
}
