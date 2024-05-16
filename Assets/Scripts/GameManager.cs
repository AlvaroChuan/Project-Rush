using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private HUDManager hudManager;
    [SerializeField] private GameObject[] starbits;
    private bool gameStarted = false;
    private int[] scores = new int[9];
    private int lap = 3;
    private int speed;
    private Rigidbody playerRigidbody;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            FindHUDManager();
            if(SceneManager.GetActiveScene().name != "Main Menu") StartRace();
            else SoundManager.instance.PlayMusicByIndex(SoundManager.OST.MAIN_MENU);
        }
        else
        {
            Destroy(gameObject);
            GameManager.instance.FindHUDManager();
            if(SceneManager.GetActiveScene().name != "Main Menu") GameManager.instance.StartRace();
            else SoundManager.instance.PlayMusicByIndex(SoundManager.OST.MAIN_MENU);
        }
    }

    public void StartRace()
    {
        gameStarted = true;
        scores = new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0};
        lap = 0;
        playerRigidbody = hudManager.playerMovement.gameObject.GetComponent<Rigidbody>();
        GetStarBits();
        hudManager.StartRace();
        SoundManager.instance.PlayMusicByIndex(SoundManager.OST.LELVEL_1);
    }

    private void Update()
    {
        if (gameStarted)
        {
            speed = (int) playerRigidbody.velocity.magnitude;
            if (speed > 0) speed += 100;
            hudManager.UpdateSpeed(speed);
            hudManager.UpdateLap(lap);
            for (int i = 0; i < scores.Length; i++)
            {
                hudManager.UpdateScore(scores[i], i);
            }
        }
    }

    public void GetStarBits()
    {
        GameObject starbitsContainer = GameObject.Find("Starbits");
        starbits = new GameObject[starbitsContainer.transform.childCount];
        for (int i = 0; i < starbitsContainer.transform.childCount; i++)
        {
            starbits[i] = starbitsContainer.transform.GetChild(i).gameObject;
        }
    }

    public void ResetStarbits()
    {
        foreach (GameObject starbit in starbits)
        {
            starbit.SetActive(true);
        }
    }

    public void AddPuntuation(int points, int playerNumber)
    {
        scores[playerNumber] += points;
    }

    public void NextLap()
    {
        lap++;
        hudManager.UpdateLap(lap);
        if (lap > 3)
        {
            gameStarted = false;
            hudManager.EndRace();
        }
    }

    public void FindHUDManager()
    {
        hudManager = FindObjectOfType<HUDManager>();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gameStarted = false;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void LoadLevel(int i)
    {
        SceneManager.LoadScene("Level " + i);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
        gameStarted = false;
        SoundManager.instance.StopMusic();
    }

    public void Pause()
    {
        if (Time.timeScale == 1) hudManager.Pause();
        else hudManager.Resume();
    }

    public int GetFinalPosition()
    {
        int position = 1;
        for (int i = 0; i < scores.Length; i++)
        {
            if (scores[i] > scores[0]) position++;
        }
        return position;
    }

    public int GetScore(int playerNumber)
    {
        return scores[playerNumber];
    }
}
