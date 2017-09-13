using UnityEngine;

namespace FiringSquad.Gameplay
{
	public interface IWeaponBearer : ICharacter
	{
		void ApplyRecoil(Vector3 direction, float amount);
	}
}
