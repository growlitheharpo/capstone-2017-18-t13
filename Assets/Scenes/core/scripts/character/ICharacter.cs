using UnityEngine;

/// <summary>
/// Base Character interface.
/// </summary>
public interface ICharacter
{
	GameObject gameObject { get; }
	Transform eye { get; }

	bool isCurrentPlayer { get; }
}
