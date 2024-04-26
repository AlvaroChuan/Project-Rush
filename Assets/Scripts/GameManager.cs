using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private Text puntuationText1;
    [SerializeField] private Text puntuationText2;
    [SerializeField] private Text lapText;
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private Text countDownText;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private Starbit[] starbits;

    private int puntuation = 0;
    private int lap = 0;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            GameManager.instance.StartGame();
            Destroy(gameObject);
        }
        StartCoroutine(CountDown());
    }

    public void StartGame()
    {
        puntuationText1.text = "Score: " + puntuation;
        puntuationText2.text = "Score: " + puntuation;
        lapText.text = "Lap: " + lap;
        StartCoroutine(CountDown());
    }

    public void AddPuntuation(int points)
    {
        puntuation += points;
        puntuationText1.text = "Score: " + puntuation;
        puntuationText2.text = "Score: " + puntuation;
    }

    public void NextLap()
    {
        lap++;
        lapText.text = "Lap: " + lap + "/3";
        if (lap == 3)
        {
            EndGame();
        }
    }

    public void EndGame()
    {
        endGamePanel.SetActive(true);
        StartCoroutine(CountDownRestart());
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Level 1");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ResetStarbits()
    {
        foreach (Starbit starbit in starbits)
        {
            starbit.gameObject.SetActive(true);
        }
    }

    private IEnumerator CountDown()
    {
        countDownText.text = "3";
        yield return new WaitForSeconds(1);
        countDownText.text = "2";
        yield return new WaitForSeconds(1);
        countDownText.text = "1";
        yield return new WaitForSeconds(1);
        countDownText.text = "GO!";
        yield return new WaitForSeconds(1);
        countDownText.gameObject.SetActive(false);
        AddPuntuation(0);
        puntuationText1.gameObject.SetActive(true);
        lapText.gameObject.SetActive(true);
        player.canMove = true;
    }

    private IEnumerator CountDownRestart()
    {
        yield return new WaitForSeconds(5);
        RestartGame();
    }
}
