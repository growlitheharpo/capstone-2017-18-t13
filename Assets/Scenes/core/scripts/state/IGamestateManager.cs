public interface IGamestateManager
{
	bool isAlive { get; }
	void RequestShutdown();
}
