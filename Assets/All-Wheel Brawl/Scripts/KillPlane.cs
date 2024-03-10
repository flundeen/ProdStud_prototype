using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillPlane : MonoBehaviour
{
    [SerializeField]
    BoxCollider trigger;
    void Start()
    {
        trigger = gameObject.GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the player collides
        if (other.tag == "Player")
        {
            Player deadPlayer = other.gameObject.GetComponent<Player>();
            deadPlayer.isAlive = false;
        }
    }
}