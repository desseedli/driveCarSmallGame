using EndlessCarChase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleControl:MonoBehaviour
{
    protected static ECCGameController gameController;
    public ParticleSystem particle;
    public string poolName;

    public void Start()
    {
        if (gameController == null) gameController = GameObject.FindObjectOfType<ECCGameController>();
        if (particle == null) particle = GetComponentInChildren<ParticleSystem>();
    }

    public void Update()
    {
        if (particle != null && !particle.isPlaying && gameObject.activeSelf)
        {
            DestroyMyself(); 
        }
    }

    public void DestroyMyself()
    {
        gameController.dictObjectPool[poolName].ReturnObject(gameObject);
    }

    public void Play()
    {
        if (particle != null)
        {
            particle.Play();
        }
    }
}
