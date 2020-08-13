﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Canvas canvas;

    public GameObject player;
    public GameObject tower;
    public Image playerHealthBar;
    public Image waveTimerBar;
    public Text towerHealthPercentage;

    public GameObject[] spawners;
    public GameObject pauseScreen;
    public GameObject optionsScreen;

    public Image loadingImg;
    public Image poisonCover;
    public Image intimidateCover;
    public Image ultCover;
    public Image repairIcon;
    public GameObject fade;


    public GameObject poisonImg;
    public GameObject intimidateImg;
    public GameObject ultImg;
    public GameObject repairButton;
    public GameObject placeButton;
    public GameObject timerObject;

    public EndScreen endPanel;

    public Button pauseButton;
    public Button continueButton;
    public Button restartButton;
    public Button mainmenuButton;

    private float _TowerHP;
    private bool IsPower = false;
    public float maximumTimer;
    public float currentTimer = 0.0f;

    public List<GameObject> signifiersForWaves = new List<GameObject>();
    public List<float> waveTimes = new List<float>();

    [Header("Ultimate")]
    public GameObject halfBlackScreen;
    public GameObject fadeWhite;
    public GameObject ultiRaccoon;
    [SerializeField] private float ultimateAnimationTimer = 2.0f;

    private void Start()
    {
        fade.SetActive(false);
        ultiRaccoon.SetActive(false);
        halfBlackScreen.SetActive(false);
        fadeWhite.SetActive(false);
    }

    public void enableFadeOut(bool ulti = false)
    {
        if(ulti)
        {
            fadeWhite.SetActive(true);
            fadeWhite.GetComponent<Animator>().Play("FadeOut");
            halfBlackScreen.SetActive(true);
            StartCoroutine(unableFadeForUltimate());
            StartCoroutine(UltimateAnimation(ultimateAnimationTimer));

        }
        else
        {
            fade.SetActive(true);
            fade.GetComponent<Animator>().Play("FadeOut");
            StartCoroutine("unableFade");
        }
    }

    public IEnumerator unableFade()
    {
        yield return new WaitForSeconds(2.1f);
        fade.SetActive(false);
    }

    public IEnumerator unableFadeForUltimate()
    {
        yield return new WaitForSeconds(0.3f);
        fadeWhite.SetActive(false);
    }

    public IEnumerator UltimateAnimation(float timer)
    {
        yield return new WaitUntil(() => { return ServiceLocator.Get<GameManager>()._isCameraUsing; });
        ultiRaccoon.SetActive(true);
        Time.timeScale = 0.0f;
        ultiRaccoon.GetComponent<Animator>().SetTrigger("RaccoonAnimation");

        yield return new WaitUntil(() => ultiRaccoon.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.4f
          && ultiRaccoon.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("RaccoonAnimation"));

        yield return new WaitForSecondsRealtime(timer / 2.0f);

        ultiRaccoon.GetComponent<Animator>().SetTrigger("Disappear");
        yield return new WaitUntil(() => ultiRaccoon.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f
        && ultiRaccoon.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Disappear"));

        Time.timeScale = 1.0f;
        ultiRaccoon.SetActive(false);
        halfBlackScreen.SetActive(false);
        yield return null;
    }

    public UIManager Initialize()
    {
        waveTimerBar.fillAmount = 0.0f;

        restartButton.onClick.AddListener(ServiceLocator.Get<LevelManager>().Restart);
        continueButton.onClick.AddListener(ServiceLocator.Get<LevelManager>().PauseGame);
        mainmenuButton.onClick.AddListener(ServiceLocator.Get<LevelManager>().ReturnToMainMenu);
        pauseButton.onClick.AddListener(ServiceLocator.Get<LevelManager>().PauseGame);

        pauseScreen.SetActive(false);
        playerHealthBar.fillAmount = 0.0f;
        towerHealthPercentage.text = string.Empty;
        return this;
    }

    public void UpdateImage(DamageType type, float fill)
    {
        switch (type)
        {
            case DamageType.Poison:
                poisonCover.fillAmount = fill;
                break;
            case DamageType.Intimidate:
                intimidateCover.fillAmount = fill;
                break;
            case DamageType.Ultimate:
                ultCover.fillAmount = fill;
                break;
            case DamageType.Loading:
                loadingImg.fillAmount = fill;
                break;
            default:
                break;
        }
    }

    public IEnumerator Reset()
    {
        //Debug.Log("Start Reset UI");

        timerObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        currentTimer = 0;
        ServiceLocator.Get<LevelManager>().PlayCancelled = false;
        waveTimerBar.fillAmount = 0.0f;


        player = ServiceLocator.Get<LevelManager>().playerInstance;
        tower = ServiceLocator.Get<LevelManager>().towerInstance;

        foreach (var obj in signifiersForWaves)
        {
            Destroy(obj);
        }

        waveTimes.Clear();
        signifiersForWaves.Clear();

        // Wave reset

        RegisterSpawnTime[] registerSpawnTimes = FindObjectsOfType<RegisterSpawnTime>();
        foreach (RegisterSpawnTime registerSpawnTime in registerSpawnTimes)
        {
            if (maximumTimer < registerSpawnTime.MaximumSpawnTime)
                maximumTimer = registerSpawnTime.MaximumSpawnTime;
        }

        foreach (RegisterSpawnTime registerSpawnTime in registerSpawnTimes)
        {
            GameObject signifier = Instantiate(timerObject, timerObject.transform.position, timerObject.transform.rotation, canvas.transform) as GameObject;
            signifiersForWaves.Add(signifier);

            RectTransform rectTransform = waveTimerBar.GetComponent<RectTransform>();
            float barWidth = waveTimerBar.GetComponent<RectTransform>().rect.width;
            float xPos = Mathf.Lerp(rectTransform.position.x - barWidth, rectTransform.position.x + barWidth, registerSpawnTime.StartSpawnTime / maximumTimer);
            waveTimes.Add(registerSpawnTime.StartSpawnTime / maximumTimer); // used for tutorial
            signifier.transform.position = new Vector3(xPos, waveTimerBar.transform.position.y + 20f);
        }
        waveTimes.Sort();

        waveTimerBar.fillAmount = 0.0f;
        timerObject.SetActive(false);
        // Animation Reset
        //AnimationTexture.SetBool("IsHit", false);
        //AnimationTexture.SetFloat("Energy", 0.0f);
        if(player && tower)
        {
            UpdatePlayerHealth(player.GetComponent<Player>().health, player.GetComponent<Player>()._maxHealth);
            UpdateTowerHealth(tower.GetComponent<Tower>().fullHealth);
        }


        StartTimer();
        yield return null;
    }

    public void StartTimer()
    {
        StartCoroutine(CountWaveTimer());
    }

    IEnumerator CountWaveTimer()
    {
        while (currentTimer < maximumTimer)
        {
            yield return new WaitForSeconds(1.0f);
            currentTimer += 1.0f;
            waveTimerBar.fillAmount = currentTimer / maximumTimer;
            if (ServiceLocator.Get<LevelManager>().PlayCancelled)
                break;
            yield return null;
        }
    }

    public void UpdatePlayerHealth(float curr, float max)
    {
        playerHealthBar.fillAmount = curr / max;
    }

    public void UpdateTowerHealth(float curr)
    {
        towerHealthPercentage.text = curr.ToString();
    }

}
