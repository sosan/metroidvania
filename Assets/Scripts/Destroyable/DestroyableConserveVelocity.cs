using UnityEngine;
using System.Collections.Generic;

namespace DestroyableObject
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Destroyable))]
	[RequireComponent(typeof(Rigidbody2D))]
	public class DestroyableConserveVelocity : MonoBehaviour
	{
		[System.NonSerialized]
		private Rigidbody2D body;

		[System.NonSerialized]
		private Destroyable destructible;

		[System.NonSerialized]
		private float angularVelocity;

		[System.NonSerialized]
		private Vector2 velocity;

		protected virtual void OnEnable()
		{
			if (destructible == null) destructible = GetComponent<Destroyable>();
			if (destructible.OnStartSplit == null) destructible.OnStartSplit = new DestroyableEvent();
			if (destructible.OnEndSplit == null) destructible.OnEndSplit   = new DestroyableListEvent();

			destructible.OnStartSplit.AddListener(StartSplit);
			destructible.OnEndSplit.AddListener(EndSplit);
		}

		protected virtual void OnDisable()
		{
			destructible.OnStartSplit.RemoveListener(StartSplit);
			destructible.OnEndSplit.RemoveListener(EndSplit);
		}

		protected virtual void StartSplit()
		{
			if (body == null) body = GetComponent<Rigidbody2D>();

			velocity        = body.velocity;
			angularVelocity = body.angularVelocity;
		}

		protected virtual void EndSplit(List<Destroyable> clones)
		{
			for (var i = clones.Count - 1; i >= 0; i--)
			{
				var clone = clones[i];

				if (clone.gameObject != gameObject)
				{
					var cloneRigidbody2D = clone.GetComponent<Rigidbody2D>();

					if (cloneRigidbody2D != null)
					{
						cloneRigidbody2D.velocity        = velocity;
						cloneRigidbody2D.angularVelocity = angularVelocity;
					}
				}
			}
		}
	}
}
