using UnityEngine;

namespace DestroyableObject
{
	[System.Serializable]
	public class DestroyableSnapshot
	{
		public Rect AlphaRect;
		
		public byte[] AlphaData;
		
		public int AlphaWidth;
		
		public int AlphaHeight;
	}
}