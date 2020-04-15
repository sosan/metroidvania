using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImageSprite : MonoBehaviour
{

    [Header("Managers")]
    [SerializeField] public PlayerAfterImagePool poolPlayer = null;

    [SerializeField] private float activeTime = 0.1f;
    [SerializeField] private float alphaSet = 0.8f;
    [SerializeField] private float alphaDecay = 0.85f;

    private float timeActivated;
    private float alpha;
    
    [HideInInspector] public Transform player;

    [HideInInspector] public SpriteRenderer playerPoolRender = null;
    [HideInInspector] public SpriteRenderer playerRender = null;
    private bool isFirstTime = true;
    private Color color;
    

    private void OnEnable()
    {
        if (isFirstTime == true)
        { 
            isFirstTime = false;
            return;
        }
            

        
        alpha = alphaSet;
        if (playerRender != null)
        {
            playerPoolRender.sprite = playerRender.sprite;

        }

        this.transform.position = player.position;
        this.transform.rotation = player.rotation;
        timeActivated = Time.time;
    }

    private void Update()
    {
        if (isFirstTime == true) return;

        alpha -= alphaDecay * Time.deltaTime;
        color = new Color(1f, 1f, 1f, alpha);
        playerPoolRender.color = color;

        if(Time.time >= (timeActivated + activeTime))
        {
            poolPlayer.AddToPool(this.gameObject);
        }

    }
}
