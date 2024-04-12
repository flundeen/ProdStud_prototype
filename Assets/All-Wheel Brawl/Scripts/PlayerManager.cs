using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using KartGame.KartSystems;

public class PlayerManager : MonoBehaviour
{
    // Fields
    private string sceneName;
    private PlayerInputManager inputManager;

    // Selection Fields
    private GameObject selectionDisplayParent;

    // Start is called before the first frame update
    void Start()
    {
        // This object carries player data between scenes
        DontDestroyOnLoad(gameObject);

        sceneName = SceneManager.GetActiveScene().name;
        inputManager = GetComponent<PlayerInputManager>();

        // Initialize player data based on scene
        if (sceneName == "Selection_Screen")
            InitSelection();
        else if (sceneName == "Game")
            InitGame();
    }

    void InitSelection()
    {
        inputManager.splitScreen = false;
        inputManager.EnableJoining();
        selectionDisplayParent = GameObject.Find("SelectionDisplays");
        
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
            Selection_Display sd = selectionDisplayParent.transform.GetChild(pInput.playerIndex).GetComponent<Selection_Display>();
            sd.player = pInput.GetComponent<Player>();
            sd.gameObject.SetActive(true);
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
            switch (player.carType)
            {
                case CarType.Pizza:
                    // Add pizza car as child
                    break;

                case CarType.Mail:
                    // Add mail car as child
                    break;
            }

            // Move cars to spawnpoints
        }

        // Init/start game
    }

    // Update is called once per frame
    void Update()
    {
        // If Selection:
        // Add/Remove players on load/disconnect
        // Load game when every selectionDisplay.IsReady = true

        // If Game:

    }

    void OnPlayerJoined(PlayerInput pInput)
    {
        // Add as child to prevent destruction across scenes
        pInput.transform.SetParent(transform);

        // Connect to selection display
        Selection_Display sd = selectionDisplayParent.transform.GetChild(pInput.playerIndex).GetComponent<Selection_Display>();
        sd.player = pInput.GetComponent<Player>();
        sd.gameObject.SetActive(true);
    }

    void OnPlayerLeft(PlayerInput pInput)
    {
        // Remove leaving player from selection display
        // Shift players so empty selection display is after players
    }
}
