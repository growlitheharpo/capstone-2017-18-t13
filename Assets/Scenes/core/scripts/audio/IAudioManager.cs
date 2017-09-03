using UnityEngine;
using AudioEvent = AudioManager.AudioEvent;

/// <summary>
/// The public interface for the Audio Manager service.
/// </summary>
public interface IAudioManager
{
	/// <summary>
	/// Used to instantiate all of the sounds at startup.
	/// </summary>
	void InitializeDatabase();

	/// <summary>
	/// Start a sound based on an event.
	/// </summary>
	/// <param name="e">The event that has occurred.</param>
	/// <param name="profile">The profile that the event is linked to.</param>
	/// <param name="location">The location of the event.</param>
	/// <returns>An IAudioReference to the new sound.</returns>
	IAudioReference PlaySound(AudioEvent e, IAudioProfile profile, Transform location);

	/// <summary>
	/// Start a sound based on an event.
	/// </summary>
	/// <param name="e">The event that has occurred.</param>
	/// <param name="profile">The profile that the event is linked to.</param>
	/// <param name="location">The location of the event.</param>
	/// <param name="offset">The offset from the location to place the sound.</param>
	/// <returns>An IAudioReference to the new sound.</returns>
	IAudioReference PlaySound(AudioEvent e, IAudioProfile profile, Transform location, Vector3 offset);

	/// <summary>
	/// Check if a reference is still playing. Will set the reference to null if it is not.
	/// </summary>
	/// <param name="reference">The reference to check.</param>
	/// <returns>The passed reference.</returns>
	IAudioReference CheckReferenceAlive(ref IAudioReference reference);
}
