using UnityEngine;
using System.Collections.Generic;

namespace DestroyableObject
{
	public static partial class DestroyableLogFloodfill
	{
		public class Island
		{
			public int MinX;
			public int MinY;
			public int MaxX;
			public int MaxY;
			public int Count;
			public List<Line> Lines = new List<Line>();
			private static DestroyableDistanceField distanceField = new DestroyableDistanceField();

			public void Clear()
			{
				Lines.Clear();
			}

			public void Submit(DestroyableDistanceField baseField, DestroyableSplitGroup splitGroup, DestroyableRect baseRect, DestroyableRect rect)
			{
				distanceField.Transform(rect, this);

				for (var y = rect.MinY; y < rect.MaxY; y++)
				{
					for (var x = rect.MinX; x < rect.MaxX; x++)
					{
						var cell     = distanceField.Cells[x - rect.MinX + (y - rect.MinY) * rect.SizeX];
						var baseCell = baseField.Cells[x - baseRect.MinX + (y - baseRect.MinY) * baseRect.SizeX];

						if (cell.D == baseCell.D)
						{
							splitGroup.AddPixel(x, y);
						}
					}
				}
			}
		}
	}
}