namespace KeatsLib.Persistence
{
	/// <summary>
	/// Base class for all persisting data.
	/// </summary>
	public interface IBasePersisting
	{
		Persistence persistence { get; }
		string id { get; }
	}
}
