using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PizzaWeapon : Weapon
{
    // Pizza Car Fields
    public GameObject bulletPrefab;
    private List<Bullet_Script> bulletPool;
    // Speedboost here

    // Start is called before the first frame update
    void Start()
    {
        bulletPool = new List<Bullet_Script>();
        GenerateBullets(20);
    }

    // Update is called once per frame
    void Update()
    {
        primaryClock.Update(Time.deltaTime);
        gadgetClock.Update(Time.deltaTime);

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
    public override void OnPrimary()
    {
        base.OnPrimary();


    }

    // Pizza Car Gadget: Running Late
    // 1.5x top speed and acceleration for 5s
    // Cooldown: 15s (after boost expires)
    public override void OnGadget()
    {
        base.OnGadget();

        if (gadgetClock.IsReady)
            return; // Do gadget
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

        //makes position adjustments based on player position
        position.x += Mathf.Sin(inputs.AimAngle) * 4;
        position.z += Mathf.Cos(inputs.AimAngle) * 4;
        position.y += 1;

        //creates bullet
        Bullet_Script bullet = bulletPool.Find(b => b.isAlive == false);

        // If no inactive bullets, generate one
        if (bullet == null)
        {
            GenerateBullets(1);
            bullet = bulletPool[bulletPool.Count - 1];
        }

        //sets this player to the shooter so bullets won't damage them and points are rewarded to shooter
        bullet.Shoot(new AttackInfo(car, -1, 10), position, inputs.AimAngle);
    }
}
