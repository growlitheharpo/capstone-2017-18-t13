using UnityEngine;

namespace FiringSquad.Core.Audio
{
	/// <summary>
	/// The public interface for the Audio Manager service.
	/// </summary>
	public interface IAudioManager : IGlobalService
	{
		/// <summary>
		/// Used to instantiate all of the sounds at startup.
		/// </summary>
		void InitializeDatabase();

		/// <summary>
		/// Start a sound based on an event.
		/// </summary>
		/// <param name="e">The event that has occurred.</param>
		/// <param name="location">The location of the event.</param>
		/// <param name="autoPlay">Whether the service should auto-start the event.</param>
		/// <returns>An IAudioReference to the new sound.</returns>
		IAudioReference CreateSound(AudioEvent e, Transform location, bool autoPlay = true);

		/// <summary>
		/// Start a sound based on an event.
		/// </summary>
		/// <param name="e">The event that has occurred.</param>
		/// <param name="location">The location of the event.</param>
		/// <param name="offset">The offset from the location to place the sound.</param>
		/// <param name="offsetType">Whether the offset is an offset from self, or an exact world position.</param>
		/// <param name="autoPlay">Whether the service should auto-start the event.</param>
		/// <returns>An IAudioReference to the new sound.</returns>
		IAudioReference CreateSound(AudioEvent e, Transform location, Vector3 offset, Space offsetType, bool autoPlay = true);

		/// <summary>
		/// Check if a reference is still playing. Will set the reference to null if it is not.
		/// </summary>
		/// <param name="reference">The reference to check.</param>
		/// <returns>The passed reference.</returns>
		IAudioReference CheckReferenceAlive(ref IAudioReference reference);
	}
}
