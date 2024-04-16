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
    private const string SELECTION_SCENE_NAME = "Selection_Screen";
    private const string GAME_SCENE_NAME = "Game";

    // Selection Fields
    private GameObject selectionDisplayParent;
    private List<Selection_Display> displays;
    [SerializeField]
    private List<GameObject> carPrefabs; // Assume pizza = 0, mail = 1

    // Start is called before the first frame update
    void Start()
    {
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
        inputManager.splitScreen = true;
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
            player.InitCar(car);

            // Move cars to spawnpoints
            Transform spawnPoint = EventManager.Instance.GetSpawnPoint();
            player.spawnPoint = spawnPoint;
        }

        // Init/start game
    }

    // Update is called once per frame
    void Update()
    {
        // If Selection:
        // Add/Remove players on load/disconnect
        // Load game when every selectionDisplay.IsReady = true
        switch (sceneName)
        {
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
                if (allReady && inputManager.playerCount > 0)
                {
                    // Load game scene
                }
                break;

            case GAME_SCENE_NAME:
                break;
        }

        // If Game:

    }

    void OnPlayerJoined(PlayerInput pInput)
    {
        // Add as child to prevent destruction across scenes
        pInput.transform.SetParent(transform);

        // Connect to selection display
        displays[pInput.playerIndex].player = pInput.GetComponent<Player>();
        displays[pInput.playerIndex].gameObject.SetActive(true);
    }

    void OnPlayerLeft(PlayerInput pInput)
    {
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
