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
        grappleCollider.center = new Vector3(0, 0, grappleMaxDistance - 1);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == whatIsGrappleable)
        {
            if(!grapplePositions.Contains(other.gameObject))
            {
                grapplePositions.Add(other.gameObject);
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
            return grapplePoint.transform.position;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public void DrawRope()
    {
        lr.positionCount = 2;
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, grapplePoint.transform.position);
    }

    public void ClearRope()
    {
        lr.positionCount = 0;
    }
}
