using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Async;
using System;

public class DisableCollider : MonoBehaviour
{
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public async void ToDisableCollider()
    { 
        await UniTask.Delay(TimeSpan.FromMilliseconds(1000));
        this.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        this.GetComponent<Rigidbody2D>().gravityScale = 0;


    }


}
