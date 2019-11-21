﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, ICharacterAction
{
    public float health = 100.0f;
    public float attack = 1.0f;
    public float attackRange = 20.0f;
    public float attackAngleRange = 45.0f;
    public float poisonDamage = 2.0f;
    public float poisonTotalTime = 2.0f;
    public float poisonTickTime = 3.0f;


    public const string DAMAGE_KEY = "Damage";
    public const string HEALTH_KEY = "Health";

    private float maxHealth = 100.0f;

    void Start()
    {
        maxHealth = health;
        //attack = PlayerPrefs.GetFloat(DAMAGE_KEY, 20.0f);
        //health = PlayerPrefs.GetFloat(HEALTH_KEY, 100.0f);
    }

    public void Initialize(float dmg, float hp)
    {
        attack = dmg;
        health = hp;
        maxHealth = health;
    }

    //public void SaveData(float dmg, float hp)
    //{
    //    PlayerPrefs.SetFloat(DAMAGE_KEY, dmg);
    //    PlayerPrefs.SetFloat(HEALTH_KEY, hp);
    //}

    public void TakeDamage(float damage, bool isHero)
    {
        health -= damage;
        Debug.Log("Player Took " + damage + " damage");

    }

    public IEnumerator Attack()
    {
        ////Justin - TODO:Find a better method.
        List<string> ListOfEnemies = ServiceLocator.Get<ObjectPoolManager>().GetKeys();
        foreach (var enemy in ListOfEnemies)
        {
            List<GameObject> gameObjects = ServiceLocator.Get<ObjectPoolManager>().GetActiveObjects(enemy);
            foreach (var go in gameObjects)
            {
                Vector3 direction = (go.transform.position - transform.position);
                float distance = Vector2.Distance(transform.position, go.transform.position);
                float angle = Vector3.Angle(transform.forward, direction);
                if (Mathf.Abs(angle) < attackAngleRange && distance < attackRange)
                {
                    go.GetComponent<Enemy>().TakeDamage(attack, true);
                }
            }
        }
        yield return null;
    }

    public void PoisonAttack()
    {
        ////Justin - TODO:Find a better method.
        List<string> ListOfEnemies = ServiceLocator.Get<ObjectPoolManager>().GetKeys();
        foreach (var enemy in ListOfEnemies)
        {
            List<GameObject> gameObjects = ServiceLocator.Get<ObjectPoolManager>().GetActiveObjects(enemy);
            foreach (var go in gameObjects)
            {
                Vector3 direction = (go.transform.position - transform.position);
                float distance = Vector2.Distance(transform.position, go.transform.position);
                float angle = Vector3.Angle(transform.forward, direction);
                if (Mathf.Abs(angle) < attackAngleRange && distance < attackRange)
                {
                    go.GetComponent<Enemy>().SetPoison(poisonDamage, poisonTickTime, poisonTotalTime);
                }
            }
        }
    }

    public void UpdateAnimation()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator DeathAnimation()
    {
        throw new System.NotImplementedException();
    }
}
