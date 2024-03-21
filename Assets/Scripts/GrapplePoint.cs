using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    private bool active = false;
    private GameObject activeIndicator;

    private void Awake()
    {
        activeIndicator = transform.GetChild(0).gameObject;
    }

    public bool GetActive()
    {
        return active;
    }

    public void SetActive(bool value)
    {
        active = value;
        activeIndicator.SetActive(value);
    }
}
