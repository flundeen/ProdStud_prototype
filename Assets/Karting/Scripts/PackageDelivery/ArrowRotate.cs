using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowRotate : MonoBehaviour
{
    [SerializeField]
    private Material arrowMat;
    public Transform target;
    public KartPackage kartPackage;
    public Transform attachedKart;

    public void Update()
    {
        if (target != null)
        {
            Vector3 directionVector = target.position - transform.position;
            transform.eulerAngles = new Vector3(0, Mathf.Atan2(directionVector.x, directionVector.z) * Mathf.Rad2Deg, 0);
        }

        if (!GameManager.Instance.packageIsPresent)
        {
            target = null;
            arrowMat.color = Color.clear;
        }
        else if (kartPackage.hasPackage)
        {
            target = GetClosestDropoff();
            arrowMat.color = Color.green;
        }
        else
        {
            target = GameManager.Instance.packageHolder;
            if (GameManager.Instance.packagePickedUp)
            {
                arrowMat.color = Color.red;
            }
            else
            {
                arrowMat.color = Color.yellow;
            }
        }

        transform.position = new Vector3(attachedKart.position.x, attachedKart.position.y + 4, attachedKart.position.z);
    }

    public Transform GetClosestDropoff()
    {
        ObjectiveDropoff closest = GameManager.Instance.objDropoffZones[0];
        foreach (ObjectiveDropoff objDropoff in GameManager.Instance.objDropoffZones)
        {
            if (SquaredDistance(objDropoff.transform.position, transform.position) < SquaredDistance(closest.transform.position, transform.position))
            {
                closest = objDropoff;
            }
        }
        return closest.transform;
    }

    public float SquaredDistance(Vector3 p1, Vector3 p2)
    {
        return Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2) + Mathf.Pow(p2.z - p1.z, 2);
    }
}
