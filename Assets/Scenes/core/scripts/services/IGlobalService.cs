namespace FiringSquad.Core
{
	/// <summary>
	/// An interface without any functions to assure type-safety in the ServiceLocator.
	/// Inheriting from this explicitly states that this service will be global.
	/// </summary>
	public interface IGlobalService
	{
	}
}
