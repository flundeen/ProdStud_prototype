using KartGame.KartSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        // If the player's car hitbox collides
        if (other.CompareTag("Player"))
        {
            // Deal lethal damage
            ArcadeKart playerCar = other.gameObject.GetComponentInParent<ArcadeKart>();
            playerCar.TakeDamage(new AttackInfo(-1, AttackType.Hazard, 10000, 0));
        }
    }
}