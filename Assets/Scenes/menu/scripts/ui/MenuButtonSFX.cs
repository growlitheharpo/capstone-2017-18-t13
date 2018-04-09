using FiringSquad.Core;
using FiringSquad.Core.Audio;
using UnityEngine;
using UnityEngine.Serialization;

public class MenuButtonSFX : MonoBehaviour {

	/// Inspector variables
	[SerializeField] [FormerlySerializedAs("playButton")] private bool mPlayButton;
	
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
			mButtonPressSound.SetParameter("IsPlayButton", mPlayButton ? 1f : 0f);
			mButtonPressSound.Start();
		}
	}
}
