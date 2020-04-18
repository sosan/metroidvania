using UnityEngine;
using System.Collections.Generic;



namespace DestroyableObject
{
	[DisallowMultipleComponent]
	public class DestroyableFixture : MonoBehaviour
	{
		public Vector3 Offset;

		[System.NonSerialized]
		private Destroyable destructible;

		protected virtual void OnEnable()
		{
			if (destructible == null) destructible = GetComponentInParent<Destroyable>();
			if (destructible.OnStartSplit == null) destructible.OnStartSplit = new DestroyableEvent();
			if (destructible.OnEndSplit == null) destructible.OnEndSplit = new DestroyableListEvent();

			Hook();
		}

		protected virtual void OnDisable()
		{
			Unhook();
		}

		protected virtual void Update()
		{
			UpdateFixture();
		}



		private void UpdateFixture()
		{
			if (destructible == null) destructible = GetComponentInParent<Destroyable>();

			if (destructible == null)
			{
				DestroyFixture();
			}
			else
			{
				var worldPosition = transform.TransformPoint(Offset);

				if (destructible.SampleAlpha(worldPosition) < 0.5f)
				{
					DestroyFixture();
				}
			}
		}

		private void DestroyFixture()
		{
			DestroyableHelper.Destroy(gameObject);
		}

		private void OnStartSplit()
		{
			transform.SetParent(null, false);
		}

		private void OnEndSplit(List<Destroyable> clones)
		{
			for (var i = clones.Count - 1; i >= 0; i--)
			{
				var clone = clones[i];

				if (TryFixTo(clone) == true)
				{
					return;
				}
			}

			DestroyFixture();
		}

		private bool TryFixTo(Destroyable newDestructible)
		{
			var isDifferent = destructible != newDestructible;

			transform.SetParent(newDestructible.transform, false);

			var worldPosition = transform.TransformPoint(Offset);

			if (newDestructible.SampleAlpha(worldPosition) > 0.5f)
			{
				if (isDifferent == true)
				{
					Unhook();

					destructible = newDestructible;

					Hook();
				}

				return true;
			}

			transform.SetParent(destructible.transform, false);

			return false;
		}

		private void Hook()
		{
			destructible.OnStartSplit.AddListener(OnStartSplit);
			destructible.OnEndSplit.AddListener(OnEndSplit);
		}

		private void Unhook()
		{
			destructible.OnStartSplit.RemoveListener(OnStartSplit);
			destructible.OnEndSplit.RemoveListener(OnEndSplit);
		}
	}
}
