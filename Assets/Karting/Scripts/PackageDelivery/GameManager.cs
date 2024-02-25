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

// Manages game flow, initializing and ending gameplay, and tracking scores
public class GameManager : MonoBehaviour
{
    // Singleton Properties
    public static GameManager Instance { get; private set; }

    // Fields
    public float gameTime = 0;
    public Player[] players = new Player[4];
    public int[] scores = new int[4];

    void Awake()
    {
        // Ensures that only one instance exists
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Gather players and set ids
        // I know this is slow, I'll figure out a better way later
        players = FindObjectsOfType<Player>();
        for (int i = 0; i < players.Length; i++)
            players[i].id = i;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameTime > 0)
        {
            gameTime -= Time.deltaTime;

            // process game state

            if (gameTime <= 0)
                EndGame();
        }
    }

    // Initialize game, enable movement and begin round
    public void StartGame()
    {
        // setup game here
        gameTime = 300f; // 5 minutes
    }

    // End round, stop game and disable movement
    public void EndGame()
    {

    }

    // Update target player's score based on event type
    public void AwardPoints(int id, ScoreEvent e)
    {
        if (id < 0 || id > scores.Length)
        {
            Debug.Log("ERROR: invalid id for awarding points");
        }

        // Enum value determines points awarded (see Player.cs)
        scores[id] += (int)e;
        Debug.Log("Player " + id + " earned " + (int)e + " points for " + e.ToString());
    }
}
