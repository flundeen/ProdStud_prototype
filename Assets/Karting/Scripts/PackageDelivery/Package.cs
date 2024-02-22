using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Package : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Player" && !collision.GetComponentInParent<KartPackage>().hasPackage)
        {
            collision.GetComponentInParent<KartPackage>().hasPackage = true;
            Destroy(gameObject);
        }
    }
}
