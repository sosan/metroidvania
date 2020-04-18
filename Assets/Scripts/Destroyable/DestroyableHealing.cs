using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace DestroyableObject
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(DestroyableHealing))]
	public class DestroyableHealing_Editor : DestroyableEditor<DestroyableHealing>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.DelayPerHeal < 0.0f));
				DrawDefault("DelayPerHeal");
			EndError();
			BeginError(Any(t => t.HealAmount <= 0));
				DrawDefault("HealAmount");
			EndError();
		}
	}
}
#endif

namespace DestroyableObject
{
	[RequireComponent(typeof(Destroyable))]
	public class DestroyableHealing : MonoBehaviour
	{
		public float DelayPerHeal = 0.1f;
		public int HealAmount = 10;
		private DestroyableSnapshot snapshot;
		private Destroyable destructible;
		[SerializeField] private float cooldown;
		
		protected virtual void Awake()
		{
			if (destructible == null) destructible = GetComponent<Destroyable>();
			
			snapshot = destructible.GetSnapshot();
		}
		
		protected virtual void Update()
		{
			cooldown -= Time.deltaTime;
			
			if (cooldown <= 0.0f)
			{
				cooldown = DelayPerHeal;
				
				if (destructible == null) destructible = GetComponent<Destroyable>();
				
				if (snapshot.AlphaWidth == destructible.AlphaWidth && snapshot.AlphaHeight == destructible.AlphaHeight)
				{
					destructible.BeginAlphaModifications();
					{
						for (var y = snapshot.AlphaHeight - 1; y >= 0; y--)
						{
							for (var x = snapshot.AlphaWidth - 1; x >= 0; x--)
							{
								var index = x + y * snapshot.AlphaWidth;
								var oldAlpha = destructible.AlphaData[index];
								var newAlpha = snapshot.AlphaData[index];
								
								if (oldAlpha != newAlpha)
								{
									newAlpha = (byte)Mathf.MoveTowards(oldAlpha, newAlpha, HealAmount);
									
									destructible.WriteAlpha(x, y, newAlpha);
								}
							}
						}
					}
					destructible.EndAlphaModifications();
				}
			}
		}
	}
}