using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace DestroyableObject
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(DestroyableFixtureGroup))]
	public class DDestroyableFixtureGroup_Editor : DestroyableEditor<DestroyableFixtureGroup>
	{
		protected override void OnInspector()
		{
			DrawDefault("Fixtures");
			DrawDefault("AutoDestroy");
			
			Separator();
			
			DrawDefault("OnAllFixturesRemoved");
		}
	}
}
#endif

namespace DestroyableObject
{
	[DisallowMultipleComponent]
	public class DestroyableFixtureGroup : MonoBehaviour
	{
		public List<DestroyableFixture> Fixtures;
		public bool AutoDestroy = true;

		public DestroyableEvent OnAllFixturesRemoved;

		public void UpdateFixtures()
		{
			if (Fixtures.Count > 0)
			{
				for (var i = Fixtures.Count - 1; i >= 0; i--)
				{
					var fixture = Fixtures[i];

					if (FixtureIsConnected(fixture) == false)
					{
						Fixtures.RemoveAt(i);
					}
				}

				if (Fixtures.Count == 0)
				{
					if (OnAllFixturesRemoved != null) OnAllFixturesRemoved.Invoke();

					if (AutoDestroy == true)
					{
						DestroyableHelper.Destroy(this);
					}
				}
			}
		}

		protected virtual void Update()
		{
			UpdateFixtures();
		}

		private bool FixtureIsConnected(DestroyableFixture fixture)
		{
			if (fixture != null)
			{
				var checkTransform = fixture.transform;

				while (checkTransform != null)
				{
					if (checkTransform == transform)
					{
						return true;
					}

					checkTransform = checkTransform.parent;
				}
			}

			return false;
		}
	}
}
