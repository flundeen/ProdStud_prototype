using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Package : MonoBehaviour
{

    private float timer = 5.0f;
    private bool isPickupEnabled = false;

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;

        // Allow time for dropping car to be moved away
        if (timer <= 4.8f) isPickupEnabled = true;
        
        if (timer <= 0f)
        {
            GameManager.Instance.packageIsPresent = false;
            GameManager.Instance.packageHolder = null;
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (isPickupEnabled && collision.CompareTag("Player") && !collision.GetComponentInParent<KartPackage>().hasPackage)
        {
            collision.GetComponentInParent<KartPackage>().hasPackage = true;
            GameManager.Instance.packagePickedUp = true;
            Destroy(gameObject);
        }
    }
}
