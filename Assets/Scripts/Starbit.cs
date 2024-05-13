using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class Starbit : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SoundManager.instance.PlaySFXByIndex(SoundManager.SFX.STARBIT);
            GameManager.instance.AddPuntuation(10, 0);
            gameObject.SetActive(false);
        }
        else if (other.CompareTag("AI"))
        {
            RunnerAgent ai = other.GetComponent<RunnerAgent>();
            GameManager.instance.AddPuntuation(10, ai.playerNumber);
            ai.GiveReward();
            gameObject.SetActive(false);
        }
    }
}
