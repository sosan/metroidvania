using UnityEngine;
using System.Collections.Generic;

namespace DestroyableObject
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Destroyable))]
	public abstract class DestroyableFracturer : MonoBehaviour
	{
		public int RemainingFractures = 1;
		public float RequiredDamage = 100.0f;
		public float RequiredDamageMultiplier = 2.0f;
		public int FractureCount = 5;
		public float FractureCountMultiplier = 0.75f;
		
		[System.NonSerialized]
		protected Destroyable destructible;
		
		[ContextMenu("Fracture")]
		public virtual void Fracture()
		{
			RemainingFractures -= 1;
			RequiredDamage *= RequiredDamageMultiplier;
			FractureCount = Mathf.CeilToInt(FractureCount * FractureCountMultiplier);

			if (destructible == null) destructible = GetComponent<Destroyable>();
		}

		public void UpdateFracture()
		{
			if (RemainingFractures > 0)
			{
				if (destructible == null) destructible = GetComponent<Destroyable>();

				if (destructible.Damage >= RequiredDamage)
				{
					Fracture();
				}
			}
		}

		protected virtual void OnEnable()
		{
			if (destructible == null) destructible = GetComponent<Destroyable>();
			if (destructible.OnDamageChanged == null) destructible.OnDamageChanged = new DestroyableFloatFloatEvent();

			destructible.OnDamageChanged.AddListener(OnDamageChanged);
		}

		protected virtual void OnDisable()
		{
			destructible.OnDamageChanged.RemoveListener(OnDamageChanged);
		}

		private void OnDamageChanged(float oldDamage, float newDamage)
		{
			UpdateFracture();
		}
	}
}
