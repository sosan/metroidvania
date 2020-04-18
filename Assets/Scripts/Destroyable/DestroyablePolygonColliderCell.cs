using UnityEngine;
using System.Collections.Generic;

namespace DestroyableObject
{
	[System.Serializable]
	public class DestroyablePolygonColliderCell
	{
		public PolygonCollider2D Collider;

		public static Stack<DestroyablePolygonColliderCell> pool = new Stack<DestroyablePolygonColliderCell>();

		public static DestroyablePolygonColliderCell Add(DestroyablePolygonColliderCell cell)
		{
			pool.Push(cell);

			return null;
		}

		public static DestroyablePolygonColliderCell Get()
		{
			if (pool.Count > 0)
			{
				var cell = pool.Pop();

				return cell;
			}

			return new DestroyablePolygonColliderCell();
		}

		public void AddPolygon(Stack<PolygonCollider2D> tempColliders, GameObject child, Vector2[] points)
		{
			if (Collider == null)
			{
				if (tempColliders.Count > 0)
				{
					Collider = tempColliders.Pop();
				}
				else
				{
					Collider = child.AddComponent<PolygonCollider2D>();

					Collider.pathCount = 0;
				}
			}

			Collider.pathCount += 1;

			Collider.SetPath(Collider.pathCount - 1, points);
		}

		public void Clear(Stack<PolygonCollider2D> tempColliders)
		{
			if (Collider != null)
			{
				Collider.pathCount = 0;

				tempColliders.Push(Collider);

				Collider = null;
			}
		}

		public void UpdateColliderSettings(bool isTrigger, PhysicsMaterial2D material)
		{
			if (Collider != null)
			{
				Collider.isTrigger = isTrigger;
				Collider.sharedMaterial = material;
			}
		}
	}
}