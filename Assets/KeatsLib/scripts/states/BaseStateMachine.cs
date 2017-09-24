namespace KeatsLib.State
{
	public class BaseStateMachine
	{
		protected abstract class BaseState<T> : IState where T : BaseStateMachine
		{
			protected T mMachine;

			protected BaseState(T machine)
			{
				mMachine = machine;
			}

			public virtual void OnEnter() { }
			public virtual void Update() { }
			public virtual void OnExit() { }
			public abstract IState GetTransition();
		}

		protected class NullState : BaseState<BaseStateMachine>
		{
			public NullState() : base(null) {}

			public override IState GetTransition()
			{
				return this;
			}
		}

		private IState mCurrentState, mPushedState;
		protected IState currentState { get { return mCurrentState; } }

		protected virtual void Update()
		{
			if (mPushedState != null)
			{
				mPushedState.Update();
				return;
			}

			if (mCurrentState == null)
				return;

			mCurrentState.Update();
			IState transition = mCurrentState.GetTransition();

			if (transition != null && transition != mCurrentState)
				TransitionStates(transition);
		}

		protected void TransitionStates(IState newState)
		{
			if (mCurrentState != null)
				mCurrentState.OnExit();

			mCurrentState = newState;
			mCurrentState.OnEnter();
		}

		protected void PushState(IState newState)
		{
			mPushedState = newState;
			mPushedState.OnEnter();
		}

		protected void PopState()
		{
			if (mPushedState != null)
				mPushedState.OnExit();
			mPushedState = null;
		}
	}
}
