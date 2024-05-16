using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private bool firstCheckPoint = false;
    [SerializeField] private int checkPointNumber = 0;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player  = other.GetComponent<PlayerMovement>();
            if(checkPointNumber < player.checkPointNumber && firstCheckPoint)
            {
                player.SetSpawnPoint(checkPointNumber);
                GameManager.instance.NextLap();
                GameManager.instance.ResetStarbits();
            }
            else if(checkPointNumber > player.checkPointNumber && !firstCheckPoint)
            {
                player.SetSpawnPoint(checkPointNumber);
            }
        }
        else if (other.CompareTag("AI"))
        {
            RunnerAgent ai = other.GetComponent<RunnerAgent>();
            AIMovement aiMovement = other.GetComponent<AIMovement>();
            if(checkPointNumber > aiMovement.checkPointNumber && !firstCheckPoint)
            {
                ai.GiveReward(1);
                aiMovement.SetSpawnPoint(checkPointNumber);
            }
            else if(checkPointNumber < aiMovement.checkPointNumber && !firstCheckPoint)
            {
                ai.GivePenalty();
            }
            else if(checkPointNumber < aiMovement.checkPointNumber && firstCheckPoint)
            {
                ai.GiveReward(1);
                aiMovement.SetSpawnPoint(checkPointNumber);
                GameManager.instance.ResetStarbits();
            }
            else ai.GivePenalty();
        }
    }
}
