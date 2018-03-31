using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Core.State;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtonSFX : MonoBehaviour {

	/// Inspector variables
	[SerializeField] private bool playButton;
	
	/// Private variables
	private IAudioReference mButtonHoverSound;
	private IAudioReference mButtonPressSound;

	public void PlayHoverSound()
	{
		IAudioManager audioService = ServiceLocator.Get<IAudioManager>();
		mButtonHoverSound = audioService.CheckReferenceAlive(ref mButtonHoverSound);

		if (mButtonHoverSound == null)
		{
			mButtonHoverSound = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.MenuButtonHover, gameObject.transform, false);
			mButtonHoverSound.Start();
		}
	}

	public void PlayPressSound()
	{
		IAudioManager audioService = ServiceLocator.Get<IAudioManager>();
		mButtonPressSound = audioService.CheckReferenceAlive(ref mButtonPressSound);

		if (mButtonPressSound == null)
		{
			mButtonPressSound = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.MenuButtonPress, gameObject.transform, false);
			if (playButton)
			{
				mButtonPressSound.SetParameter("IsPlayButton", 1f);
			} else
			{
				mButtonPressSound.SetParameter("IsPlayButton", 0f);
			}
			mButtonPressSound.Start();
		}
	}
}
