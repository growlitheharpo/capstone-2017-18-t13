/// <summary>
/// Interface for objects that will be stored in a GameObjectPool.
/// </summary>
public interface IPoolable
{
	/// <summary>
	/// Called when an object is about to be released from a pool.
	/// Object has been selected and verified for release, but has not had its
	/// position adjusted and SetActive(true) has not been called.
	/// </summary>
	void PreSetup();

	/// <summary>
	/// Called immediately after an object is released from a pool.
	/// Object has been selected and verified, has had its position adjusted,
	/// and SetActive(true) has been called.
	/// </summary>
	void PostSetup();

	/// <summary>
	/// Called immediately before an object is returned to the pool.
	/// Object has been verified to be a pool member, but its position
	/// has not been adjusted and SetActive(false) has not been called.
	/// </summary>
	void PreDisable();

	/// <summary>
	/// Called immediately after an object is returned to the pool.
	/// Object has been verified to be a pool member, its position has
	/// been adjusted, and SetActive(false) has been called.
	/// </summary>
	void PostDisable();
}
