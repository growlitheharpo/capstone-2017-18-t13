using System;

public interface IGameConsole
{
	void AssertCheatsEnabled();
	void RegisterCommand(string command, Action<string[]> handle);

	Logger.System enabledLogLevels { get; }
}
