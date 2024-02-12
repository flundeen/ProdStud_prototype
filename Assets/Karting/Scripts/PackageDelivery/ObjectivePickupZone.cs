using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectivePickupZone : MonoBehaviour
{

    [SerializeField]
    private float pickupTimer = 5f;

    private bool hasPackage;
    private List<GameObject> players;
    private List<float> timers;

    public bool HasPackage { get; set; }


    void Awake()
    {
        players = new List<GameObject>();
        timers = new List<float>();
        hasPackage = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasPackage && players.Count == 1 && timers[0] > 0)
        {
            timers[0] -= Time.deltaTime;
            Debug.Log("Picking up package");
            Debug.Log(players[0].name);
            if (timers[0] <= 0)
            {
                players[0].GetComponentInParent<KartPackage>().hasPackage = true;
                hasPackage = false;
                Debug.Log("Package has been picked up");
            }
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Player")
        {
            players.Add(collision.gameObject);
            timers.Add(pickupTimer);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.tag == "Player")
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] == collision)
                {
                    players.RemoveAt(i);
                    timers.RemoveAt(i);
                }
            }
        }
    }
}
