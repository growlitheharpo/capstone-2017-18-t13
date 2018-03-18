using UnityEngine;
using UnityEngine.Playables;

namespace FiringSquad.Gameplay.Timeline
{
	/// <summary>
	/// Class for handling the intro manager and enabling it properly.
	/// </summary>
	public class IntroManagerPlayable : MonoBehaviour
	{
		/// <summary>
		/// Unity's Start function
		/// </summary>
		private void Start()
		{
			EventManager.Local.OnReceiveStartIntroNotice += OnReceiveStartIntroNotice;
		}

		/// <summary>
		/// Unity's OnDestroy function
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Local.OnReceiveStartIntroNotice -= OnReceiveStartIntroNotice;
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnReceiveStartIntroNotice
		/// </summary>
		private void OnReceiveStartIntroNotice()
		{
			GetComponent<PlayableDirector>().Play();
		}
	}
}
