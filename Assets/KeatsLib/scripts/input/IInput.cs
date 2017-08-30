using System;
using InputLevel = KeatsLib.Unity.Input.InputLevel;

public interface IInput
{
	IInput RegisterInput<T>(Func<T, bool> method, T key, Action command, InputLevel level, bool allowOtherKeys = true);
	IInput UnregisterInput(Action command);

	IInput RegisterAxis(Func<string, float> method, string axis, Action<float> command, InputLevel level, bool allowOtherAxes = true);
	IInput UnregisterAxis(Action<float> command);
	
	void SetInputLevel(InputLevel level);
	void SetInputLevelState(InputLevel level, bool state);
	void EnableInputLevel(InputLevel level);
	void DisableInputLevel(InputLevel level);
	bool IsInputEnabled(InputLevel level);
}
