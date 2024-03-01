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
                return;

            case AttackType.Homing:
                if (attackInfo.targetId > -1)
                {
                    Player target = GameManager.Instance.players[attackInfo.targetId];

                    // Steer towards target, excluding Y axis
                    UnityEngine.Vector2 desVel = new (transform.position.x - target.Position.x, transform.position.z - target.Position.z);
                    desVel = desVel.normalized * speed;
                    rbody.AddForce(rbody.velocity.x - desVel.x, 0, rbody.velocity.z - desVel.y, ForceMode.Impulse);
                }
                return;

            case AttackType.Lob:
                return;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Skip if inactive or hitting trigger
        if (!isAlive || other.isTrigger) return;

        // Car hitbox are boxcolliders only
        if (other is BoxCollider)
        {
            // Attempt to call ArcadeKart's TakeDamage by getting hitbox's parent's ArcadeKart component
            if (other.transform.parent != null)
            {
                if (other.transform.parent.TryGetComponent<ArcadeKart>(out var target))
                {
                    // If attack fails (self-attack), dont stop bullet
                    if (!target.TakeDamage(attackInfo))
                        return;
                }
            }
        }

        switch (attackInfo.type)
        {
            case AttackType.Shot:
            case AttackType.Homing:
                EndTrajectory();
                return;

            case AttackType.Lob:
                if (!isExploding) // Start explosion on contact
                {
                    isExploding = true;
                    rbody.useGravity = false;
                    rbody.velocity = UnityEngine.Vector3.zero;
                    transform.localScale = UnityEngine.Vector3.one * 10; // Blast radius
                }
                // If exploding, deals damage to any colliders entering until expiration
                return;
        }
    }

    public void Shoot(AttackInfo info, UnityEngine.Vector3 pos, float aimAngle)
    {
        isAlive = true;
        ElapsedTime = 0;
        transform.position = pos;
        rbody.velocity = UnityEngine.Vector3.zero;
        direction = aimAngle;
        attackInfo = info;
        speed = info.speed;

        switch (info.type)
        {
            case AttackType.Shot:
                rbody.useGravity = false;
                lifeTime = 3;
                rbody.AddForce(Mathf.Sin(direction) * speed, 0, Mathf.Cos(direction) * speed, ForceMode.Impulse);
                return;

            case AttackType.Lob:
                rbody.useGravity = true; // Creates arc trajectory
                lifeTime = 2; // Need a better method for ending lob, lob lifetime is arc time + explosion time
                rbody.AddForce(Mathf.Sin(direction) * speed, 5, Mathf.Cos(direction) * speed, ForceMode.Impulse);
                return;

            case AttackType.Homing:
                rbody.useGravity = false;
                lifeTime = 5;

                // Start moving in original direction
                rbody.maxLinearVelocity = speed;
                rbody.velocity = new UnityEngine.Vector3(Mathf.Sin(direction) * speed, 0, Mathf.Cos(direction) * speed);
                return;
        }
    }

    public void EndTrajectory(){
        isAlive = false;
        isExploding = false;
        speed = 0;
        direction = 0;
        rbody.velocity = UnityEngine.Vector3.zero;
        transform.position = new UnityEngine.Vector3(transform.position.x, -20, transform.position.y);
        transform.localScale = UnityEngine.Vector3.one * 0.25f; // Original bullet scale
    }
}
