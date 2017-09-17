﻿using UnityEngine;

public class BaseStateMachine : MonoBehaviour
{
	protected interface IState
	{
		void OnEnter();
		void Update();
		void OnExit();
		IState GetTransition();
	}

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

	private IState mCurrentState;
	protected IState currentState { get { return mCurrentState; } }

	protected virtual void Update()
	{
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
}
