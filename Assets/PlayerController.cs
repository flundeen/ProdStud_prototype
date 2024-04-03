using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject Player1;
    public GameObject Player2;
    public GameObject Player3;
    public GameObject Player4;
    private float tracker;
    private int players;
    // Start is called before the first frame update
    void Start()
    {
        Player1.SetActive(true);
        players = 1;
    }

    // Update is called once per frame
    void Update()
    {
        tracker += Time.deltaTime;
        if(tracker > 2)
        {
            players++;
            tracker = 0;
        }
        if(players >= 2) Player2.SetActive(true);
        if(players >= 3) Player3.SetActive(true);
        if(players >= 4) Player4.SetActive(true);
    }
}
