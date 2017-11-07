using UnityEngine;

namespace FiringSquad.Core.Audio
{
	/// <summary>
	/// A simple interface for maintaining a reference to a currently-playing audio.
	/// Used to prevent code outside of the audio system directly manipulating audio sources.
	/// </summary>
	public interface IAudioReference
	{
		/// <summary>
		/// Start playing the actual sound.
		/// </summary>
		IAudioReference Start();

		/// <summary>
		/// Immediately stop playing the sound.
		/// </summary>
		IAudioReference Kill(bool allowFade = true);

		/// <summary>
		/// Set the volume of the sound to a new level.
		/// </summary>
		IAudioReference SetVolume(float vol);

		/// <summary>
		/// Sets the sound to follow a GameObject.
		/// </summary>
		/// <param name="rb">The rigidbody to attach to.</param>
		IAudioReference AttachToRigidbody(Rigidbody rb);
		
		/// <summary>
		/// Returns true if the audio is currently playing.
		/// </summary>
		bool isPlaying { get; }

		// Common variables:

		float playerSpeed { get; set; }
		float weaponType { get; set; }

		IAudioReference SetParameter(string name, float value);
		float GetParameter(string name);
	}
}
