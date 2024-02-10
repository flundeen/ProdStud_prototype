using KartGame.KartSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [SerializeField]
    InGameMenuManager gameMenuManager;

    [SerializeField]
    List<ArcadeKart> players; // Active players

    // Start is called before the first frame update
    void Start()
    {
        if (gameMenuManager != null)
        {
            foreach (ArcadeKart p in players)
            {
                p.RaiseMenuToggle += (object sender, EventArgs e) => {
                    gameMenuManager.ToggleMenu();
                };
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
