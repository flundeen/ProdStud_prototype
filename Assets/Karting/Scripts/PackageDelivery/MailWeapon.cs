using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MailWeapon : Weapon
{
    // Mail Car Fields
    public GameObject lobPrefab;
    private List<Bullet_Script> lobPool;
    public GameObject dronePrefab;
    private List<Bullet_Script> dronePool;
    private float primaryCharge; // Float between 0-1, 1 being fully charged

    // Start is called before the first frame update
    void Start()
    {
        // Create bullet pools for primary and gadget
        lobPool = new List<Bullet_Script>();
        GenerateBullets(5, lobPrefab, lobPool);
        dronePool = new List<Bullet_Script>();
        GenerateBullets(3, dronePrefab, dronePool);
    }

    // Update is called once per frame
    void Update()
    {
        primaryClock.Update(Time.deltaTime);
        gadgetClock.Update(Time.deltaTime);

        // Don't process primary weapon on cooldown
        if (!primaryClock.IsReady) return;

        if (isPrimaryActive)
        {
            // Charge primary for up to 0.5s
            primaryCharge = Mathf.Min(primaryCharge + Time.deltaTime * 2, 1);
        }
        else
        {
            // If primary input up and primary is charged, fire a lob!
            if (primaryCharge > 0)
            {
                FirePrimary();
                primaryCharge = 0;
                primaryClock.Start();
            }
        }
    }

    // Mail Truck Primary: First-Class Catapult
    // Lob AOE attack, holding primary input increases travel distance until release
    // 50 dmg, 0.5s between shots, charge for up to 0.5s, 0.5s from release until impact
    public override void OnPrimary(InputValue val)
    {
        if (!enabled) return;

        base.OnPrimary(val);
    }

    // Mail Truck Gadget: Drone Swarm
    // Spawn 3 homing drones
    // Last for 5s, deal 20 dmg each
    // Cooldown: 20s
    public override void OnGadget()
    {
        if (!enabled) return;

        // Gadget cooldown must be over
        if (!gadgetClock.IsReady) return;
        gadgetClock.Start();

        // Shoot 3 homing bullets
        for (int i = 0; i < 3; i++)
        {
            //get position of kart to find bullet spawn point
            Vector3 position = car.transform.position;

            // Fire at 0-120-240 angles
            float aimAngle = (i * 120 + car.transform.rotation.eulerAngles.y) * Mathf.Deg2Rad;

            // move bullet out of mesh
            position.x += Mathf.Sin(aimAngle) * 2;
            position.z += Mathf.Cos(aimAngle) * 2;
            position.y += 1;

            // Find inactive bullet
            Bullet_Script bullet = dronePool.Find(b => !b.isAlive);

            // If no inactive bullets, create one
            if (bullet == null)
            {
                GenerateBullets(1, dronePrefab, dronePool);
                bullet = lobPool[^1]; // This is simplified from (bulletPool.Count - 1)??? if it works it works ig
            }

            // Search for closest player
            Player target = null;
            float currDist = 0;
            foreach (Player p in GameManager.Instance.players)
            {
                if (p.id == playerId) continue;

                float dist = Vector3.Distance(car.transform.position, p.Position);

                if (target == null || dist < currDist)
                {
                    target = p;
                    currDist = dist;
                }
            }

            // Activate bullet with set target (if found)
            if (target != null)
                bullet.Shoot(new AttackInfo(playerId, AttackType.Homing, 20, 20, target.id), position, aimAngle);
            else
                bullet.Shoot(new AttackInfo(playerId, AttackType.Homing, 20, 20, -1), position, aimAngle);
        }
    }

    void GenerateBullets(int count, GameObject prefab, List<Bullet_Script> pool)
    {
        for (int i = 0; i < count; i++)
            pool.Add(Instantiate(prefab, new Vector3(i * 4, -10, 5), prefab.transform.rotation).GetComponent<Bullet_Script>());
    }

    void FirePrimary()
    {
        //get position of kart to find bullet spawn point
        Vector3 position = car.transform.position;
        position.y += 2;

        // get firing angle based on car direction and aim vector
        float aimAngle = Mathf.Atan2(aimVector.x, aimVector.y) + (car.transform.rotation.eulerAngles.y * Mathf.Deg2Rad);

        // Find inactive bullet
        Bullet_Script bullet = lobPool.Find(b => !b.isAlive);

        // If no inactive bullets, create one
        if (bullet == null)
        {
            GenerateBullets(1, lobPrefab, lobPool);
            bullet = lobPool[^1]; // This is simplified from (bulletPool.Count - 1)??? if it works it works ig
        }

        // Set range using primaryCharge
        int speed = (int)Mathf.Round(Mathf.Lerp(2f, 15f, primaryCharge)); // Min-max ranges

        bullet.Shoot(new AttackInfo(playerId, AttackType.Lob, primaryDamage, speed), position, aimAngle);
    }

    public override void ResetWeapons()
    {
        // Resets cooldown clocks
        base.ResetWeapons();

        // Reset bullet pool
        foreach (Bullet_Script b in lobPool)
        {
            // Reset all bullets to inactive state
            if (b.isAlive)
                b.EndTrajectory();
        }

        // Reset primary data
        isPrimaryActive = false;
        primaryCharge = 0;
    }
}
