using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using KartGame.KartSystems;
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
    public float ElapsedTime = 0;
    public bool isAlive = false;
    public AttackInfo attackInfo;
    
    void Awake()
    {
        UnityEngine.Quaternion target = UnityEngine.Quaternion.Euler(0, direction + 90, 0);

        gameObject.GetComponent<Transform>().rotation = target;
    }

    void FixedUpdate()
    {
        if(isAlive)
        {
            ElapsedTime += Time.fixedDeltaTime;

            if (rbody.velocity.magnitude <= maxSpeed)
                rbody.AddForce(Mathf.Sin(direction) * speed, 0, Mathf.Cos(direction) * speed, ForceMode.Impulse);

            if (ElapsedTime >= lifeTime)
            {
                EndTrajectory();
                ElapsedTime = 0;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Skip if inactive
        if (!isAlive) return;

        // Car hitbox are boxcolliders only
        if (other is BoxCollider)
        {
            if (other.transform.parent != null)
            {
                // Attempt to call ArcadeKart's TakeDamage by getting hitbox's parent's ArcadeKart component
                if (other.transform.parent.TryGetComponent<ArcadeKart>(out var target))
                {
                    // If attack succeeds, stop bullet
                    if (target.TakeDamage(attackInfo))
                        EndTrajectory();
                    else // If attack fails (self-attack), pass through collider
                        return;
                }
            }
        }

        EndTrajectory();

        // PREVIOUS CODE
        //if(other.gameObject != shooter.GetComponentInChildren<CapsuleCollider>())
        //{
        //    EndTrajectory();
        //    if(other.gameObject.tag == "Player")
        //    {
        //        other.gameObject.GetComponent<ArcadeKart>().TakeDamage(damage);
        //    }
        //}   
    }

    public void Shoot(AttackInfo info, UnityEngine.Vector3 pos, float aimAngle)
    {
        isAlive = true;
        ElapsedTime = 0;
        transform.position = pos;
        rbody.velocity = UnityEngine.Vector3.zero;
        direction = aimAngle;
        attackInfo = info;
        speed = 24; // Maybe these are also parameters?
        maxSpeed = 25;
    }

    public void EndTrajectory(){
        isAlive = false;
        speed = 0;
        direction = 0;
        gameObject.GetComponent<Rigidbody>().velocity = UnityEngine.Vector3.zero;
        transform.position = new UnityEngine.Vector3(transform.position.x, -20, transform.position.y);
    }
}
