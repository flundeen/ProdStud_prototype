using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

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
    public bool packageIsPresent = false;
    public bool packagePickedUp = false;
    public int[] scores = new int[4];
    public Transform packageHolder = null;
    public List<ObjectivePickupZone> objPickupZones = new List<ObjectivePickupZone>();
    public List<ObjectiveDropoff> objDropoffZones = new List<ObjectiveDropoff>();
    public TMP_Text timerText;

    // Score Screen Fields
    public GameObject scoreScreen;
    private bool isScoring = false;
    private int scorePlayerIndex = 0;
    private Timer scoreRevealTimer = new Timer(0.5f);

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

    }

    // Update is called once per frame
    void Update()
    {
        if (gameTime > 0)
        {
            gameTime -= Time.deltaTime;
            timerText.text = ((int)(gameTime / 60)).ToString() + ":" + ((int)(gameTime % 60)).ToString("D2");

            // process game state
            if (!packageIsPresent)
            {
                int randomPickupZone = Random.Range(0, objPickupZones.Count);
                objPickupZones[randomPickupZone].AddPackage();
                packageHolder = objPickupZones[randomPickupZone].GetComponentInParent<Transform>();
            }

            if (packagePickedUp)
            {
                objDropoffZones[0].dropoffMaterial.color = Color.blue;
            }

            if (gameTime <= 0)
                EndGame();
        }
        else if (isScoring)
        {
            // Display scores one at a time
            if (scoreRevealTimer.Update(Time.deltaTime))
            {
                // If there are still player scores to reveal, reveal next player's score
                if (scorePlayerIndex < PlayerInputManager.instance.playerCount)
                {
                    TMP_Text scoreLabel = scoreScreen.transform.GetChild(++scorePlayerIndex).GetComponent<TextMeshProUGUI>();
                    scoreLabel.text = "Player " + scorePlayerIndex + ":   " + scores[scorePlayerIndex-1];
                    scoreLabel.gameObject.SetActive(true);
                    scoreRevealTimer.SetLength(0.25f);
                    scoreRevealTimer.Start();
                }
                else
                {
                    // After all scores revealed, show menu button
                    scoreScreen.transform.GetChild(scoreScreen.transform.childCount - 1).gameObject.SetActive(true);
                    isScoring = false;
                }
            }
        }
    }

    // Initialize game, enable movement and begin round
    public void StartGame()
    {
        // setup game here
        gameTime = 2f; // 5 minutes


    }

    // End round, stop game and disable movement
    public void EndGame()
    {
        PlayerManager.Instance.RemovePlayers();
        isScoring = true;
        timerText.gameObject.SetActive(false);
        scoreScreen.SetActive(true);
        scoreRevealTimer.Start();
    }

    // Update target player's score based on event type
    public void AwardPoints(int id, ScoreEvent e)
    {
        if (id < 0 || id > scores.Length)
        {
            Debug.Log("ERROR: invalid id for awarding points");
            return;
        }

        // Enum value determines points awarded (see Player.cs)
        scores[id] += (int)e;
        Debug.Log("Player " + id + " earned " + (int)e + " points for " + e.ToString());
    }
}
