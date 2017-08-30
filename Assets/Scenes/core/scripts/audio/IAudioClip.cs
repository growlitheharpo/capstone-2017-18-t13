using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Interface for getting audio data. This data is "static".
/// </summary>
public interface IAudioClip
{
	AudioMixerGroup group { get; }
	AudioClip sound { get; }
	bool bypassEffects { get; }
	bool looping { get; }
	int priority { get; }
	float volume { get; }
	float pitch { get; }
	float stereoPan { get; }
	float spatialBlend { get; }
	AudioRolloffMode rolloffMode { get; }
	float minDistance { get; }
	float maxDistance { get; }

	float fadeInTime { get; }
	float fadeOutTime { get; }

	bool playAtSource { get; }
}
