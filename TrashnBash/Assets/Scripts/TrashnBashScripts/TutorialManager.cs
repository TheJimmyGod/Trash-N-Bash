﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public List<GameObject> UISequences;
    public Button barricadeCreateBtn;

    private int currentSequence = 0;
    private int numEnemiesToKill = 0;
    private bool isSpawnStarted = false;

    private LevelManager levelManager;
    public EnemySpawnManager enemySpawnManager;
    private Barricade barricade = null;

    private bool isplaced = false;

    private void Start()
    {
        levelManager = ServiceLocator.Get<LevelManager>();
        levelManager.isTutorial = true;
        PlayerController player = levelManager.playerInstance.GetComponent<PlayerController>();
        player.attackEnabled = true;
        player.poisonAttackEnabled = false;
        player.intimidateAttackEnabled = false;
        player.ultimateAttackEnabled = false;
        UIManager uiManager = ServiceLocator.Get<UIManager>();
        uiManager.attackImg.SetActive(true);
        uiManager.poisonImg.SetActive(false);
        uiManager.intimidateImg.SetActive(false);
        uiManager.ultImg.SetActive(false);

        StartCoroutine(StartSequence(1.0f));
        barricadeCreateBtn?.onClick.AddListener(ServiceLocator.Get<UIManager>().enableScreenFadeIn);
    }

    private void Update()
    {
        if (numEnemiesToKill <= levelManager.enemyDeathCount && isSpawnStarted)
        {
            IncrementSequence();
            isSpawnStarted = false;
            ServiceLocator.Get<UIManager>().totalWave = 0;
            ServiceLocator.Get<UIManager>().currentWave = 0;
            ServiceLocator.Get<UIManager>().currentWave = ServiceLocator.Get<UIManager>().totalWave = numEnemiesToKill;

            enemySpawnManager.ResetSpawners();
        }
        if (barricade)
        {
            if (barricade.isPlaced == true && isplaced == false)
            {
                IncrementSequence();

                barricade.TakeFullDamage();
                isplaced = true;
            }
        }


    }

    IEnumerator StartSequence(float delay)
    {
        yield return new WaitForSeconds(delay);
        UISequences[currentSequence].SetActive(true);
    }

    public void IncrementSequence()
    {
        currentSequence++;
        if (UISequences.Count - 1 >= currentSequence)
        {
            UISequences[currentSequence].SetActive(true);
        }

    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void SetEnemySpawner(List<EnemySpawner> enemySpawner)
    {
        isSpawnStarted = true;
        foreach (var item in enemySpawner)
        {
            numEnemiesToKill += item._numberOfWave * item._enemiesPerWave;
        }
    }

    public void AddCount(int total)
    {
        isSpawnStarted = true;
        numEnemiesToKill += total;
    }

    public void AddBarricade(Barricade inputBarricade)
    {
        currentSequence++;
        if (UISequences.Count - 1 >= currentSequence)
        {
            UISequences[currentSequence].SetActive(true);
        }
        barricade = inputBarricade;
    }

    public void EndTutorial()
    {
        currentSequence++;
        if (UISequences.Count - 1 >= currentSequence)
        {
            UISequences[currentSequence].SetActive(true);
        }
        GameObject barricadeManager = GameObject.FindGameObjectWithTag("BarricadeSpawner");
        ServiceLocator.Get<LevelManager>().isTutorial = false;
        LevelManager levelManager =  ServiceLocator.Get<LevelManager>();
        levelManager.playerInstance.GetComponent<PlayerController>().EnableAttack();
        levelManager.playerInstance.GetComponent<PlayerController>().EnableIntimidateAttack();
        levelManager.playerInstance.GetComponent<PlayerController>().EnableUltAttack();
        levelManager.playerInstance.GetComponent<PlayerController>().EnablePoisonAttack();
        enemySpawnManager.StartAllSpawners();
    }

}
