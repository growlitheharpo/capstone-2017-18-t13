using KeatsLib.Persistence;

/// <summary>
/// Base interface for the Save/Load service.
/// </summary>
public interface ISaveLoadManager
{
	/// <summary>
	/// Get the master persistent data instance.
	/// </summary>
	Persistence persistentData { get; }

	/// <summary>
	/// Load our data from disk (or create a new instance if no file is found).
	/// </summary>
	void LoadData();
}
