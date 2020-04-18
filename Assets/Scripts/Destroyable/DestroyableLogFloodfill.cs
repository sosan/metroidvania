using System.Collections.Generic;

namespace DestroyableObject
{
	public static partial class DestroyableLogFloodfill
	{
		public class Line
		{
			public bool   Used;
			public int    Y;
			public int    MinX;
			public int    MaxX;
			public Island Island;

			public List<Line> Ups = new List<Line>();
			public List<Line> Dns = new List<Line>();
		}
	}
}