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

    // Audio Fields
    private AudioSource audioSrc;
    public AudioClip shootSFX;
    public AudioClip impactSFX;

    // Lob Fields
    public bool isExploding = false;
    
    void Awake()
    {
        audioSrc = GetComponent<AudioSource>();
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
                break;

            case AttackType.Homing:
                if (attackInfo.targetId > -1)
                {
                    Player target = PlayerManager.Instance.Players[attackInfo.targetId];

                    // Steer towards target, excluding Y axis
                    UnityEngine.Vector2 desVel = new (transform.position.x - target.Position.x, transform.position.z - target.Position.z);
                    desVel = desVel.normalized * speed;
                    rbody.AddForce(rbody.velocity.x - desVel.x, 0, rbody.velocity.z - desVel.y, ForceMode.Impulse);
                }
                break;

            case AttackType.Lob:
                break;
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

        // Collision behavior based on attack type
        switch (attackInfo.type)
        {
            case AttackType.Shot:
                if (audioSrc != null && impactSFX != null)
                    audioSrc.PlayOneShot(impactSFX);
                EndTrajectory();
                break;

            case AttackType.Homing:
                EndTrajectory();
                break;

            case AttackType.Lob:
                if (!isExploding) // Start explosion on contact
                {
                    isExploding = true;
                    rbody.useGravity = false;
                    rbody.velocity = UnityEngine.Vector3.zero;
                    transform.localScale = UnityEngine.Vector3.one * 10; // Blast radius
                    lifeTime = ElapsedTime + 0.5f; // Explosion lasts for 0.5s

                    // Play impact audio on explosion
                    if (audioSrc != null && impactSFX != null)
                        audioSrc.PlayOneShot(impactSFX);
                }
                // If exploding, deals damage to any colliders entering until expiration
                break;
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

        // Shoot SFX
        if (audioSrc != null && shootSFX != null)
            audioSrc.PlayOneShot(shootSFX);

        switch (info.type)
        {
            case AttackType.Shot:
                rbody.useGravity = false;
                lifeTime = 3;
                rbody.AddForce(Mathf.Sin(direction) * speed, 0, Mathf.Cos(direction) * speed, ForceMode.Impulse);
                break;

            case AttackType.Lob:
                rbody.useGravity = true; // Creates arc trajectory
                lifeTime = 1000; // Need a better method for ending lob, lob lifetime is arc time + explosion time
                rbody.AddForce(Mathf.Sin(direction) * speed, 10, Mathf.Cos(direction) * speed, ForceMode.Impulse);
                break;

            case AttackType.Homing:
                rbody.useGravity = false;
                lifeTime = 5;

                // Start moving in original direction
                rbody.maxLinearVelocity = speed;
                rbody.velocity = new UnityEngine.Vector3(Mathf.Sin(direction) * speed, 0, Mathf.Cos(direction) * speed);
                break;
        }
    }

    public void EndTrajectory(){

        // Homing bullets always play impact SFX when expiring
        if (attackInfo.type == AttackType.Homing)
        {
            if (audioSrc != null && impactSFX != null)
                audioSrc.PlayOneShot(impactSFX);
        }

        // Reset variables/flags and rigidbody
        isAlive = false;
        isExploding = false;
        speed = 0;
        direction = 0;
        rbody.velocity = UnityEngine.Vector3.zero;
        transform.position = new UnityEngine.Vector3(transform.position.x, -20, transform.position.y);
        transform.localScale = UnityEngine.Vector3.one * 0.25f; // Original bullet scale
    }
}
