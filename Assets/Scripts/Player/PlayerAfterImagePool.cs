using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImagePool : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private GameObject afterImagePrefab;
    [SerializeField] private GameObject playerGo = null;
    [SerializeField] private SpriteRenderer playerRender = null;

    private Queue<GameObject> availableObjects = new Queue<GameObject>();


    private void Awake()
    {
        GrowPool();
    }

    private void GrowPool()
    {
        for (int i = 0; i < 10; i++)
        {
            var playerInstanced = GameObject.Instantiate(afterImagePrefab, this.transform);
            playerInstanced.SetActive(false);
            playerInstanced.GetComponent<PlayerAfterImageSprite>().player = playerGo.transform;
            playerInstanced.GetComponent<PlayerAfterImageSprite>().playerRender = playerRender;
            playerInstanced.GetComponent<PlayerAfterImageSprite>().playerPoolRender = playerInstanced.GetComponent<SpriteRenderer>();
            playerInstanced.GetComponent<PlayerAfterImageSprite>().poolPlayer = this.GetComponent<PlayerAfterImagePool>(); 
            
            AddToPool(playerInstanced);
            
        }
    }

    public void AddToPool(GameObject instance)
    {
        instance.SetActive(false);
        availableObjects.Enqueue(instance);
    }

    public GameObject GetFromPool()
    {
        if(availableObjects.Count == 0)
        {
            GrowPool();
        }

        GameObject playerInstance = availableObjects.Dequeue();
        playerInstance.SetActive(true);
        return playerInstance;
    }
}
