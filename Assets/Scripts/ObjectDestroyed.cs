using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DestroyableObject;
using UniRx.Async;
using System;


public class ObjectDestroyed : MonoBehaviour
{

	[SerializeField] private Rigidbody2D rigid = null;
	[SerializeField] public bool isObject = false;
	private float positionX = default;
	private float positionY = default;

    void Awake()
    {
            
    }

    // Update is called once per frame
    
    public async void SetObjectDestroyed(GameObject collider, float xPos, float yPos)
    { 
		//print("123");
		var destructible = collider.GetComponent<Destroyable>();

		if (destructible != null)
		{

			positionX = xPos;
			positionY = yPos;

			destructible.OnEndSplit.AddListener(OnEndSplit);
			DestroyableQuadFracturer.Fracture(destructible, 10, 0.5f);
			destructible.OnEndSplit.RemoveListener(OnEndSplit);

			
			
			//rigid.simulated = false;

			// Spawn explosion prefab?
			//if (ExplosionPrefab != null)
			//{
			//	var worldRotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)); // Random rotation around Z axis

			//	Instantiate(ExplosionPrefab, explosionPosition, worldRotation);
			//}
		}
    
    }


	private async void DisableObjects(List<Destroyable> clones)
	{ 
		await UniTask.Delay(200);
		print("disabled " + clones.Count);
		for (ushort i = 0; i < clones.Count; i++)
		{
			clones[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
			//rigidbody.gravityScale = 0;

			//if (rigidbody != null)
			//{
			//	var triggers = clone.GetComponentsInChildren<PolygonCollider2D>();
			//	for (ushort o = 0; o < triggers.Length; o++)
			//	{ 
			//		//print(triggers[o].name);
			//		triggers[o].isTrigger = true;
				
			//	}


			//}
		}


		print("disabledobjects");
		
	
	
	}

	private void OnEndSplit(List<Destroyable> clones)
	{

		for (ushort i = 0; i < clones.Count; i++)
		{
			var clone     = clones[i];
			clone.GetComponent<DisableCollider>().ToDisableCollider();
			clone.gameObject.layer = LayerMask.NameToLayer("NoPlayer");

			var triggers = clone.GetComponentsInChildren<PolygonCollider2D>();
			for (ushort o = 0; o < triggers.Length; o++)
			{ 
				//print(triggers[o].name);
				triggers[o].isTrigger = false;
				triggers[o].gameObject.layer = LayerMask.NameToLayer("NoPlayer");
				
			}

			var rigidbody = clone.GetComponent<Rigidbody2D>();

			

			if ((rigidbody is null) == false)
			{
				//var localPoint = (Vector2)clone.transform.InverseTransformPoint(explosionPosition);

				var vector = clone.AlphaRect.center;// - new Vector2(positionX, positionY );

				rigidbody.AddForce(vector * 200, ForceMode2D.Impulse);
				rigidbody.gravityScale = 7;

				


			}
		}

	

		


	}


	//private void OnEndSplit(List<Destroyable> clones)
	//{
	//	// Go through all clones in the clones list
	//	for (ushort i = 0; i < clones.Count; i++)
	//	{
	//		var clone     = clones[i];
	//		clone.GetComponent<DestroyablePolygonCollider>().IsTrigger = false;

	//		var triggers = clone.GetComponentsInChildren<PolygonCollider2D>();
	//		for (ushort o = 0; o < triggers.Length; o++)
	//		{
	//			triggers[o].isTrigger = false;

	//		}
			
			
	//		var rigidbody = clone.GetComponent<Rigidbody2D>();

	//		if (rigidbody != null)
	//		{
	//			// Get the local point of the explosion that called this split event
	//			//var localPoint = (Vector2)clone.transform.InverseTransformPoint(explosionPosition);

	//			// Get the vector between this point and the center of the destructible's current rect
	//			//var vector = clone.AlphaRect.center; // - localPoint;

	//			rigidbody.AddForce(Vector2.right * 100, ForceMode2D.Impulse);
	//			rigidbody.gravityScale = 8;
				


	//		}
	//	}
	//}

}
