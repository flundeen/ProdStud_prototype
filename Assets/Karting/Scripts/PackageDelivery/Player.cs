using KartGame.KartSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // Components
    private Camera camera;
    private ArcadeKart car;
    private Weapon weapon;
    private PlayerInput pInput;

    // Selection Menu Data
    public CarType carType;
    [NonSerialized]
    public bool isSelectionConfirmed = false;

    // Input Fields
    private Vector2 cameraOffset = Vector2.zero;

    // Fields
    [NonSerialized]
    public bool isAlive = true;
    [NonSerialized]
    public Transform spawnPoint;

    // Properties
    public Vector3 Position { get { return car.transform.position; } }
    public int ID { get { return pInput.playerIndex; } }

    // Start is called before the first frame update
    void Start()
    {
        pInput = GetComponent<PlayerInput>();
    }

    public void InitCar(ArcadeKart car)
    {
        // Car initialization
        car.AssignOwner(this);
        this.car = car;
        camera = car.camera;
        pInput.camera = camera;
        Debug.Log(spawnPoint.position);
        car.SetTransform(spawnPoint);

        // Hook car death to player death event
        car.deathCallback += (int attackerId) =>
        {
            // Disable weapons
            weapon.enabled = false;

            // Alert EventManager of player death
            EventManager.Instance.PlayerDeath(this, new ScoreEventArgs
            {
                scoreEvent = (car.HasPackage ? ScoreEvent.CarrierKill : ScoreEvent.Kill),
                scorerId = attackerId,
                deadPlayerId = ID
            });
        };

        // Weapon initialization
        weapon = car.GetComponent<Weapon>();
        weapon.Initialize(ID, car);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Respawn(Transform spawn)
    {
        // Reset player data
        spawnPoint = spawn;
        Debug.Log(spawn.position);
        car.SetTransform(spawn);
        car.ResetCar();
        weapon.ResetWeapons();
        weapon.enabled = true;
        isAlive = true;
    }

    void OnMenu()
    {
        EventManager.Instance.QuitToMenu();
    }

    void OnSwitch(InputValue val)
    {
        if (isSelectionConfirmed) return;

        if (val.Get<float>() < 0)
        {
            if (carType == CarType.Pizza)
                carType = CarType.Mail;
            else
                carType--;
        }
        else
        {
            if (carType == CarType.Mail)
                carType = CarType.Pizza;
            else
                carType++;
        }
    }

    void OnSelect()
    {
        isSelectionConfirmed = !isSelectionConfirmed;
    }

    void OnCancel()
    {
        // Player leave
        Destroy(gameObject);
    }
}
