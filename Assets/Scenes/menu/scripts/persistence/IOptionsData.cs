namespace FiringSquad.Data
{
	/// <summary>
	/// Main game options.
	/// </summary>
	public interface IOptionsData
	{
		/// <summary>
		/// The player's camera field of view.
		/// </summary>
		float fieldOfView { get; set; }

		/// <summary>
		/// The master volume of the game.
		/// </summary>
		float masterVolume { get; set; }

		/// <summary>
		/// The volume of the effect mixer.
		/// </summary>
		float sfxVolume { get; set; }

		/// <summary>
		/// The volume of the music mixer.
		/// </summary>
		float musicVolume { get; set; }

		/// <summary>
		/// The mouse sensitivity.
		/// </summary>
		float mouseSensitivity { get; set; }
	}
}
