namespace FiringSquad.Core.Audio
{
	/// <summary>
	/// A simple interface for maintaining a reference to a currently-playing audio.
	/// Used to prevent code outside of the audio system directly manipulating audio sources.
	/// </summary>
	public interface IAudioReference
	{
		/// <summary>
		/// Immediately stop playing the sound.
		/// </summary>
		void Kill();

		/// <summary>
		/// Slowly fade out the sound.
		/// </summary>
		/// <param name="time">Length of time to fade out over.</param>
		void FadeOut(float time);

		/// <summary>
		/// Set whether or not the audio should repeat.
		/// </summary>
		void SetRepeat(bool repeat);

		/// <summary>
		/// Set the volume of the sound to a new level.
		/// </summary>
		void SetVolume(float vol);

		/// <summary>
		/// Set the pitch of the sound to a new level.
		/// </summary>
		void SetPitch(float pitch);

		/// <summary>
		/// Returns true if the audio is currently playing.
		/// </summary>
		bool isPlaying { get; }
	}
}
