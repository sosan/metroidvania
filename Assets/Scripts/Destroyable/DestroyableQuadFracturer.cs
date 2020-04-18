using UnityEngine;
using System.Collections.Generic;



namespace DestroyableObject
{
	public class DestroyableQuadFracturer : DestroyableFracturer
	{
		
		[Range(0.0f, 0.5f)]
		public float Irregularity = 0.25f;
		
		private static List<DestroyableQuad> quads  = new List<DestroyableQuad>();
		private static List<DestroyableVector2Operator> points = new List<DestroyableVector2Operator>();
		private static int quadCount;
		private static int pointCount;
		private static int xMin;
		private static int xMax;
		private static int yMin;
		private static int yMax;
		
		[ContextMenu("Fracture")]
		public override void Fracture()
		{
			base.Fracture();
			
			Fracture(destructible, FractureCount, Irregularity);
		}
		
		public static void Fracture(Destroyable destructible, int count, float irregularity)
		{
			if (destructible != null && count > 0)
			{
				DestroyableSplitGroup.ClearAll();
				{
					var width    = destructible.AlphaWidth;
					var height   = destructible.AlphaHeight;
					var mainQuad = new DestroyableQuad();

					quadCount  = 1;
					pointCount = 0;
					xMin       = 0;
					xMax       = width - 1;
					yMin       = 0;
					yMax       = height - 1;

					mainQuad.BL = new DestroyableVector2Operator(xMin, yMin);
					mainQuad.BR = new DestroyableVector2Operator(xMax, yMin);
					mainQuad.TL = new DestroyableVector2Operator(xMin, yMax);
					mainQuad.TR = new DestroyableVector2Operator(xMax, yMax);
					mainQuad.Calculate();

					if (quads.Count > 0)
					{
						quads[0] = mainQuad;
					}
					else
					{
						quads.Add(mainQuad);
					}

					for (var i = 0; i < count; i++)
					{
						SplitLargest();
					}

					if (irregularity > 0.0f)
					{
						FindPoints();
						ShiftPoints(irregularity);
					}

					for (var i = 0; i < quadCount; i++)
					{
						var quad  = quads[i];
						var group = DestroyableSplitGroup.GetSplitGroup();
					
						group.AddTriangle(quad.BL, quad.BR, quad.TL);
						group.AddTriangle(quad.TR, quad.TL, quad.BR);
					}

					destructible.SplitWhole(DestroyableSplitGroup.SplitGroups);
				}
				DestroyableSplitGroup.ClearAll();
			}
		}
		
		private static void FindPoints()
		{
			for (var i = 0; i < quadCount; i++)
			{
				var quad = quads[i];
				
				TryAddPoint(quad.BL);
				TryAddPoint(quad.BR);
				TryAddPoint(quad.TL);
				TryAddPoint(quad.TR);
			}
		}
		
		private static void ShiftPoints(float irregularity)
		{
			for (var i = 0; i < pointCount; i++)
			{
				var point  = points[i];
				var delta  = Random.insideUnitCircle.normalized * FindMaxMovement(point.X, point.Y) * irregularity;
				var deltaX = Mathf.RoundToInt(delta.x);
				var deltaY = Mathf.RoundToInt(delta.y);
				
				if (point.X <= xMin || point.X >= xMax) deltaX = 0;
				if (point.Y <= yMin || point.Y >= yMax) deltaY = 0;
				
				if (deltaX != 0 || deltaY != 0)
				{
					var newPoint = new DestroyableVector2Operator(point.X + deltaX, point.Y + deltaY);
					
					MovePoints(point, newPoint);
					
					points[i] = newPoint;
				}
			}
		}
		
		private static void MovePoints(DestroyableVector2Operator oldPoint, DestroyableVector2Operator newPoint)
		{
			for (var i = 0; i < quadCount; i++)
			{
				var quad = quads[i];
				
				TryMovePoint(ref quad.BL, oldPoint, newPoint);
				TryMovePoint(ref quad.BR, oldPoint, newPoint);
				TryMovePoint(ref quad.TL, oldPoint, newPoint);
				TryMovePoint(ref quad.TR, oldPoint, newPoint);
				
				quads[i] = quad;
			}
		}
		
		private static void TryMovePoint(ref DestroyableVector2Operator point, DestroyableVector2Operator oldPoint, DestroyableVector2Operator newPoint)
		{
			if (point.X == oldPoint.X && point.Y == oldPoint.Y)
			{
				point.X = newPoint.X;
				point.Y = newPoint.Y;
			}
		}
		
		private static void TryAddPoint(DestroyableVector2Operator newPoint)
		{
			for (var i = 0; i < pointCount; i++)
			{
				var point = points[i];
				
				if (point.X == newPoint.X && point.Y == newPoint.Y)
				{
					return;
				}
			}
			
			if (points.Count > pointCount)
			{
				points[pointCount] = newPoint;
			}
			else
			{
				points.Add(newPoint);
			}
			
			pointCount += 1;
		}
		
		private static int FindMaxMovement(int x, int y)
		{
			var minDistanceSq = int.MaxValue;
			
			for (var i = 0; i < pointCount; i++)
			{
				var point      = points[i];
				var distanceX  = point.X - x;
				var distanceY  = point.Y - y;
				var distanceSq = distanceX * distanceX + distanceY * distanceY;
				
				if (distanceSq > 0)
				{
					minDistanceSq = Mathf.Min(minDistanceSq, distanceSq);
				}
			}
			
			return Mathf.FloorToInt(Mathf.Sqrt(minDistanceSq));
		}
		
		private static void SplitLargest()
		{
			var largestIndex = 0;
			var largestArea  = 0;
			
			for (var i = 0; i < quadCount; i++)
			{
				var quad = quads[i];
				
				if (quad.Area > largestArea)
				{
					largestIndex = i;
					largestArea  = quad.Area;
				}
			}
			
			var first  = new DestroyableQuad();
			var second = new DestroyableQuad();
			
			quads[largestIndex].Split(ref first, ref second);
			
			quads[largestIndex] = first;
			
			if (quads.Count > quadCount)
			{
				quads[quadCount] = second;
			}
			else
			{
				quads.Add(second);
			}
			
			quadCount += 1;
		}
	}
}