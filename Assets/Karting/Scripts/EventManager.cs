using KartGame.KartSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

// Middleman for game events and interested objects
// Uses events when multiple listeners for a game event

public class EventManager : MonoBehaviour
{
    // Singleton Properties
    public static EventManager Instance { get; private set; }

    // Fields
    [SerializeField]
    private InGameMenuManager gameMenuManager;
    public GameObject spawnPointTree;
    private List<Player> players;
    private List<Transform> recentSpawnPoints = new List<Transform>();
    // LATER: Set length when player count is determined
    private Timer[] respawnTimers = new Timer[4];

    void Awake()
    {
        // Ensures that only one instance exists
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        // Initialize respawn timers
        for (int i = 0; i < respawnTimers.Length; i++)
            respawnTimers[i] = new Timer(3, false);

        players = PlayerManager.Instance.Players;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // if respawn timer ends, select spawn point for respawning player
        for (int i = 0; i < players.Count; i++)
        {
            // If a player is not alive, update their respawn timer
            if (!players[i].isAlive)
            {
                respawnTimers[i].Update(Time.deltaTime);

                if (respawnTimers[i].IsReady)
                {
                    RespawnPlayer(i);
                    respawnTimers[i].Start(); // prevent multiple calls
                }
            }
        }
    }

    public void PlayerDeath(object sender, ScoreEventArgs e)
    {
        // start respawn timer for dead player
        players[e.deadPlayerId].isAlive = false;
        respawnTimers[e.deadPlayerId].Start();

        // award points to attacker
        if (e.scorerId >= 0)
            GameManager.Instance.AwardPoints(e.scorerId, e.scoreEvent);
    }

    public Transform GetSpawnPoint()
    {
        Transform spawnpoint;

        do
        {
            // Select a random spawn point
            spawnpoint = spawnPointTree.transform.GetChild(UnityEngine.Random.Range(0, spawnPointTree.transform.childCount));

            // Reselect if this spawn was recently used
        } while (recentSpawnPoints.Contains(spawnpoint));

        // Track 3 most recently used spawns
        recentSpawnPoints.Add(spawnpoint);
        if (recentSpawnPoints.Count > 3) recentSpawnPoints.RemoveAt(0);

        return spawnpoint;
    }

    public void RespawnPlayer(int playerId)
    {
        // randomly pick a respawn point
        Transform spawnPoint = GetSpawnPoint();

        // respawn player at selected point
        players[playerId].Respawn(spawnPoint);
    }

    public void QuitToMenu()
    {
        GetComponent<SceneChanger>().ChangeScene("IntroMenu");
    }
}

// Event Argument classes
public class ScoreEventArgs : EventArgs
{
    public ScoreEvent scoreEvent;
    public int scorerId; // -1 if not player
    public int deadPlayerId; // Used when another player was killed
}