using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Interface for getting audio data. This data is "static" and saved in editor.
/// An AudioClip is the lowest level of information available about a sound.
/// </summary>
public interface IAudioClip
{
	/// <summary>
	/// The mixer group for this audio clip (sfx, music, etc.)
	/// </summary>
	AudioMixerGroup group { get; }

	/// <summary>
	/// The actual sound file for this audio clip.
	/// </summary>
	AudioClip sound { get; }

	/// <summary>
	/// Whether this clip should bypass effects.
	/// </summary>
	bool bypassEffects { get; }

	/// <summary>
	/// Whether this clip should loop.
	/// </summary>
	bool looping { get; }

	/// <summary>
	/// The audio priority of this clip.
	/// </summary>
	int priority { get; }

	/// <summary>
	/// The volume this clip should be played at within its MixerGroup.
	/// </summary>
	float volume { get; }

	/// <summary>
	/// The pitch this clip should be modded to.
	/// </summary>
	float pitch { get; }

	/// <summary>
	/// The stereo pan of this clip.
	/// </summary>
	float stereoPan { get; }

	/// <summary>
	/// The 2D-3D blend of this clip. 0.0 is fully 2D, 1.0 is fully 3D.
	/// </summary>
	float spatialBlend { get; }

	/// <summary>
	/// The audio rolloff type of this clip.
	/// </summary>
	AudioRolloffMode rolloffMode { get; }

	/// <summary>
	/// Minimum distance for rolloff to begin for this clip.
	/// </summary>
	float minDistance { get; }

	/// <summary>
	/// Maximum distance that this sound can be heard at.
	/// </summary>
	float maxDistance { get; }

	/// <summary>
	/// The fade-in time for this clip.
	/// </summary>
	float fadeInTime { get; }

	/// <summary>
	/// The fade-out time for this clip.
	/// </summary>
	float fadeOutTime { get; }

	/// <summary>
	/// Whether this clip should be played at its source.
	/// Generally, the source is a GameObject with an IAudioProfile.
	/// </summary>
	bool playAtSource { get; }
}
