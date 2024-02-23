using KartGame.KartSystems;
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
    private Vector2 aimVector = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        car = GetComponent<ArcadeKart>();
        weapon = GetComponent<Weapon>();
        weapon.SetSource(car);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Camera
    // Handle input here
    void GatherInputs()
    {
        inputs = new InputData
        {
            Acceleration = accelVal,
            Braking = brakeVal,
            Turning = turnVal,
            AimAngle = Mathf.Atan2(aimVector.x, aimVector.y) + (car.transform.rotation.eulerAngles.y * Mathf.Deg2Rad)
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
        aimVector = val.Get<Vector2>();

        camera.transform.localPosition = new Vector3(aimVector.x, 0, aimVector.y) * 2;
    }
}
