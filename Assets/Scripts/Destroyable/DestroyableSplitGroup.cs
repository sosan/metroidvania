using UnityEngine;
using System.Collections.Generic;

namespace DestroyableObject
{
	public class DestroyableSplitGroup
	{
		public static List<DestroyableSplitGroup> SplitGroups = new List<DestroyableSplitGroup>();

		public List<DestroyableSplitPixel> Pixels = new List<DestroyableSplitPixel>();

		public byte[] Data;

		public DestroyableRect Rect;

		public static DestroyableSplitGroup GetSplitGroup()
		{
			var splitGroup = DestroyablePool<DestroyableSplitGroup>.Spawn() ?? new DestroyableSplitGroup();

			SplitGroups.Add(splitGroup);

			return splitGroup;
		}

		public static void ClearAll()
		{
			for (var i = SplitGroups.Count - 1; i >= 0; i--)
			{
				var splitGroup = SplitGroups[i];

				splitGroup.Clear();

				DestroyablePool<DestroyableSplitGroup>.Despawn(splitGroup);
			}

			SplitGroups.Clear();
		}

		public void GenerateData()
		{
			var width   = Rect.SizeX;
			var offsetX = Rect.MinX;
			var offsetY = Rect.MinY;

			Data = new byte[Rect.Area];

			for (var i = Pixels.Count - 1; i >= 0; i--)
			{
				var pixel = Pixels[i];
				var x     = pixel.X - offsetX;
				var y     = pixel.Y - offsetY;

				Data[x + y * width] = pixel.Alpha;
			}
		}

		public void CombineData(byte[] prevData, int prevWidth, int prevHeight)
		{
			var dataX      = Rect.MinX;
			var dataY      = Rect.MinY;
			var dataWidth  = Rect.SizeX;
			var dataHeight = Rect.SizeY;

			if (Data != null && Data.Length >= dataWidth * dataHeight && prevData != null && prevData.Length >= prevWidth * prevHeight)
			{
				for (var y = 0; y < dataHeight; y++)
				{
					for (var x = 0; x < dataWidth; x++)
					{
						var prevX = x + dataX;
						var prevY = y + dataY;
						var dataI = x + y * dataWidth;

						if (prevX >= 0 && prevY >= 0 && prevX < prevWidth && prevY < prevHeight)
						{
							var prevI = prevX + prevY * prevWidth;
							var dataA = DestroyableHelper.ConvertAlpha(    Data[dataI]);
							var prevA = DestroyableHelper.ConvertAlpha(prevData[prevI]);

							Data[dataI] = DestroyableHelper.ConvertAlpha(dataA * prevA);
						}
						else
						{
							Data[dataI] = 0;
						}
					}
				}
			}
		}

		public void AddPixel(int x, int y)
		{
			var pixel = DestroyablePool<DestroyableSplitPixel>.Spawn() ?? new DestroyableSplitPixel();

			pixel.Alpha = 255;
			pixel.X     = x;
			pixel.Y     = y;

			Rect.Add(x, y);

			Pixels.Add(pixel);
		}

		public void AddIsland(DestroyableLogFloodfill.Island island)
		{
			for (var i = island.Lines.Count - 1; i >= 0; i--)
			{
				var line = island.Lines[i];

				for (var j = line.MinX; j < line.MaxX; j++)
				{
					AddPixel(j, line.Y);
				}
			}
		}

		public void AddTriangle(DestroyableVector2Operator a, DestroyableVector2Operator b, DestroyableVector2Operator c)
		{
			if (a.Y != b.Y || a.Y != c.Y)
			{
				//var z = (a.V + b.V + c.V) / 3.0f;
				//var z1 = Vector3.MoveTowards(a.V, z, 1.0f);
				//var z2 = Vector3.MoveTowards(b.V, z, 1.0f);
				//var z3 = Vector3.MoveTowards(c.V, z, 1.0f);
				
				//Debug.DrawLine(z1, z2, Color.red, 10.0f);
				//Debug.DrawLine(z2, z3, Color.red, 10.0f);
				//Debug.DrawLine(z3, z1, Color.red, 10.0f);
				
				
				if (b.Y > a.Y) DestroyableHelper.Swap(ref a, ref b);
				if (c.Y > a.Y) DestroyableHelper.Swap(ref c, ref a);
				if (c.Y > b.Y) DestroyableHelper.Swap(ref b, ref c);

				var fth = a.Y - c.Y; 
				var tth = a.Y - b.Y; 
				var bth = b.Y - c.Y;

				var inx = c.X + (a.X - c.X) * DestroyableHelper.Divide(bth, fth);
				var d   = new DestroyableVector2Operator((int)inx, b.Y);

				var abs = DestroyableHelper.Divide(a.X - b.X, tth);
				var ads = DestroyableHelper.Divide(a.X - d.X, tth);

				AddTriangle(b.X, d.X, abs, ads, b.Y, 1, tth);

				var cbs = DestroyableHelper.Divide(c.X - b.X, bth);
				var cds = DestroyableHelper.Divide(c.X - d.X, bth);

				AddTriangle(b.X, d.X, cbs, cds, b.Y, -1, bth);
			}
		}

		public void AddTriangle(float l, float r, float ls, float rs, int y, int s, int c)
		{
			if (l > r)
			{
				DestroyableHelper.Swap(ref l, ref r);
				DestroyableHelper.Swap(ref ls, ref rs);
			}

			for (var i = 0; i < c; i++)
			{
				var il = Mathf.FloorToInt(l);
				var ir = Mathf.CeilToInt(r);

				for (var x = il; x < ir; x++)
				{
					AddPixel(x, y);
				}

				y += s;
				l += ls;
				r += rs;
			}
		}

		public void Clear()
		{
			for (var i = Pixels.Count - 1; i >= 0; i--)
			{
				DestroyablePool<DestroyableSplitPixel>.Despawn(Pixels[i]);
			}

			Data = null;

			Rect.Clear();

			Pixels.Clear();
		}
	}
}