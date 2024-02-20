using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class Bullet_Script : MonoBehaviour
{
    public Rigidbody rbody;
    public float speed;
    public int maxSpeed;
    public float direction;
    public int damage;
    public GameObject shooter;
    public int bullet_num;
    private float lifeTime = 10;
    public float ElapsedTime = 0;
    
    void Awake()
    {

        UnityEngine.Quaternion target = UnityEngine.Quaternion.Euler(0, direction + 90, 0);

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
            EndTrajectory();
            ElapsedTime = 0;
        }
        if(rbody.velocity.magnitude <= maxSpeed)
        rbody.AddForce(Mathf.Sin(direction) * speed, 0, Mathf.Cos(direction) * speed, ForceMode.Impulse);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject != shooter.GetComponentInChildren<CapsuleCollider>())
        {
            EndTrajectory();
        }   
    }

    void EndTrajectory(){
        speed = 0;
        direction = 0;
        gameObject.GetComponent<Rigidbody>().velocity = UnityEngine.Vector3.zero;
        transform.position = new UnityEngine.Vector3(transform.position.x, -20, transform.position.y);
    }
}
