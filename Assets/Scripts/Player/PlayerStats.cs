using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Manager")]
    [SerializeField] private GameManager gameManager = null;
    
    [Header("Config")]
    [SerializeField] private float maxHealth;
    [SerializeField]private GameObject deathChunkParticle = null;
    [SerializeField]private GameObject deathBloodParticle = null;

    private float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
       
    }

    public void DecreaseHealth(float amount)
    {
        currentHealth -= amount;

        if(currentHealth <= 0.0f)
        {
            Die();
        }
    }

    private void Die()
    {
        GameObject.Instantiate(deathChunkParticle, transform.position, deathChunkParticle.transform.rotation);
        GameObject.Instantiate(deathBloodParticle, transform.position, deathBloodParticle.transform.rotation);
        gameManager.Respawn();
        Destroy(this.gameObject);
    }
}
