using System.Collections.Generic;
using UnityEngine;

public class Grapplinghook : MonoBehaviour
{
    private LineRenderer lr;
    private GameObject grapplePoint;
    private string whatIsGrappleable;
    private List<GameObject> grapplePositions;
    private SphereCollider grappleCollider;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        grapplePositions = new List<GameObject>();
        grappleCollider = GetComponent<SphereCollider>();
    }

    public void Setup(string tag, float grappleMaxDistance)
    {
        whatIsGrappleable = tag;
        grappleCollider.radius = grappleMaxDistance;
        Debug.Log(whatIsGrappleable);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == whatIsGrappleable)
        {
            if(!grapplePositions.Contains(other.gameObject))
            {
                grapplePositions.Add(other.gameObject);
                Debug.Log("Added " + other.gameObject.name);
            }

            foreach (GameObject hook in grapplePositions)
            {
                float distance = Vector3.Distance(transform.position, hook.transform.position);
                float minDistance = Mathf.Infinity;
                GrapplePoint grapplePoint2 = other.GetComponent<GrapplePoint>();
                if (distance < minDistance)
                {
                    minDistance = distance;
                    grapplePoint2.SetActive(true);
                    grapplePoint = hook;
                }
                else if (grapplePoint2.GetActive())
                {
                    grapplePoint2.SetActive(false);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == whatIsGrappleable)
        {
            grapplePositions.Remove(other.gameObject);
            other.gameObject.GetComponent<GrapplePoint>().SetActive(false);
        }
    }

    public Vector3 GetGrapplePoint()
    {
        if(grapplePositions.Contains(grapplePoint))
        {
            Debug.Log("Returning " + grapplePoint.name);
            return grapplePoint.transform.position;
        }
        else
        {
            Debug.Log("Returning zero");
            return Vector3.zero;
        }
    }
}
