using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace DestroyableObject
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(DestroyableEdgeCollider))]
	public class DestroyableEdgeCollider_Editor : DestroyableCollider_Editor<DestroyableEdgeCollider>
	{
		protected override void OnInspector()
		{
			var regenerate = false;

			DrawDefault("CellSize", ref regenerate);
			DrawDefault("Detail", ref regenerate);
			DrawDefault("Weld", ref regenerate);

			if (regenerate == true) DirtyEach(t => t.Regenerate());

			base.OnInspector();
		}
	}
}
#endif

namespace DestroyableObject
{
	public class DestroyableEdgeCollider : DestroyableCollider
	{
		[Range(8, 256)]
		public int CellSize = 256;

		[Range(0.5f, 1.0f)]
		public float Detail = 0.9f;

		[Range(0.001f, 1.0f)]
		public float Weld = 0.01f;

		[SerializeField]
		private int expectedCellSize;

		[SerializeField]
		private int expectedWidth;

		[SerializeField]
		private int expectedHeight;

		[SerializeField]
		private int cellWidth;

		[SerializeField]
		private int cellHeight;

		[SerializeField]
		private DestroyableEdgeColliderCell[] cells;

		private static Stack<EdgeCollider2D> tempColliders = new Stack<EdgeCollider2D>();

		public override void UpdateColliderSettings()
		{
			if (cells != null)
			{
				for (var i = cells.Length - 1; i >= 0; i--)
				{
					var cell = cells[i];

					if (cell != null)
					{
						cell.UpdateColliderSettings(IsTrigger, Material);
					}
				}
			}
		}

		protected override void OnAlphaDataReplaced()
		{
			base.OnAlphaDataReplaced();
			
			Rebuild();
		}

		protected override void OnAlphaDataModified(DestroyableRect rect)
		{
			base.OnAlphaDataModified(rect);

			if (CellSize <= 0)
			{
				Mark(); Sweep(); return;
			}

			if (destructible.AlphaWidth != expectedWidth || destructible.AlphaHeight != expectedHeight || cells == null || cells.Length != cellWidth * cellHeight || CellSize != expectedCellSize)
			{
				Rebuild(); return;
			}

			var cellXMin = rect.MinX / CellSize;
			var cellYMin = rect.MinY / CellSize;
			var cellXMax = (rect.MaxX + 1) / CellSize;
			var cellYMax = (rect.MaxY + 1) / CellSize;

			cellXMin = Mathf.Clamp(cellXMin, 0, cellWidth  - 1);
			cellXMax = Mathf.Clamp(cellXMax, 0, cellWidth  - 1);
			cellYMin = Mathf.Clamp(cellYMin, 0, cellHeight - 1);
			cellYMax = Mathf.Clamp(cellYMax, 0, cellHeight - 1);

			for (var cellY = cellYMin; cellY <= cellYMax; cellY++)
			{
				var offset = cellY * cellWidth;

				for (var cellX = cellXMin; cellX <= cellXMax; cellX++)
				{
					var index = cellX + offset;

					RebuildCell(ref cells[index], cellX, cellY);
				}
			}

			Sweep();
		}

		protected override void OnAlphaDataSubset(DestroyableRect rect)
		{
			base.OnAlphaDataSubset(rect);

			Rebuild();
		}

		protected override void OnStartSplit()
		{
			base.OnStartSplit();
			
			Mark();
			Sweep();
		}

		private void Mark()
		{
			tempColliders.Clear();

			if (cells != null)
			{
				for (var i = cells.Length - 1; i >= 0; i--)
				{
					var cell = cells[i];

					if (cell != null)
					{
						cell.Clear(tempColliders);

						cells[i] = DestroyableEdgeColliderCell.Add(cell);
					}
				}
			}
		}

		private void Sweep()
		{
			while (tempColliders.Count > 0)
			{
				DestroyableHelper.Destroy(tempColliders.Pop());
			}
		}

		private void Rebuild()
		{
			Mark();
			{
				if (CellSize > 0)
				{
					expectedCellSize = CellSize;
					expectedWidth    = destructible.AlphaWidth;
					expectedHeight   = destructible.AlphaHeight;
					cellWidth        = (expectedWidth  + CellSize - 1) / CellSize;
					cellHeight       = (expectedHeight + CellSize - 1) / CellSize;
					cells            = new DestroyableEdgeColliderCell[cellWidth * cellHeight];

					for (var cellY = 0; cellY < cellHeight; cellY++)
					{
						var offset = cellY * cellWidth;

						for (var cellX = 0; cellX < cellWidth; cellX++)
						{
							RebuildCell(ref cells[cellX + offset], cellX, cellY);
						}
					}
				}
			}
			Sweep();
		}

		private void RebuildCell(ref DestroyableEdgeColliderCell cell, int cellX, int cellY)
		{
			var xMin = CellSize * cellX;
			var yMin = CellSize * cellY;
			var xMax = Mathf.Min(CellSize + xMin, destructible.AlphaWidth );
			var yMax = Mathf.Min(CellSize + yMin, destructible.AlphaHeight);

			if (xMin > 0) xMin += 1;
			if (yMin > 0) yMin += 1;
			if (xMax < destructible.AlphaWidth ) xMax -= 1;
			if (yMax < destructible.AlphaHeight) yMax -= 1;

			DestroyableColliderBuilder.AlphaData   = destructible.AlphaData;
			DestroyableColliderBuilder.AlphaWidth  = destructible.AlphaWidth;
			DestroyableColliderBuilder.AlphaHeight = destructible.AlphaHeight;
			DestroyableColliderBuilder.MinX        = xMin;
			DestroyableColliderBuilder.MinY        = yMin;
			DestroyableColliderBuilder.MaxX        = xMax;
			DestroyableColliderBuilder.MaxY        = yMax;

			DestroyableColliderBuilder.CalculateEdgeCells();

			if (cell == null)
			{
				cell = DestroyableEdgeColliderCell.Get();
			}

			cell.Clear(tempColliders);

			DestroyableColliderBuilder.BuildEdge(cell, tempColliders, child, Weld, Detail);

			cell.UpdateColliderSettings(IsTrigger, Material);
		}
	}
}