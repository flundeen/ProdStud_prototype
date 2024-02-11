using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_Script : MonoBehaviour
{
        public Rigidbody rbody;
    public float speed;
    public int maxSpeed;
    public float direction;
    public int damage;
    public GameObject shooter;

    private float lifeTime = 10;
    private float ElapsedTime = 0;

    void Awake()
    {

        Quaternion target = Quaternion.Euler(0, direction + 90, 0);

        gameObject.GetComponent<Transform>().rotation = target;

    }

    void FixedUpdate()
    {
        if(speed > 0)
        {
            ElapsedTime += Time.fixedDeltaTime;
        }
        if(ElapsedTime > lifeTime)
        {
            Destroy(gameObject);
        }
        if(rbody.velocity.magnitude <= maxSpeed)
        rbody.AddForce(Mathf.Sin(direction * Mathf.Deg2Rad) * speed, 0, Mathf.Cos(direction * Mathf.Deg2Rad) * speed, ForceMode.Impulse);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject != shooter.GetComponentInChildren<CapsuleCollider>())
        {
            Destroy(gameObject);
        }   
    }
}
