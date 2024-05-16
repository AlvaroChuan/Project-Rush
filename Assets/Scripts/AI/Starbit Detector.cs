using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarbitDetector : MonoBehaviour
{
    [SerializeField] private AIMovement controller;
    private Transform target;
    List<GameObject> starbits = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Starbit") && !starbits.Contains(other.gameObject))
        {
            starbits.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Starbit") && starbits.Contains(other.gameObject))
        {
            starbits.Remove(other.gameObject);
        }
    }

    private void Update()
    {
        controller.verticalInput = 1;
        if (starbits.Count > 0)
        {
            float distance = Mathf.Infinity;
            for (int i = starbits.Count - 1; i >= 0; i--)
            {
                if (Vector3.Distance(transform.position, starbits[i].transform.position) < distance)
                {
                    distance = Vector3.Distance(transform.position, starbits[i].transform.position);
                    target = starbits[i].transform;
                    Vector3 direction = (target.position - transform.position).normalized;
                    controller.horizontalInput = direction.x;
                }
            }
        }
    }
}
