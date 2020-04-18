using UnityEngine;
using System.Collections.Generic;

namespace DestroyableObject
{
	[System.Serializable]
	public class DestroyableEdgeColliderCell
	{
		public List<EdgeCollider2D> Colliders;

		public static Stack<DestroyableEdgeColliderCell> pool = new Stack<DestroyableEdgeColliderCell>();

		public static DestroyableEdgeColliderCell Add(DestroyableEdgeColliderCell cell)
		{
			pool.Push(cell);

			return null;
		}

		public static DestroyableEdgeColliderCell Get()
		{
			if (pool.Count > 0)
			{
				return pool.Pop();
			}

			return new DestroyableEdgeColliderCell();
		}

		public EdgeCollider2D AddPath(Stack<EdgeCollider2D> tempColliders, GameObject child, Vector2[] points)
		{
			var collider = default(EdgeCollider2D);

			if (tempColliders.Count > 0)
			{
				collider = tempColliders.Pop();
			}
			else
			{
				collider = child.AddComponent<EdgeCollider2D>();
			}

			collider.points = points;

			if (Colliders == null)
			{
				Colliders = new List<EdgeCollider2D>();
			}

			Colliders.Add(collider);

			return collider;
		}

		public void Clear(Stack<EdgeCollider2D> tempColliders)
		{
			if (Colliders != null)
			{
				for (var i = Colliders.Count - 1; i >= 0; i--)
				{
					var collider = Colliders[i];

					if (collider != null)
					{
						tempColliders.Push(collider);
					}
				}

				Colliders.Clear();
			}
		}

		public void UpdateColliderSettings(bool isTrigger, PhysicsMaterial2D material)
		{
			if (Colliders != null)
			{
				for (var i = Colliders.Count - 1; i >= 0; i--)
				{
					var collider = Colliders[i];

					if (collider != null)
					{
						collider.isTrigger = isTrigger;
						collider.sharedMaterial = material;
					}
				}
			}
		}
	}
}