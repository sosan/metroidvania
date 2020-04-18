namespace DestroyableObject
{
	[System.Serializable]
	public struct DestroyableVector2Operator
	{
		public int X;

		public int Y;

		public DestroyableVector2Operator(int newX, int newY)
		{
			X = newX;
			Y = newY;
		}

		public static int DistanceSq(DestroyableVector2Operator a, DestroyableVector2Operator b)
		{
			var x = b.X - a.X;
			var y = b.Y - a.Y;

			return x * x + y * y;
		}

		public static DestroyableVector2Operator operator + (DestroyableVector2Operator a, DestroyableVector2Operator b)
		{
			a.X += b.X;
			a.Y += b.Y;

			return a;
		}

		public static DestroyableVector2Operator operator - (DestroyableVector2Operator a, DestroyableVector2Operator b)
		{
			a.X -= b.X;
			a.Y -= b.Y;

			return a;
		}

		public static DestroyableVector2Operator operator * (DestroyableVector2Operator a, float b)
		{
			a.X = (int)(a.X * b);
			a.Y = (int)(a.Y * b);

			return a;
		}

		public static DestroyableVector2Operator operator / (DestroyableVector2Operator a, int b)
		{
			a.X = a.X / b;
			a.Y = a.Y / b;

			return a;
		}

		public override bool Equals(System.Object o)
		{
			if (o is DestroyableVector2Operator)
			{
				var v = (DestroyableVector2Operator)o;

				return X == v.X && Y == v.Y;
			}

			return false;
		}

		public static bool operator ==(DestroyableVector2Operator a, DestroyableVector2Operator b)
		{
			return a.X == b.X && a.Y == b.Y;
		}

		public static bool operator !=(DestroyableVector2Operator a, DestroyableVector2Operator b)
		{
			return a.X != b.X || a.Y != b.Y;
		}

		public UnityEngine.Vector3 V
		{
			get
			{
				return new UnityEngine.Vector3(X, Y, 0.0f);
			}
		}

		public float Magnitude
		{
			get
			{
				return (float)System.Math.Sqrt(X * X + Y * Y);
			}
		}

		public override string ToString()
		{
			return " ("+ X + ", " + Y + ") ";
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}
	}
}
