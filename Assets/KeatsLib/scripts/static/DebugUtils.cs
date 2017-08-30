using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace KeatsLib.Unity
{
	public static class DebugExt
	{
		/// <summary>
		/// Prints a queue of logs accumulated over the course of the game.
		/// </summary>
		/// <param name="introMessage">A message to introduce the logging.</param>
		/// <param name="logQueue">The queue of logs.</param>
		public static void PrintLog(string introMessage, Queue<string> logQueue)
		{
			Debug.Log(introMessage + "; " + logQueue.Count + " logs.");

			if (logQueue.Count <= 0)
				return;

			string masterString = "";

			while (logQueue.Count > 0)
				masterString += logQueue.Dequeue() + '\n';

			Debug.Log(masterString);
		}

		/// <summary>
		/// Returns a string value of the current game time and system time.
		/// </summary>
		public static string TimeStamp()
		{
			return "\n" + "Game Time: " + Time.realtimeSinceStartup + " System Time: " + DateTime.Now;
		}

		/// <summary>
		/// CONDITIONAL: DEVELOPMENT_BUILD || UNITY_EDITOR
		/// Sets the name of a GameObject for testing purposes.
		/// </summary>
		/// <param name="g">The GameObject to change.</param>
		/// <param name="name">The new name to set the GameObject to.</param>
		[Conditional("DEVELOPMENT_BUILD")]
		[Conditional("UNITY_EDITOR")]
		public static void DebugSetGameObjectName(GameObject g, string name)
		{
			g.name = name;
		}
	}
}
