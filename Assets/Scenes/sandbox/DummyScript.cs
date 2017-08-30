using System;
using UnityEngine;
using Input = KeatsLib.Unity.Input;
using UnityInput = UnityEngine.Input;

public class DummyScript : MonoBehaviour
{
	[Serializable]
	private struct Data
	{
		[ComponentTypeRestriction(typeof(IAudioProfile))] [SerializeField] private ScriptableObject mAudioProfile;
		public IAudioProfile audioProfile { get { return mAudioProfile as IAudioProfile; } }
	}

	[SerializeField] private Data mData;

	private IAudioReference mMainMusicRef;
	private IAudioManager mAudioRef;

	// Use this for initialization
	private void Start()
	{
		ServiceLocator.Get<IInput>()
			.RegisterInput(UnityInput.GetKeyDown, KeyCode.Space, INPUT_StartTestExplosionEffect, Input.InputLevel.None)
			.RegisterInput(UnityInput.GetKeyDown, KeyCode.Return, INPUT_StartMusic, Input.InputLevel.None)
			.RegisterInput(UnityInput.GetKeyDown, KeyCode.X, INPUT_StopMusic, Input.InputLevel.None);
		mAudioRef = ServiceLocator.Get<IAudioManager>();
	}

	private void INPUT_StartTestExplosionEffect()
	{
		mAudioRef.PlaySound(AudioManager.AudioEvent.TestExplosionEffect, mData.audioProfile, transform);
	}

	private void INPUT_StartMusic()
	{
		if (mAudioRef.CheckReferenceAlive(ref mMainMusicRef) == null)
			mMainMusicRef = mAudioRef.PlaySound(AudioManager.AudioEvent.MainBackgroundEffect, mData.audioProfile, transform);
	}

	private void INPUT_StopMusic()
	{
		if (mAudioRef.CheckReferenceAlive(ref mMainMusicRef) != null)
			mMainMusicRef.Kill();
	}
}
