using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
namespace DestroyableObject
{
	public class DestroyableCollider_Editor<T> : DestroyableEditor<T>
		where T : DestroyableCollider
	{
		protected override void OnInspector()
		{
			var updateColliderSettings = false;

			DrawDefault("IsTrigger", ref updateColliderSettings);
			DrawDefault("Material", ref updateColliderSettings);

			if (updateColliderSettings == true) DirtyEach(t => t.UpdateColliderSettings());
		}
	}
}
#endif

namespace DestroyableObject
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Destroyable))]
	public abstract class DestroyableCollider : MonoBehaviour
	{
		public bool IsTrigger;

		public PhysicsMaterial2D Material;
		[SerializeField] protected GameObject child;
		[SerializeField] protected bool awoken;
		[System.NonSerialized] protected Destroyable destructible;

		public abstract void UpdateColliderSettings();

		[System.NonSerialized]
		private GameObject tempChild;


		//por si acaso
		[ContextMenu("Regenerate")]
		public void Regenerate()
		{
			OnAlphaDataReplaced();
		}

		public void DestroyChild()
		{
			if (child != null)
			{
				child = DestroyableHelper.Destroy(child);
			}
		}

		protected virtual void OnEnable()
		{
			if (destructible == null) destructible = GetComponent<Destroyable>();
			if (destructible.OnAlphaDataReplaced == null) destructible.OnAlphaDataReplaced = new DestroyableEvent();
			if (destructible.OnAlphaDataModified == null) destructible.OnAlphaDataModified = new DestroyableRectEvent();
			if (destructible.OnAlphaDataSubset == null) destructible.OnAlphaDataSubset = new DestroyableRectEvent();
			if (destructible.OnStartSplit == null) destructible.OnStartSplit = new DestroyableEvent();
			if (destructible.OnEndSplit == null) destructible.OnEndSplit = new DestroyableListEvent();

			destructible.OnAlphaDataReplaced.AddListener(OnAlphaDataReplaced);
			destructible.OnAlphaDataModified.AddListener(OnAlphaDataModified);
			destructible.OnAlphaDataSubset.AddListener(OnAlphaDataSubset);
			destructible.OnStartSplit .AddListener(OnStartSplit);
			destructible.OnEndSplit.AddListener(OnEndSplit);

			if (child != null)
			{
				child.SetActive(true);
			}
		}

		protected virtual void OnDisable()
		{
			destructible.OnAlphaDataReplaced.RemoveListener(OnAlphaDataReplaced);
			destructible.OnAlphaDataModified.RemoveListener(OnAlphaDataModified);
			destructible.OnAlphaDataSubset.RemoveListener(OnAlphaDataSubset);
			destructible.OnStartSplit .RemoveListener(OnStartSplit);
			destructible.OnEndSplit.RemoveListener(OnEndSplit);

			if (child != null)
			{
				child.SetActive(false);
			}

			if (destructible.IsOnStartSplit == true)
			{
				if (child != null)
				{
					child.transform.SetParent(null, false);

					child = DestroyableHelper.Destroy(child);
				}

				if (tempChild != null)
				{
					tempChild = DestroyableHelper.Destroy(tempChild);
				}
			}
		}

		protected virtual void Awake()
		{
			if (this.GetComponent<Collider2D>() != null)
			{
				var collider2Ds = this.GetComponents<Collider2D>();

				for (var i = collider2Ds.Length - 1; i >= 0; i--)
				{
					DestroyableHelper.Destroy(collider2Ds[i]);
				}
			}
		}

		protected virtual void Start()
		{
			if (awoken == false)
			{
				awoken = true;

				OnAlphaDataReplaced();
			}
		}

		protected virtual void Update()
		{
			if (child == null)
			{
				OnAlphaDataReplaced();
			}

		}

		protected virtual void OnDestroy()
		{
			DestroyChild();
		}

		protected virtual void OnAlphaDataReplaced()
		{
			UpdateBeforeBuild();
		}

		protected virtual void OnAlphaDataModified(DestroyableRect rect)
		{
			UpdateBeforeBuild();
		}

		protected virtual void OnAlphaDataSubset(DestroyableRect rect)
		{
			UpdateBeforeBuild();
		}

		protected virtual void OnStartSplit()
		{
			if (child != null)
			{
				child.transform.SetParent(null, false);

				tempChild = child;
				child = null;
			}
		}

		protected virtual void OnEndSplit(List<Destroyable> clones)
		{
			ReconnectChild();
		}

		private void UpdateBeforeBuild()
		{
			if (destructible == null) destructible = GetComponent<Destroyable>();

			if (child == null)
			{
				ReconnectChild();

				if (child == null)
				{
					child = new GameObject("Collider");

					child.layer = transform.gameObject.layer;

					child.transform.SetParent(transform, false);
				}
			}

			if (destructible.AlphaIsValid == true)
			{
				var offsetX = destructible.AlphaRect.x;
				var offsetY = destructible.AlphaRect.y;
				var scaleX= destructible.AlphaRect.width / destructible.AlphaWidth;
				var scaleY= destructible.AlphaRect.height / destructible.AlphaHeight;

				child.transform.localPosition = new Vector3(offsetX, offsetY, 0.0f);
				child.transform.localScale= new Vector3(scaleX, scaleY, 0.0f);
			}
		}

		private void ReconnectChild()
		{
			if (tempChild != null)
			{
				child = tempChild;

				child.transform.SetParent(transform, false);

				tempChild = null;
			}
		}
	}
}