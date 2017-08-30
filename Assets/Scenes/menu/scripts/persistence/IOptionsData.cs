using KeatsLib.Persistence;

public interface IOptionsData : IBasePersisting
{
	float fieldOfView { get; set; }
	float masterVolume { get; set; }
	float sfxVolume { get; set; }
	float musicVolume { get; set; }
	float mouseSensitivity { get; set; }
}
