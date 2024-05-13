using System.Collections;
using System.Collections.Generic;
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
    }
}
