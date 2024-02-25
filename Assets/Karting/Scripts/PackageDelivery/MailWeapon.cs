using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MailWeapon : Weapon
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
    }
}
