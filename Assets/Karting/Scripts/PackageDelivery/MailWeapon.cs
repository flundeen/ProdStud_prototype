using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MailWeapon : Weapon
{
    // Mail Car Fields
    public GameObject bulletPrefab;
    private List<Bullet_Script> bulletPool;
    private float primaryCharge;

    // Start is called before the first frame update
    void Start()
    {
        // Create bullet pool for primary weapon
        bulletPool = new List<Bullet_Script>();
        GenerateBullets(5);

    }

    // Update is called once per frame
    void Update()
    {
        if (isPrimaryActive)
        {
            // Charge primary for up to 0.5s
            primaryCharge = Mathf.Min(primaryCharge + Time.deltaTime * 2, 1);
            Debug.Log("Primary Charge: " + primaryCharge);
        }
        else
        {
            // If primary input up and primary is charged, fire a lob!
            if (primaryCharge > 0)
            {
                Debug.Log("Firing primary!");
                FirePrimary();
                primaryCharge = 0;
            }
        }
    }

    // Mail Truck Primary: First-Class Catapult
    // Lob AOE attack, holding primary input increases travel distance until release
    // 50 dmg, 0.5s between shots, charge for up to 0.5s, 0.5s from release until impact
    public override void OnPrimary(InputValue val)
    {
        base.OnPrimary(val);
    }

    // Mail Truck Gadget: Drone Swarm
    // Spawn 3 homing drones
    // Last for 5s, deal 20 dmg each
    // Cooldown: 20s
    public override void OnGadget()
    {
        base.OnGadget();

        // Shoot 3 homing bullets at 0-120-240 angles
    }

    void GenerateBullets(int count)
    {
        for (int i = 0; i < count; i++)
            bulletPool.Add(Instantiate(bulletPrefab, new Vector3(i * 4, -10, 5), bulletPrefab.transform.rotation).GetComponent<Bullet_Script>());
    }

    void FirePrimary()
    {
        //get position of kart to find bullet spawn point
        Vector3 position = car.transform.position;
        position.y += 2;

        // get firing angle based on car direction and aim vector
        float aimAngle = Mathf.Atan2(aimVector.x, aimVector.y) + (car.transform.rotation.eulerAngles.y * Mathf.Deg2Rad);

        // Find inactive bullet
        Bullet_Script bullet = bulletPool.Find(b => b.isAlive == false);

        // If no inactive bullets, create one
        if (bullet == null)
        {
            GenerateBullets(1);
            bullet = bulletPool[^1]; // This is simplified from (bulletPool.Count - 1)??? if it works it works ig
        }

        // Set range using primaryCharge

        bullet.Shoot(new AttackInfo(playerId, AttackType.Lob, primaryDamage), position, aimAngle);
    }
}
