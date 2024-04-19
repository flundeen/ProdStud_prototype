using KartGame.KartSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Selection_Display : MonoBehaviour
{
    // Player Data
    public Player player;
    public CarType choice;

    // Text Labels
    public GameObject carName;
    public GameObject health_val;
    public GameObject speed_val;
    public GameObject accel_val;
    public GameObject handle_val;
    public GameObject weight_val;
    private TextMeshProUGUI carName_text;
    private TextMeshProUGUI health_val_text;
    private TextMeshProUGUI speed_val_text;
    private TextMeshProUGUI accel_val_text;
    private TextMeshProUGUI handle_val_text;
    private TextMeshProUGUI weight_val_text;

    // Car Models
    public GameObject PizzaCar;
    public GameObject MailTruck;

    // Display Data
    private float spinSpeed = 0.5f;
    private GameObject DisplayCar;

    public bool IsReady
    {
        get { return player.isSelectionConfirmed; }
    }

    // Start is called before the first frame update
    void Awake()
    {
        carName_text = carName.GetComponent<TextMeshProUGUI>();
        health_val_text = health_val.GetComponent<TextMeshProUGUI>();
        speed_val_text = speed_val.GetComponent<TextMeshProUGUI>();
        accel_val_text = accel_val.GetComponent<TextMeshProUGUI>();
        handle_val_text = handle_val.GetComponent<TextMeshProUGUI>();
        weight_val_text = weight_val.GetComponent<TextMeshProUGUI>();

        carName_text.text = "Pizza Car";
        health_val_text.text = "80";
        speed_val_text.text = "3";
        accel_val_text.text = "3";
        handle_val_text.text = "2";
        weight_val_text.text = "3";
        DisplayCar = PizzaCar;
        DisplayCar.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        // Do nothing if no player connected
        if (player == null) return;

        if (choice != player.carType)
        {
            choice = player.carType;

            // Update display
            switch (choice)
            {
                case CarType.Pizza:
                    carName_text.text = "Pizza Car";
                    health_val_text.text = "80";
                    speed_val_text.text = "3";
                    accel_val_text.text = "3";
                    handle_val_text.text = "2";
                    weight_val_text.text = "3";
                    DisplayCar.SetActive(false);
                    DisplayCar = PizzaCar;
                    DisplayCar.SetActive(true);
                    break;

                case CarType.Mail:
                    carName_text.text = "Mail Truck";
                    health_val_text.text = "125";
                    speed_val_text.text = "2";
                    accel_val_text.text = "2";
                    handle_val_text.text = "3";
                    weight_val_text.text = "4";
                    DisplayCar.SetActive(false);
                    DisplayCar = MailTruck;
                    DisplayCar.SetActive(true);
                    break;
            }
        }

        // Rotate while car is unselected
        if (IsReady)
            DisplayCar.transform.rotation = Quaternion.identity;
        else
            DisplayCar.transform.Rotate(new Vector3(0, 1, 0), spinSpeed);
    }
}
