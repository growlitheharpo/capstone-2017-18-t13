using FiringSquad.Core;
using FiringSquad.Core.Audio;
using UnityEngine;
using UnityEngine.Playables;

namespace FiringSquad.Gameplay.Timeline
{
	/// <summary>
	/// Class for handling the intro manager and enabling it properly.
	/// </summary>
	public class IntroManagerPlayable : MonoBehaviour
	{	
		/// Private variables
		private IAudioReference mIntroMusic;

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

			// play music
			IAudioManager audioService = ServiceLocator.Get<IAudioManager>();
			mIntroMusic = audioService.CheckReferenceAlive(ref mIntroMusic);

			if (mIntroMusic == null)
			{
				mIntroMusic = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.IntroMusic, gameObject.transform , false);
				mIntroMusic.Start();
			}
		}
	}
}
