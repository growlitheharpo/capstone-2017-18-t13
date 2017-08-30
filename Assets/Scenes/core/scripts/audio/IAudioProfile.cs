public interface IAudioProfile
{
	string id { get; }
	AudioManager.ProfileType profile { get; }

	IAudioClip[] GetAllClips();
	IAudioClip[] GetClip(AudioManager.AudioEvent e);
	IAudioClip[] GetClipInParents(AudioManager.AudioEvent e);
}
