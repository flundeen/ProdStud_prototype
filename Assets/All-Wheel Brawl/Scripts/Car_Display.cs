using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public float Spin_Speed;
    // Start is called before the first frame update
    void Start()
    {
        if(Spin_Speed == null)
        {
            Spin_Speed = 0.5f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(new Vector3(0, 1, 0), Spin_Speed);
    }
}
