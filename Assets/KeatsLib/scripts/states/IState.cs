namespace KeatsLib.State
{
	/// <summary>
	/// Interface used for a state.
	/// </summary>
	public interface IState
	{
		/// <summary>
		/// Called when state is first entered.
		/// </summary>
		void OnEnter();

		/// <summary>
		/// Called every time the holder State Machine's "Update" is called and we are active.
		/// </summary>
		void Update();

		/// <summary>
		/// Called when state is exited.
		/// </summary>
		void OnExit();

		/// <summary>
		/// The state this state wants to transition to. Returns null or this if we should stay as we are.
		/// </summary>
		IState GetTransition();
	}
}
