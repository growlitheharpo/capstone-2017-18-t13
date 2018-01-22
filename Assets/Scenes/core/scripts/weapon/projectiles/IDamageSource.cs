namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Interface for anything that causes damage.
	/// </summary>
	public interface IDamageSource
	{
		/// <summary>
		/// The character that is enacting this damage.
		/// </summary>
		ICharacter source { get; }
	}
}
