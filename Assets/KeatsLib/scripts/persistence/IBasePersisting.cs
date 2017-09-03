namespace KeatsLib.Persistence
{
	/// <summary>
	/// Base class for all persisting data.
	/// </summary>
	public interface IBasePersisting
	{
		/// <summary>
		/// Gets this Persistence instance.
		/// </summary>
		Persistence persistence { get; }

		/// <summary>
		/// Gets the constant ID for this persisting object.
		/// </summary>
		string id { get; }
	}
}
