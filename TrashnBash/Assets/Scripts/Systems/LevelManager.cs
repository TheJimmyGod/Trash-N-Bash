using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SheetCodes;

public class LevelManager : MonoBehaviour
{
    public GameObject playerInstance;
    public GameObject towerInstance;

    public int levelNumber;
    public int enemyDeathCount;

    public float playerHealth = 100.0f;
    public float towerHealth = 100.0f;

    public bool isTutorial = false;
    public bool PlayCancelled = false;

    public LevelManager Initialize()
    {
        return this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PauseGame();
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        UIManager uiManager = ServiceLocator.Get<UIManager>();
        PauseGame();
        //ResetLevel();
        PlayCancelled = true;
        uiManager.enableFadeOut();
        uiManager.Reset();
        
    }

    public void ReturnToMainMenu()
    {
        UIManager uiManager = ServiceLocator.Get<UIManager>();
        PlayCancelled = true;
        uiManager.poisonImg.SetActive(true);
        uiManager.intimidateImg.SetActive(true);
        uiManager.ultImg.SetActive(true);
        PauseGame();
        //ResetLevel();
        SceneManager.LoadScene("MainMenu");
    }

    public void PauseGame()
    {
        GameObject pauseScreen = ServiceLocator.Get<UIManager>().pauseScreen;
        pauseScreen.SetActive(!pauseScreen.activeSelf);

        if (pauseScreen.activeSelf)
        {
            Time.timeScale = 0.0f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }

    }

    public void IncreaseEnemyDeathCount(int increment)
    {
        enemyDeathCount += increment;
        //Debug.Log("Enemy Death Count:" + enemyDeathCount);
    }

    public bool CheckLoseCondition()
    {
        if (playerInstance == null || towerInstance == null)
        {
            return false;
        }
        if (playerInstance.GetComponent<Player>().Health <= 0.0f )
        {
            ServiceLocator.Get<UIManager>().endPanel.EnableLoseText(true);
            return true;
        }
        if( towerInstance.GetComponent<Tower>().fullHealth <= 0.0f)
        {
            ServiceLocator.Get<UIManager>().endPanel.EnableLoseText(true);
            return true;
        }
        return false;
    }

    public bool CheckWinCondition()
    {
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        if (boss && boss.GetComponent<Boss>().IsDead) return true;

        List<GameObject> survivedEnemies = new List<GameObject>();
        List<GameObject> enemyspawners = new List<GameObject>();

        // Checking Units
        List<string> enemies = ServiceLocator.Get<ObjectPoolManager>().GetKeys(); // Get the monster name
        foreach (var enemy in enemies)
        {
            foreach (var e in ServiceLocator.Get<ObjectPoolManager>().GetActiveObjects(enemy)) // Check enemy which is surviving
            {
                if (e.tag == "Enemy")
                {
                    survivedEnemies.Add(e);
                }
                if (e.tag == "Boss")
                {
                    survivedEnemies.Add(e);
                }
            }
        }

        // Checking Spawners
        GameObject[] spawners = GameObject.FindGameObjectsWithTag("Spawner");
        foreach(var obj in spawners)
        {
            if(obj.GetComponent<EnemySpawner>()._currentWave < obj.GetComponent<EnemySpawner>()._numberOfWave) 
                // It is represent to they are still spawning
            {
                enemyspawners.Add(obj);
            }
        }
        if (enemyspawners.Count == 0 && survivedEnemies.Count == 0)
        {
            ServiceLocator.Get<UIManager>().endPanel.EnableWinText(true);
            return true; // If no more spawning
        }
        enemyspawners.Clear();
        survivedEnemies.Clear();

        return false;
    }

    public int GetStarRating()
    {
        float average = (playerHealth + towerHealth) * 0.5f;

        if (average < 60.0f)
            return 1;
        else if (average >= 60.0f && average < 80.0f)
            return 2;
        else
            return 3;
    }

