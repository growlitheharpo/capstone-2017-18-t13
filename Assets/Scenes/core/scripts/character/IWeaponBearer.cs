using UnityEngine;

namespace FiringSquad.Gameplay
{
	public interface IWeaponBearer : ICharacter
	{
		IWeapon weapon { get; }
		void ApplyRecoil(Vector3 direction, float amount);
	}
}
