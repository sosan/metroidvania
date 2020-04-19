using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PresaEscaladaManager : MonoBehaviour
{

    [SerializeField] private SpriteRenderer selectedRender = null;

    private void Awake()
    {
        selectedRender.enabled = false;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void ActivarSelectedRender()
    { 
    
        selectedRender.enabled = true;
    
    }

    public void DesActivarSelectedRender()
    { 
    
        selectedRender.enabled = false;
    
    }


    

}
