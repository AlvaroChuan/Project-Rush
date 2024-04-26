using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private bool firstCheckPoint = false;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<PlayerMovement>().GetSpawnPoint() != this.transform.GetChild(2).transform)
        {
            other.GetComponent<PlayerMovement>().SetSpawnPoint(transform.GetChild(2).transform);
            if(firstCheckPoint)
            {
                GameManager.instance.NextLap();
                GameManager.instance.ResetStarbits();
            }
        }
    }
}
