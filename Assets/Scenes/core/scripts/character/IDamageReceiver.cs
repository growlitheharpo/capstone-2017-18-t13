using UnityEngine;

namespace FiringSquad.Gameplay
{
	public interface IDamageReceiver
	{
		void ApplyDamage(float amount, Vector3 point, IDamageSource cause);
	}
}
