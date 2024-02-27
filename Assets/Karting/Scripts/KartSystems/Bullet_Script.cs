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
    private float lifeTime;
    public float ElapsedTime = 0;
    public bool isAlive = false;
    public AttackInfo attackInfo;

    // Lob Fields
    public bool isExploding = false;
    
    void Awake()
    {
        //UnityEngine.Quaternion target = UnityEngine.Quaternion.Euler(0, direction + 90, 0);

        //gameObject.GetComponent<Transform>().rotation = target;
    }

    void FixedUpdate()
    {
        if (isAlive)
        {
            ElapsedTime += Time.fixedDeltaTime;

            Move();

            if (ElapsedTime >= lifeTime)
            {
                EndTrajectory();
                ElapsedTime = 0;
            }
        }
    }

    public void Move()
    {
        switch (attackInfo.type)
        {
            case AttackType.Shot:
                if (rbody.velocity.magnitude <= speed)
                    rbody.AddForce(Mathf.Sin(direction) * speed, 0, Mathf.Cos(direction) * speed, ForceMode.Impulse);
                return;

            case AttackType.Lob:
                //if (rbody.velocity.magnitude <= speed)
                //    rbody.AddForce(Mathf.Sin(direction) * speed, 0, Mathf.Cos(direction) * speed, ForceMode.Impulse);
                //rbody.velocity = new UnityEngine.Vector3(Mathf.Sin(direction) * speed, rbody.velocity.y - Time.fixedDeltaTime * 2, Mathf.Cos(direction) * speed);
                //rbody.AddForce(0, -4 * Time.fixedDeltaTime, 0, ForceMode.VelocityChange);
                return;

            case AttackType.Homing:
                return;
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

        switch (info.type)
        {
            case AttackType.Shot:
                speed = 24;
                maxSpeed = 25;
                lifeTime = 3;
                return;

            case AttackType.Lob:
                speed = 12;
                lifeTime = 100; // Lifetime is defined by arc
                rbody.velocity = UnityEngine.Vector3.up * 2;
                //rbody.AddForce(Mathf.Sin(direction) * speed, 100, Mathf.Cos(direction) * speed, ForceMode.Impulse);
                // Vertical impluse to start arc
                //rbody.AddForce(0, 10, 0, ForceMode.Impulse);
                return;

            case AttackType.Homing:
                speed = 20;
                lifeTime = 5;
                return;
        }
    }

    public void EndTrajectory(){
        isAlive = false;
        speed = 0;
        direction = 0;
        gameObject.GetComponent<Rigidbody>().velocity = UnityEngine.Vector3.zero;
        transform.position = new UnityEngine.Vector3(transform.position.x, -20, transform.position.y);
    }
}
