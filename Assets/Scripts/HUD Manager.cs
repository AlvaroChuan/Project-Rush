using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private GameObject MainPanel;
    [SerializeField] private GameObject EndGamePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Text CountDown;
    [SerializeField] private Text CountDownShadow;
    [SerializeField] private Text Lap;
    [SerializeField] private Text LapShadow;
    [SerializeField] private Text[] Scores;
    [SerializeField] private Text Speed;
    [SerializeField] private Text SpeedShadow;
    [SerializeField] private Image Speedometer;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private Text finalScore;
    [SerializeField] private Text finalScoreShadow;
    public PlayerMovement playerMovement;

    public void StartRace()
    {
        MainPanel.SetActive(true);
        EndGamePanel.SetActive(false);
        StartCoroutine(CountDownSecuence());
    }

    public void UpdateLap(int lap)
    {
        Lap.text = "Lap: " + lap + "/3";
        LapShadow.text = "Lap: " + lap + "/3";
    }

    public void UpdateScore(int score, int playerNumber)
    {
        Scores[playerNumber].text = score + " Pts";
    }

    public void UpdateSpeed(float speed)
    {
        Speed.text = math.floor(speed).ToString() + " Km/h";
        SpeedShadow.text = math.floor(speed).ToString() + " Km/h";
        Speedometer.fillAmount = speed / 160;
    }

    public void EndRace()
    {
        MainPanel.SetActive(false);
        EndGamePanel.SetActive(true);
        playerMovement.canMove = false;
        playerMovement.rb.velocity = Vector3.zero;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        string text = "Position: ";
        switch (GameManager.instance.GetFinalPosition())
        {
            case 1:
                text += "1st";
                break;
            case 2:
                text += "2nd";
                break;
            case 3:
                text += "3rd";
                break;
            case 4:
                text += "4th";
                break;
            case 5:
                text += "5th";
                break;
        }
        text += "\nScore: " + GameManager.instance.GetScore(0) + " Pts";
        finalScore.text = text;
        finalScoreShadow.text = text;
    }

    private IEnumerator CountDownSecuence()
    {
        CountDown.text = "3";
        CountDownShadow.text = "3";
        yield return new WaitForSeconds(1);
        CountDown.text = "2";
        CountDownShadow.text = "2";
        yield return new WaitForSeconds(1);
        CountDown.text = "1";
        CountDownShadow.text = "1";
        yield return new WaitForSeconds(1);
        CountDown.text = "GO!";
        CountDownShadow.text = "GO!";
        yield return new WaitForSeconds(1);
        CountDown.gameObject.SetActive(false);
        CountDownShadow.gameObject.SetActive(false);
        playerMovement.canMove = true;
    }

    public void ShowOptions()
    {
        optionsPanel.SetActive(true);
    }

    public void HideOptions()
    {
        optionsPanel.SetActive(false);
    }

    public void ShowCredits()
    {
        creditsPanel.SetActive(true);
    }

    public void HideCredits()
    {
        creditsPanel.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        GameManager.instance.RestartGame();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void LoadLevel(int i)
    {
        GameManager.instance.LoadLevel(i);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1;
        GameManager.instance.LoadMainMenu();
    }

    public void Pause()
    {
        if(playerMovement.canMove)
        {
            Time.timeScale = 0;
            pausePanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void Resume()
    {
        if(playerMovement.canMove)
        {
            Time.timeScale = 1;
            pausePanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void GoToItchio()
    {
        Application.OpenURL("https://hollowblink.itch.io");
    }

    public void GoToTwitter()
    {
        Application.OpenURL("https://twitter.com/hollowblink");
    }
}
