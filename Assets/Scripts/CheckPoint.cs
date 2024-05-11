using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private bool firstCheckPoint = false;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerMovement>().SetSpawnPoint();
            if(firstCheckPoint)
            {
                GameManager.instance.NextLap();
                GameManager.instance.ResetStarbits();
            }
        }
    }
}
