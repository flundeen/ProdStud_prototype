using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KartGame.KartSystems;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class PizzaWeapon : Weapon
{
    // Pizza Car Fields
    public GameObject bulletPrefab;
    private List<Bullet_Script> bulletPool;
    private ArcadeKart.StatPowerup speedBoost;
    public ArcadeKart.Stats speedBoostEffect = new ArcadeKart.Stats
    {
        Acceleration = 15f,
        TopSpeed = 20f
    };
    private bool isBoostActive = false;

    // Start is called before the first frame update
    void Start()
    {
        // Create bullet pool for primary weapon
        bulletPool = new List<Bullet_Script>();
        GenerateBullets(20);

        // Define gadget parameters
        speedBoost = new ArcadeKart.StatPowerup(speedBoostEffect, "On-Time Speed Boost", 5);
    }

    // Update is called once per frame
    void Update()
    {
        // Update weapon cooldowns
        primaryClock.Update(Time.deltaTime);
        gadgetClock.Update(Time.deltaTime);

        // Fire primary while input is held
        if (isPrimaryActive)
        {
            if (primaryClock.IsReady)
            {
                FirePrimary();
                primaryClock.Start();
            }
        }
    }

    // Pizza Car Primary: Pepperoni Popper
    // Fire 5 shots/sec at 40 DPS (8 dmg each) whle active
    public override void OnPrimary(InputValue val)
    {
        if (!enabled) return;

        base.OnPrimary(val);
    }

    // Pizza Car Gadget: Running Late
    // 1.5x top speed and acceleration for 5s
    // Cooldown: 15s (after boost expires)
    public override void OnGadget()
    {
        if (!enabled) return;

        if (gadgetClock.IsReady)
        {
            isBoostActive = true;
            speedBoost.Reset(); // resets elapsed time
            car.AddPowerup(speedBoost);
            gadgetClock.Start();
        }
    }

    // Resets primary and gadget states for respawning
    public override void ResetWeapons()
    {
        // Resets timers
        base.ResetWeapons();

        // Reset primary pool
        foreach (Bullet_Script b in bulletPool)
        {
            // Reset all bullets to inactive state
            if (b.isAlive)
                b.EndTrajectory();
        }

        // Reset gadget data
        isBoostActive = false;
    }

    void GenerateBullets(int count)
    {
        for (int i = 0; i < count; i++)
            bulletPool.Add(Instantiate(bulletPrefab, new Vector3(i * 4, -10, 5), bulletPrefab.transform.rotation).GetComponent<Bullet_Script>());
    }

    void FirePrimary()
    {
        //get position of kart
        Vector3 position = car.transform.position;

        // get firing angle based on car direction and aim vector
        float aimAngle = Mathf.Atan2(aimVector.x, aimVector.y) + (car.transform.rotation.eulerAngles.y * Mathf.Deg2Rad);

        // makes position adjustments based on player position
        // Constant multiplier based on car size, higher = further from center
        position.x += Mathf.Sin(aimAngle) * 2;
        position.z += Mathf.Cos(aimAngle) * 2;
        position.y += 1;

        // Find inactive bullet
        Bullet_Script bullet = bulletPool.Find(b => b.isAlive == false);

        // If no inactive bullets, create one
        if (bullet == null)
        {
            GenerateBullets(1);
            bullet = bulletPool[^1]; // This is simplified from (bulletPool.Count - 1)??? if it works it works ig
        }

        //sets this player to the shooter so bullets won't damage them and points are rewarded to shooter
        bullet.Shoot(new AttackInfo(playerId, AttackType.Shot, primaryDamage, 25), position, aimAngle);
    }
}
