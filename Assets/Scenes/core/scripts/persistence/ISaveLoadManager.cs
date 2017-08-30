using KeatsLib.Persistence;

public interface ISaveLoadManager
{
	Persistence persistentData { get; }
	void LoadData();
}
