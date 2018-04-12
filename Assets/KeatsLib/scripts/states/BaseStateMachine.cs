using System.Collections.Generic;

namespace KeatsLib.State
{
	/// <summary>
	/// Base StateMachine class.
	/// Handles core functions like transitioning between different states.
	/// Inherit and add internal private classes for the states.
	/// </summary>
	/// <para>
	/// State Machine Flow: When Update() is called, the current state's Update is called.
	/// Immediately after this, we call GetTransition on the current state. If it returns anything
	/// other than itself or null, we transition to the new state that it provided immediately.
	/// </para>
	/// <para>
	/// Can function as a "Push Down Automata" or a standard state machine. Use "TransitionState"
	/// for the latter, and "PushState" and "PopState" for the former.
	/// </para>
	public abstract class BaseStateMachine
	{
		/// <summary>
		/// Abstract base class with utility functions to avoid empty functions in states.
		/// Inherit from in order to avoid having to define OnEnter, Update, and/or OnExit for states that don't need them.
		/// </summary>
		/// <typeparam name="T">The concrete type of the holder state machine.</typeparam>
		protected abstract class BaseState<T> : IState where T : BaseStateMachine
		{
			protected T mMachine;

			protected BaseState(T machine)
			{
				mMachine = machine;
			}

			/// <inheritdoc />
			public virtual void OnEnter() { }

			/// <inheritdoc />
			public virtual void Update() { }

			/// <inheritdoc />
			public virtual void OnExit() { }

			/// <inheritdoc />
			public abstract IState GetTransition();
		}

		/// <summary>
		/// A state that does absolutely nothing. Useful if you
		/// need to put your state machine in a "holding pattern".
		/// </summary>
		protected class NullState : BaseState<BaseStateMachine>
		{
			public NullState() : base(null) { }

			/// <inheritdoc />
			public override IState GetTransition()
			{
				return this;
			}
		}

		private readonly Stack<IState> mCurrentStates = new Stack<IState>(5);
		private IState currentState { get { return mCurrentStates.Count > 0 ? mCurrentStates.Peek() : null; } }

		/// <summary>
		/// Update the state machine. 
		/// </summary>
		/// <para>
		/// Runs Update on the current state, then calls GetTransition. If GetTransition returns
		/// a different state, OnExit is called on the current, OnEnter is called on the new one,
		/// and the new state is set as the current.
		/// </para>
		protected virtual void Update()
		{
			IState current = currentState;
			if (current == null)
				return;

			current.Update();
			IState transition = current.GetTransition();

			if (transition != null && transition != current)
				TransitionStates(transition);
		}

		/// <summary>
		/// Immediately pops every state from the machine
		/// </summary>
		public virtual void OnDestroy()
		{
			while (mCurrentStates.Count > 0)
				PopState();
		}

		/// <summary>
		/// Immediately transition to a new state by calling OnExit on the current
		/// and then OnEnter on the new one.
		/// </summary>
		/// <param name="newState">The new state that we are entering.</param>
		protected void TransitionStates(IState newState)
		{
			PopState();
			PushState(newState);
		}

		/// <summary>
		/// Push a new state onto the state machine's stack.
		/// Signals that we are currently functioning as a pushdown automata.
		/// </summary>
		/// <param name="newState"></param>
		protected void PushState(IState newState)
		{
			mCurrentStates.Push(newState);
			newState.OnEnter();
		}

		/// <summary>
		/// Remove a state from the state machine's stack.
		/// Signals that we are currently functioning as a pushdown automata.
		/// </summary>
		protected void PopState()
		{
			if (mCurrentStates.Count > 0)
			{
				IState current = currentState;
				if (current != null)
					current.OnExit();

				mCurrentStates.Pop();
			}
		}
	}
}
