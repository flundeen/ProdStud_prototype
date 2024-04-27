using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using KartGame.KartSystems;
using UnityEngine.Rendering;

public class PlayerManager : MonoBehaviour
{
    // Fields
    private string sceneName;
    private PlayerInputManager inputManager;
    private const string TITLE_SCENE_NAME = "IntroMenu";
    private const string SELECTION_SCENE_NAME = "Selection_Screen";
    private const string GAME_SCENE_NAME = "Game";
    private bool isGameLoading = false;

    // Static Fields
    public static PlayerManager Instance { get; private set; }
    public List<Player> Players { get; private set; }

    // Selection Fields
    private GameObject selectionDisplayParent;
    private List<Selection_Display> displays;
    [SerializeField]
    private List<GameObject> carPrefabs; // Assume pizza = 0, mail = 1

    // Start is called before the first frame update
    void Start()
    {
        // Ensures that only one instance exists
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
        
        if (Players == null) Players = new List<Player>();

        // This object carries player data between scenes
        DontDestroyOnLoad(gameObject);

        sceneName = SceneManager.GetActiveScene().name;
        inputManager = GetComponent<PlayerInputManager>();
        
        // Initialize player data based on scene
        if (sceneName == SELECTION_SCENE_NAME)
            InitSelection();
        else if (sceneName == GAME_SCENE_NAME)
            InitGame();
    }

    void InitSelection()
    {
        inputManager.splitScreen = false;
        inputManager.EnableJoining();
        selectionDisplayParent = GameObject.Find("SelectionDisplays");

        displays = new List<Selection_Display>();
        for (int i = 0; i < selectionDisplayParent.transform.childCount; i++)
            displays.Add( selectionDisplayParent.transform.GetChild(i).GetComponent<Selection_Display>() );
        
        // For existing players...
        for (int i = 0; i < inputManager.playerCount; i++)
        {
            PlayerInput pInput = PlayerInput.GetPlayerByIndex(i);

            // Set input maps to Selection inputs
            // Controls are in KartControls.inputactions
            pInput.SwitchCurrentActionMap("Selection");

            // Remove last match's cars
            for (int j = 0; j < pInput.transform.childCount; j++)
                Destroy(pInput.transform.GetChild(j).gameObject);

            // Connect to selection displays
            displays[i].player = pInput.GetComponent<Player>();
            displays[i].gameObject.SetActive(true);
        }
    }

    void InitGame()
    {
        inputManager.DisableJoining();

        // Load player data
        for (int i = 0; i < inputManager.playerCount; i++)
        {
            PlayerInput pInput = PlayerInput.GetPlayerByIndex(i);
            Player player = pInput.GetComponent<Player>();

            // Set input maps to Kart inputs
            // Controls are in KartControls.inputactions
            pInput.SwitchCurrentActionMap("Kart");

            // Add player cars based on selections
            // Initialize cars/weapons
            ArcadeKart car = null; // Will always have value after switch
            switch (player.carType)
            {
                case CarType.Pizza:
                    // Add pizza car as child
                    car = Instantiate(carPrefabs[0], player.transform).GetComponent<ArcadeKart>();
                    break;

                case CarType.Mail:
                    // Add mail car as child
                    car = Instantiate(carPrefabs[1], player.transform).GetComponent<ArcadeKart>();
                    break;
            }

            // Set player spawnpoints and initialize car
            Transform spawnPoint = EventManager.Instance.GetSpawnPoint();
            player.spawnPoint = spawnPoint;
            player.InitCar(car);
        }

        // Init/start game
        inputManager.splitScreen = true;
        GameManager.Instance.StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case TITLE_SCENE_NAME:

                // Should not be on title screen
                Destroy(gameObject);
                break;

            case SELECTION_SCENE_NAME:

                // Check if all players are ready
                bool allReady = true;
                foreach (Selection_Display sd in displays)
                {
                    // If active display is not ready, lower allReady flag
                    if (sd.gameObject.activeSelf)
                        allReady = allReady && sd.IsReady;
                }

                // Load game when players are ready (ensure there are players)
                if (allReady && inputManager.playerCount > 0 && !isGameLoading)
                {
                    // Load game scene, initialize game once scene is loaded
                    isGameLoading = true;
                    SceneManager.LoadSceneAsync("Game").completed += (asyncOperation) => {
                        InitGame(); 
                    };
                }
                break;

            case GAME_SCENE_NAME:
                break;
        }
    }

    public void RemovePlayers()
    {
        // Delete player cars to remove cameras/inputs
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).GetComponentInChildren<ArcadeKart>().gameObject);
        }
    }

    void OnPlayerJoined(PlayerInput pInput)
    {
        // Add Player component to static list
        Players.Add(pInput.GetComponent<Player>());

        // Add as child to prevent destruction across scenes
        pInput.transform.SetParent(transform);

        // Connect to selection display
        displays[pInput.playerIndex].player = pInput.GetComponent<Player>();
        displays[pInput.playerIndex].gameObject.SetActive(true);
        displays[pInput.playerIndex].player.isSelectionConfirmed = false;
    }

    void OnPlayerLeft(PlayerInput pInput)
    {
        // Remove Player component from list
        Players.Remove(pInput.GetComponent<Player>());

        if (sceneName != SELECTION_SCENE_NAME) return;

        // Remove leaving player from selection display
        // Shift players so empty selection display is after players
        Player p = null;
        for (int i = displays.Count - 1; i >= pInput.playerIndex; i--)
        {
            // Skip inactive displays
            if (!displays[i].gameObject.activeSelf) continue;

            // Store player from last active display and turn it off
            if (p == null)
            {
                p = displays[i].player;
                displays[i].player = null;
                displays[i].gameObject.SetActive(false);
            }
            else // Shift players forward into leaving player's display
            {
                // Tuple to swap values
                (p, displays[i].player) = (displays[i].player, p);
            }
        }
    }
}
