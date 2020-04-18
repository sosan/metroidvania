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
    
    public void SetObjectDestroyed(GameObject collider, float xPos, float yPos)
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

			//TODO: Explosion
		}
    
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

				var vector = clone.AlphaRect.center;// - new Vector2(positionX, positionY );

				rigidbody.AddForce(vector * 200, ForceMode2D.Impulse);
				rigidbody.gravityScale = 7;

				


			}
		}

	

		


	}

	public void SetObjectHealing(GameObject collider)
	{
	
	
	
	
	
	}

}