    public void ResetLevel()
    {
        if(SceneManager.GetActiveScene().buildIndex == 3)
        {
            ServiceLocator.Get<GameManager>().changeGameState(GameManager.GameState.Tutorial);
        }
        else
        {
            ServiceLocator.Get<GameManager>().changeGameState(GameManager.GameState.GamePlay);
        }
        ServiceLocator.Get<GameManager>().LoadTowerHP();

        UIManager uiManager = ServiceLocator.Get<UIManager>();
        uiManager.enableFadeOut();
        if (playerInstance == null || towerInstance == null)
        {
            playerInstance = GameObject.FindGameObjectWithTag("Player");
            towerInstance = GameObject.FindGameObjectWithTag("Tower");
        }
        enemyDeathCount = 0;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].GetComponent<Enemy>().rigid.velocity = Vector3.zero;
            enemies[i].GetComponent<Enemy>().killed?.Invoke();
        }
        uiManager.Reset();
        isTutorial = false;
        playerInstance.GetComponent<Player>().health = playerInstance.GetComponent<Player>()._maxHealth;

        ApplyUpgrades();
    }

    public void LoadData()
    {
        return;
    }

    void ApplyUpgrades()
    {
        // Upgrade options
        GameManager gameManager = ServiceLocator.Get<GameManager>();
        UpgradesModel upgradesModel = ModelManager.UpgradesModel;
        UpgradesIdentifier upgradesIdentifier = UpgradesIdentifier.None;

        // Long Ranged Upgrade
        // Moved to tower script

        // Barricade Reduction Cost Upgrade
        // Moved to Barricade Spawner Script

        // Wife tower Upgrade
        Tower wife = GameObject.FindGameObjectWithTag("Wife").GetComponent<Tower>();
        int wifeLevel = gameManager.upgradeLevelsDictionary[UpgradeMenu.Upgrade.ExtraProjectiles] - 1;
        upgradesIdentifier = upgradesModel.GetUpgradeEnum(UpgradeMenu.Upgrade.ExtraProjectiles, wifeLevel + 1);
        if (wifeLevel >= 0)
        {
            wife.isShooting = true;
            wife.attackRate -= upgradesModel.GetRecord(upgradesIdentifier).ModifierValue; //upgradeStats.throwingSpeed[wifeLevel];
        }

        // Specific Target Upgrade
        int specficLevel = gameManager.upgradeLevelsDictionary[UpgradeMenu.Upgrade.TargetEnemy] - 1;
        upgradesIdentifier = upgradesModel.GetUpgradeEnum(UpgradeMenu.Upgrade.TargetEnemy, specficLevel + 1);
        if (specficLevel >= 0)
            towerInstance.GetComponent<Tower>().specificEnemy = gameManager.choosenTarget;//upgradesModel.GetRecord(upgradesIdentifier).Target;

        // Fire Upgrade
        int fireLevel = gameManager.upgradeLevelsDictionary[UpgradeMenu.Upgrade.FireProjectile] - 1;
        upgradesIdentifier = upgradesModel.GetUpgradeEnum(UpgradeMenu.Upgrade.FireProjectile, fireLevel + 1);
        if (fireLevel >= 0)
        {
            towerInstance.GetComponent<Tower>().damageType = DamageType.Fire;
            towerInstance.GetComponent<Tower>().fireDuration = upgradesModel.GetRecord(upgradesIdentifier).ModifierValue;// upgradeStats.fireDuration[fireLevel];
        }

        // Improved Barricades
        // Moved to Barricade Script

        // Barricade Spawn Rate Improved
        // Moved to Barricade Spawner Script

        // Trash Spawn Rate Improved 
        // Moved to Resource Spawner Script

        // Improved Player HP
        // Moved to Player Script

        // Improved healing
        // Moved to Tower Script

        // UpdateTowerTrashCount
        towerInstance.GetComponent<Tower>().fullHealth = gameManager._houseHP;
    }
}
