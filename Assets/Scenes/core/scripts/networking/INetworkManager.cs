namespace FiringSquad.Gameplay
{
	public interface INetworkManager
	{
		bool isGameClient { get; }
		bool isGameServer { get; }
		bool isGameHost { get; }
	}
}
