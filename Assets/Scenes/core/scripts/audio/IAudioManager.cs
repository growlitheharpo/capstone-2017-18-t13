using UnityEngine;
using AudioEvent = AudioManager.AudioEvent;

public interface IAudioManager
{
	void InitializeDatabase();

	IAudioReference PlaySound(AudioEvent e, IAudioProfile profile, Transform location);
	IAudioReference PlaySound(AudioEvent e, IAudioProfile profile, Transform location, Vector3 offset);

	IAudioReference CheckReferenceAlive(ref IAudioReference reference);
}
