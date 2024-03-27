using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Package : MonoBehaviour
{

    private float timer = 5.0f;

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            GameManager.Instance.packageIsPresent = false;
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Player" && !collision.GetComponentInParent<KartPackage>().hasPackage)
        {
            collision.GetComponentInParent<KartPackage>().hasPackage = true;
            GameManager.Instance.packagePickedUp = true;
            Destroy(gameObject);
        }
    }
}
