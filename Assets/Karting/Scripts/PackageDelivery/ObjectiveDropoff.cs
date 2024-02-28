using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveDropoff : MonoBehaviour
{
    [SerializeField]
    private float scoreTimer = 1.0f;
    [SerializeField]
    public Material dropoffMaterial;

    private bool scored;

    void Awake()
    {
        scored = false;
    }

    void Start()
    {
        GameManager.Instance.objDropoffZones.Add(this);
    }

    // Update is called once per frame
    void Update()
    { 
        if (scored)
        {
            dropoffMaterial.color = Color.green;
            scoreTimer -= Time.deltaTime;
            if (scoreTimer < 0)
            {
                scored = false;
            }
        }
        else
        {
            dropoffMaterial.color = Color.gray;
            scoreTimer = 1.0f;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Player" && collision.GetComponentInParent<KartPackage>().hasPackage)
        {
            collision.GetComponentInParent<KartPackage>().hasPackage = false;
            GameManager.Instance.packageIsPresent = false;
            GameManager.Instance.packagePickedUp = false;
            scored = true;

            GameManager.Instance.AwardPoints(collision.GetComponentInParent<Player>().id, ScoreEvent.Delivery);
        }
    }
}
