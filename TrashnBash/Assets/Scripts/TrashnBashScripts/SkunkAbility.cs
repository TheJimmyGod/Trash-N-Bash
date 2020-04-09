﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkunkAbility : MonoBehaviour, IEnemyAbilities
{
    [SerializeField][Tooltip("Tick time for poison skill")] private float _skunksPoisonTickTime = 3.0f;
    [SerializeField][Tooltip("Value to damage poison for Skunk")] private float _skunksPoisonDamage = 1.0f;
    [SerializeField][Tooltip("Area of poison for Skunk")] private float _skunksPoisonRange = 5.0f;
    [SerializeField][Tooltip("Total time for poison skill")] private float _skunksPoisonTotaltime = 3.0f;
    public GameObject poisonArea;
    private void Start()
    {
        poisonArea.SetActive(false);
    }

    private void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player)
        {
            if (Vector3.Distance(poisonArea.transform.position, player.transform.position) + (_skunksPoisonRange / 2) >= _skunksPoisonRange)
            {
                poisonArea.SetActive(false);
            }
        }

    }

    public void GroupAttack()
    {
        return;
    }

    public void PoisonAOE(GameObject player)
    {
        poisonArea.GetComponent<Transform>().localScale = new Vector3(_skunksPoisonRange, 0.00746f, _skunksPoisonRange);
        if (Vector3.Distance(poisonArea.transform.position, player.transform.position) + (_skunksPoisonRange / 2) < _skunksPoisonRange)
        {
            poisonArea.SetActive(true);
            player.GetComponent<Player>().SetPoisoned(_skunksPoisonDamage, _skunksPoisonTickTime, _skunksPoisonTotaltime);
        }
    }

    public void Flying(Transform wayPoint)
    {
        return;
    }

    public void PlayDead(GameObject player)
    {
        return;
    }
}
