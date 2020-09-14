﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerInfoMenu : MonoBehaviour
{
    public Text playerLoginName;
    public GameObject loginHolder;
    public GameObject infoHolder;

    public InputField firstNameText;
    public InputField lastNameText;
    public InputField dobText;
    public InputField email;
    public InputField nickName;
    public Dropdown optInDropDown;

    public GameObject matchContent;
    public GameObject matchTextPrefab;

    private void Awake()
    {
        loginHolder.SetActive(true);
        infoHolder.SetActive(false);
    }

    public void BackButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LogIn()
    {
        var db = DatabaseConnection.Instance;
        if (db.Login(playerLoginName.text))
        {
            loginHolder.SetActive(false);
            infoHolder.SetActive(true);
            DisplayPlayerInfo();
            DisplayMatches();
        }
    }

    void DisplayPlayerInfo()
    {
        var db = DatabaseConnection.Instance;
        if (db.currentPlayer != null)
        {
            firstNameText.text = db.currentPlayer.first_name;
            lastNameText.text = db.currentPlayer.last_name;
            dobText.text = db.currentPlayer.date_of_birth.ToShortDateString();
            nickName.text = db.currentPlayer.nickname;
            email.text = db.currentPlayer.email;
            optInDropDown.value = db.currentPlayer.opt_in == true ? 1 : 2;
        }
        else
        {
            firstNameText.text = "";
            lastNameText.text = "";
            dobText.text = "";
            email.text = "";
            optInDropDown.value = 0;
        }
    }

    void DisplayMatches()
    {
        var db = DatabaseConnection.Instance;
        List<MatchSQL> matchSQLs = db.GetCurretPlayerMatches();
        if (matchSQLs != null)
        {
            float counter = 0.5f;
            foreach (var match in matchSQLs)
            {
                float size = matchTextPrefab.GetComponent<RectTransform>().rect.height;
                GameObject go = Instantiate(matchTextPrefab, matchContent.transform);
                go.transform.localPosition = new Vector3(go.transform.localPosition.x, -counter * size, go.transform.localPosition.z);
                counter += 1.0f;
                go.GetComponent<Text>().text = "level: "+ match.level_number.ToString() +" score:"+ match.score.ToString() +" date: "+ match.date.ToShortDateString();
            }
        }
    }
}