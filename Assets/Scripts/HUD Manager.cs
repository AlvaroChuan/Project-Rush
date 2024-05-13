using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.VFX;

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
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private Text finalScore;
    [SerializeField] private Text finalScoreShadow;
    [SerializeField] private VisualEffect speedLines;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    public PlayerMovement playerMovement;

    public void Start()
    {
        if(SceneManager.GetActiveScene().name == "Main Menu")
        {
            musicSlider.value = SoundManager.instance.GetMusicVolume();
            sfxSlider.value = SoundManager.instance.GetSFXVolume();
        }
    }

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
        if(speed > 0)
        {
            speedLines.SetFloat("radious", 2.5f);
            speedLines.SetFloat("Min Speed", (speed * 3.5f) / 160);
            speedLines.SetFloat("Max Speed", (speed * 6.5f) / 160);
        }
        else    speedLines.SetFloat("radious", 100);
    }

    public void EndRace()
    {
        SoundManager.instance.StopMusic();
        SoundManager.instance.PlayFootsteps(false);
        SoundManager.instance.PlaySFXByIndex(SoundManager.SFX.RACE_END);
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
        for (int i = 0; i < 14; i++)
        {
            CountDown.text = (14 - i).ToString();
            CountDownShadow.text = (14 - i).ToString();
            if(i != 13)yield return new WaitForSeconds(1);
            else yield return new WaitForSeconds(0.5f);
        }
        SoundManager.instance.PlaySFXByIndex(SoundManager.SFX.START);
        CountDown.text = "GO!";
        CountDownShadow.text = "GO!";
        playerMovement.canMove = true;
        yield return new WaitForSeconds(1);
        CountDown.gameObject.SetActive(false);
        CountDownShadow.gameObject.SetActive(false);
    }

    public void ShowOptions()
    {
        SoundManager.instance.PlaySFXByIndex(SoundManager.SFX.BUTTON_PRESSED);
        optionsPanel.SetActive(true);
    }

    public void HideOptions()
    {
        SoundManager.instance.PlaySFXByIndex(SoundManager.SFX.BUTTON_PRESSED);
        optionsPanel.SetActive(false);
    }

    public void ShowCredits()
    {
        SoundManager.instance.PlaySFXByIndex(SoundManager.SFX.BUTTON_PRESSED);
        creditsPanel.SetActive(true);
    }

    public void HideCredits()
    {
        SoundManager.instance.PlaySFXByIndex(SoundManager.SFX.BUTTON_PRESSED);
        creditsPanel.SetActive(false);
    }

    public void ShowControls()
    {
        SoundManager.instance.PlaySFXByIndex(SoundManager.SFX.BUTTON_PRESSED);
        controlsPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SoundManager.instance.PlaySFXByIndex(SoundManager.SFX.BUTTON_PRESSED);
        Time.timeScale = 1;
        GameManager.instance.RestartGame();
    }

    public void ExitGame()
    {
        SoundManager.instance.PlaySFXByIndex(SoundManager.SFX.BUTTON_PRESSED);
        Application.Quit();
    }

    public void LoadLevel(int i)
    {
        SoundManager.instance.PlaySFXByIndex(SoundManager.SFX.BUTTON_PRESSED);
        GameManager.instance.LoadLevel(i);
    }

    public void LoadMainMenu()
    {
        SoundManager.instance.PlaySFXByIndex(SoundManager.SFX.BUTTON_PRESSED);
        Time.timeScale = 1;
        GameManager.instance.LoadMainMenu();
    }

    public void Pause()
    {
        if(playerMovement.canMove)
        {
            SoundManager.instance.PauseMusic();
            SoundManager.instance.PlayFootsteps(false);
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
            SoundManager.instance.ResumeMusic();
            Time.timeScale = 1;
            pausePanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void GoToItchio()
    {
        SoundManager.instance.PlaySFXByIndex(SoundManager.SFX.BUTTON_PRESSED);
        Application.OpenURL("https://hollowblink.itch.io");
    }

    public void GoToTwitter()
    {
        SoundManager.instance.PlaySFXByIndex(SoundManager.SFX.BUTTON_PRESSED);
        Application.OpenURL("https://twitter.com/hollowblink");
    }

    public void SetMusicVolume()
    {
        SoundManager.instance.SetMusicVolume(musicSlider.value);
    }

    public void SetSFXVolume()
    {
        SoundManager.instance.SetSFXVolume(sfxSlider.value);
    }
}
