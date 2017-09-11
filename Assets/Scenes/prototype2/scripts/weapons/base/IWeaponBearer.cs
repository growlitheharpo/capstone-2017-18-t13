using UnityEngine;

public interface IWeaponBearer : ICharacter
{
	void ApplyRecoil(Vector3 direction, float amount);
}
