using UnityEngine;
using System.Collections.Generic;

namespace DestroyableObject
{
	public static class DestroyablePool<T>
		where T : class
	{
		private static List<T> cache = new List<T>();
		
		public static T Spawn()
		{
			return Spawn(null, null);
		}
		
		public static T Spawn(System.Action<T> onSpawn)
		{
			return Spawn(null, onSpawn);
		}
		
		public static T Spawn(System.Predicate<T> match)
		{
			return Spawn(match, null);
		}
		
		public static T Spawn(System.Predicate<T> match, System.Action<T> onSpawn)
		{
			var index = match != null ? cache.FindIndex(match) : cache.Count - 1;
			
			if (index >= 0)
			{
				var instance = cache[index];
				
				cache.RemoveAt(index);
				
				if (onSpawn != null)
				{
					onSpawn(instance);
				}
				
				return instance;
			}
			
			return null;
		}
		
		public static void Despawn(T instance)
		{
			Despawn(instance, null);
		}
		
		public static void Despawn(T instance, System.Action<T> onDespawn)
		{
			if (instance != null)
			{
				if (onDespawn != null)
				{
					onDespawn(instance);
				}
				
				cache.Add(instance);
			}
		}
	}
}