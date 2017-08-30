using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager>, IAudioManager
{
	public enum ProfileType
	{
		ChooseRandom,
		PlayAll,
	}

	public enum AudioEvent
	{
		TestExplosionEffect = -1,
		MainBackgroundEffect = 0,
	}

	private class AudioReference : IAudioReference
	{
		public List<IAudioClip> mClipData;
		public List<AudioSource> mSources;

		public void Kill()
		{
			instance.StopSoundImmediate(this);
		}

		public void FadeOut(float time)
		{
			throw new System.NotImplementedException("Fading audio in and out is not yet supported!");
		}

		public void SetRepeat(bool repeat)
		{
			foreach (AudioSource source in mSources)
				source.loop = repeat;
		}
	}

	[SerializeField] private AudioDatabase mAudioDatabase;
	[SerializeField] private bool mShouldSelfInitialize;
	private HashSet<IAudioReference> mCurrentSounds;

	protected override void Awake()
	{
		base.Awake();
		mCurrentSounds = new HashSet<IAudioReference>();
	}

	private void Start()
	{
		if (!ServiceLocator.Get<IGamestateManager>().isAlive && mShouldSelfInitialize)
			InitializeDatabase();
	}
	
	public void InitializeDatabase()
	{
		mAudioDatabase.InitializePrefabs(transform);
		EventManager.Notify(EventManager.InitialAudioLoadComplete);
	}

	// Update is called once per frame
	private void Update()
	{
		ProcessFinishedAudio();
	}

	private void ProcessFinishedAudio()
	{
		var refsToDelete = new List<IAudioReference>();
		foreach (IAudioReference reference in mCurrentSounds)
		{
			AudioReference r = reference as AudioReference;
			if (r == null || r.mSources.Count == 0)
			{
				refsToDelete.Add(reference);
				continue;
			}

			for (int i = 0; i < r.mSources.Count; i++)
			{
				// if !finished
				if (r.mSources[i].loop || !(r.mSources[i].time >= r.mSources[i].clip.length - Time.deltaTime))
					continue;

				Destroy(r.mSources[i].gameObject);
				r.mSources.RemoveAt(i);
				r.mClipData.RemoveAt(i);
			}
		}

		foreach (IAudioReference r in refsToDelete)
			mCurrentSounds.Remove(r);
	}

	public IAudioReference PlaySound(AudioEvent e, IAudioProfile profile, Transform location)
	{
		return PlaySound(e, profile, location, Vector3.zero);
	}

	public IAudioReference PlaySound(AudioEvent e, IAudioProfile profile, Transform location, Vector3 offset)
	{
		var clips = profile.GetClipInParents(e);
		var sources = new List<AudioSource>();

		foreach (IAudioClip clip in clips)
		{
			GameObject newGameObject = Instantiate(mAudioDatabase.GetPrefab(profile, clip));
			newGameObject.name += "__" + e;
			if (clip.playAtSource)
			{
				newGameObject.transform.SetParent(location);
				newGameObject.transform.localPosition = offset;
			}

			AudioSource s = newGameObject.GetComponent<AudioSource>();
			sources.Add(s);

			s.Play();
		}

		//TODO: Remove this!
		if (clips.Any(x => x.fadeInTime > 0.0f || x.fadeOutTime > 0.0f))
			Logger.Warn("Fading audio in and out is not supported yet!", Logger.System.Audio);

		AudioReference newRef = new AudioReference { mClipData = clips.ToList(), mSources = sources };
		mCurrentSounds.Add(newRef);
		return newRef;
	}

	public IAudioReference CheckReferenceAlive(ref IAudioReference reference)
	{
		if (reference == null || mCurrentSounds.Contains(reference))
			return reference;

		reference = null;
		return null;
	}

	public void StopSoundImmediate(IAudioReference sound)
	{
		if (!mCurrentSounds.Contains(sound))
			return;

		AudioReference soundImpl = sound as AudioReference;
		if (soundImpl == null)
			return;

		foreach (AudioSource source in soundImpl.mSources)
		{
			source.Stop();
			Destroy(source.gameObject);
		}

		soundImpl.mSources.Clear();
		soundImpl.mClipData.Clear();
	}
}
