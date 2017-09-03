/// <summary>
/// The base interface for an Audio Profile.
/// </summary>
/// 
/// <para>
/// Audio Profiles are concepts attached to characters or settings.
/// They can have base profiles that they extend or override.
/// For example, there might be a BaseGuardProfile that provides
/// a footstep noise and a shout noise. Then, the SpecializedGuardProfile
/// could define only the shout noise and, when used in game, would
/// find its footstep noise in its parent or its parent's parents.
/// </para>
public interface IAudioProfile
{
	/// <summary>
	/// The unique ID for this audio profile.
	/// </summary>
	string id { get; }

	/// <summary>
	/// Gets whether the list of clips this profile provides
	/// should be played all at once, or chosen from randomly.
	/// </summary>
	AudioManager.ProfileType profile { get; }

	/// <summary>
	/// Get all clips associated with this AudioProfile.
	/// </summary>
	IAudioClip[] GetAllClips();

	/// <summary>
	/// Get a clip for a particular event for this AudioProfile.
	/// </summary>
	/// <param name="e">The event to check for.</param>
	/// <returns>The list of associated audio clips. Can be empty.</returns>
	IAudioClip[] GetClip(AudioManager.AudioEvent e);
	
	/// <summary>
	/// Get a clip for a particular event for this AudioProfile or its parents.
	/// </summary>
	/// <param name="e">The event to check for.</param>
	/// <returns>The list of associated audio clips. Can be empty.</returns>
	IAudioClip[] GetClipInParents(AudioManager.AudioEvent e);
}
