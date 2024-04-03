using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.UI;

public class Selection_Display : MonoBehaviour
{
    public enum Cars{
        PizzaCar,
        MailTruck
    }

    public Cars choice;

    public InputDevice playerInput;
    public GameObject carName;
    public GameObject health_val;
    public GameObject speed_val;
    public GameObject accel_val;
    public GameObject handle_val;
    public GameObject weight_val;
    public GameObject PizzaCar;
    public GameObject MailTruck;
    public float Spin_Speed;
    private GameObject DisplayCar;
    private TextMeshProUGUI carName_text;
    private TextMeshProUGUI health_val_text;
    private TextMeshProUGUI speed_val_text;
    private TextMeshProUGUI accel_val_text;
    private TextMeshProUGUI handle_val_text;
    private TextMeshProUGUI weight_val_text;


    private float changeEl;

    // Start is called before the first frame update
    void Awake()
    {
        choice = Cars.PizzaCar;

        if(Spin_Speed == null)
        {
            Spin_Speed = 0.5f;
        }

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

        changeEl = 0f;

        DisplayCar = PizzaCar;
    }

    // Update is called once per frame
    void Update()
    {
        changeEl += Time.deltaTime;

        if(changeEl > 2.8)
        {
            OnJoystick();
            changeEl = 0;
        }

        if(choice == Cars.PizzaCar)
        {
        carName_text.text = "Pizza Car";
        health_val_text.text = "80";
        speed_val_text.text = "3";
        accel_val_text.text = "3";
        handle_val_text.text = "2";
        weight_val_text.text = "3";
        DisplayCar.SetActive(false);
        DisplayCar = PizzaCar;
        DisplayCar.SetActive(true);
        }
        else if(choice == Cars.MailTruck)
        {
            carName_text.text = "Mail Truck";
        health_val_text.text = "125";
        speed_val_text.text = "2";
        accel_val_text.text = "2";
        handle_val_text.text = "3";
        weight_val_text.text = "4";
        DisplayCar.SetActive(false);
        DisplayCar = MailTruck;
        DisplayCar.SetActive(true);
        }

        DisplayCar.transform.Rotate(new Vector3(0, 1, 0), Spin_Speed);
    }

    public  void OnJoystick(){
        if(choice == Cars.MailTruck)
        {
            choice = Cars.PizzaCar;
        }
        else
        {
            choice++;
        }
    }
}
