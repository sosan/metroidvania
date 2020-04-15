using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint = null;
    [SerializeField] private GameObject player = null;
    [SerializeField] private float respawnTime;
    [SerializeField] private CinemachineVirtualCamera cinemachine = null;
    private float respawnTimeStart;
    private bool respawn;


    private void Start()
    {
        
    }

    private void Update()
    {
        CheckRespawn();
    }
    public void Respawn()
    {
        respawnTimeStart = Time.time;
        respawn = true;
    }

    private void CheckRespawn()
    {
        if(Time.time >= respawnTimeStart + respawnTime && respawn)
        {
            var playerTemp = Instantiate(player, respawnPoint);
            cinemachine.m_Follow = playerTemp.transform;
            respawn = false;
        }
    }
}