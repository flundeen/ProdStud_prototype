using KartGame.KartSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // Components
    public Camera camera;
    public ArcadeKart car;
    public Weapon weapon;

    // Input Fields
    private InputData inputs;
    private float accelVal = 0;
    private float brakeVal = 0;
    private float turnVal = 0;
    private Vector2 cameraOffset = Vector2.zero;

    // Audio
    private AudioSource audioSrc;
    public AudioClip honkSFX;

    // Fields
    public static List<Player> players = new List<Player>();
    [NonSerialized]
    public int id;
    [NonSerialized]
    public bool isAlive;

    // Properties
    public Vector3 Position { get { return car.transform.position; } }

    // Start is called before the first frame update
    void Start()
    {
        id = players.Count;
        players.Add(this);

        isAlive = true;

        // Car initialization
        car.AssignOwner(this);
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
                deadPlayerId = id
            });
        };

        // Weapon initialization
        if (weapon != null) weapon.Initialize(id, car);

        audioSrc = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        // Input handling
        GatherInputs();
        car.SendInputs(inputs);
    }

    public void Respawn(Vector3 pos, Quaternion rot)
    {
        // Reset player data
        isAlive = true;
        car.transform.SetPositionAndRotation(pos, rot);
        car.ResetCar();
        weapon.ResetWeapons();
        weapon.enabled = true;
    }

    void GatherInputs()
    {
        inputs = new InputData
        {
            Acceleration = accelVal,
            Braking = brakeVal,
            Turning = turnVal
        };
    }

    void OnAccelerate(InputValue val)
    {
        accelVal = val.Get<float>();
    }
    void OnBrake(InputValue val)
    {
        brakeVal = val.Get<float>();
    }
    void OnTurn(InputValue val)
    {
        turnVal = val.Get<float>();
    }
    void OnAim(InputValue val)
    {
        cameraOffset = val.Get<Vector2>();

        camera.transform.localPosition = new Vector3(cameraOffset.x, 0, cameraOffset.y) * 2;
    }
    void OnMenuToggle()
    {
        EventManager.Instance.ToggleMenu();
    }

    void OnHonk()
    {
        if (audioSrc != null && honkSFX != null)
            audioSrc.PlayOneShot(honkSFX);
    }
}
