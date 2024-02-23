using KartGame.KartSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ScoreEvent
{
    Pickup = 1,
    Kill = 3,
    CarrierKill = 5,
    Delivery = 7
}

public class Player : MonoBehaviour
{
    // Fields
    private ArcadeKart car;

    // Start is called before the first frame update
    void Start()
    {
        car = GetComponent<ArcadeKart>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
