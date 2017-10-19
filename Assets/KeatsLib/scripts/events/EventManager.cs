using System;
using System.Collections.Generic;
using System.Diagnostics;
using FiringSquad.Core;
using FiringSquad.Core.Input;
using FiringSquad.Debug;
using Debug = UnityEngine.Debug;

/// <summary>
/// A manager for a custom Event system within Unity.
/// </summary>
public partial class EventManager : MonoSingleton<EventManager>
{
	private static Queue<Action> kEventsFromLastFrame;

	public static event Action OnInitiateXmlRefresh = () => { LogEvent(); };

	public static void InitiateXmlRefresh()
	{
		OnInitiateXmlRefresh();
	}

	public static event Action OnXmlSuccesfullyRefreshed = () => { LogEvent(); };

	public static void XmlSuccesfullyRefreshed()
	{
		OnXmlSuccesfullyRefreshed();
	}

	public static event Action<InputLevel, bool> OnInputLevelChanged = (a, b) => { LogEvent(); };

	public static void InputLevelChanged(InputLevel level, bool state)
	{
		OnInputLevelChanged(level, state);
	}

	/// <summary>
	/// Instantiate the EventManager.
	/// </summary>
	protected override void Awake()
	{
		base.Awake();

		// 15 is a magic number to avoid any memory allocs/frees during runtime
		kEventsFromLastFrame = new Queue<Action>(15);
	}

	/// <summary>
	/// Invoke an event next frame.
	/// </summary>
	/// <para>
	/// The best way to invoke any events that are of a different type than BaseEvent
	/// is through enclosure. I.e., InvokeNextFrame(() => EventManager.ComplicatedEvent(1, 24, null, EnumVal));
	/// </para>
	/// <param name="e"></param>
	public static void Notify(Action e)
	{
		kEventsFromLastFrame.Enqueue(e);
	}

	/// <summary>
	/// Process all the events we received through Notify() last frame.
	/// </summary>
	private void Update()
	{
		// Cache our number of events to prevent a potentially infinite loop if one event triggers another.
		int initialCount = kEventsFromLastFrame.Count;

		for (int i = 0; i < initialCount; ++i)
		{
			Action e = kEventsFromLastFrame.Dequeue();
			try
			{
				e.Invoke();
			}
			catch (Exception except)
			{
				Debug.LogException(except);
			}
		}
	}

	/// <summary>
	/// Event log that only fires in certain conditions.
	/// </summary>
	[Conditional("DEBUG")]
	[Conditional("DEVELOPMENT_BUILD")]
	private static void LogEvent()
	{
		var stackFrames = new StackTrace().GetFrames();
		if (stackFrames == null)
			throw new NullReferenceException("Stack frame was null while logging event!");

		string eventName = stackFrames[2].GetMethod().Name;

		try
		{
			// ReSharper disable once PossibleNullReferenceException
			string callerName = stackFrames[stackFrames.Length - 2].GetMethod().DeclaringType.Name;
			string callerFunc = stackFrames[stackFrames.Length - 2].GetMethod().Name;

			Logger.Info(string.Format("Event {0} fired from {1}::{2}", eventName, callerName, callerFunc), Logger.System.Event);
		}
		catch (Exception)
		{
			Logger.Info("Event " + eventName + " fired from unknown source.", Logger.System.Event);
		}
	}
}

/// <summary>
/// This attribute is just used for non-comment
/// documentation in code.
/// It marks a function as an event handler.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class EventHandlerAttribute : Attribute { }
