using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using KartGame.KartSystems;
using UnityEngine.InputSystem;

public enum AiBehavior
{
    Sit,
    SitAttack,
    FollowPath
}

public class AiController : MonoBehaviour
{
    // Fields
    // Uses id -1
    public ArcadeKart car;
    public Weapon weapon;
    public AiBehavior behaviorType;
    private Timer attackTimer;
    private bool isAttacking;

    // Start is called before the first frame update
    void Start()
    {
        if (weapon != null)
            weapon.Initialize(-1, car);
        attackTimer = new Timer(1);
    }

    // Update is called once per frame
    void Update()
    {
        switch (behaviorType)
        {
            case AiBehavior.Sit:
                break;

            case AiBehavior.SitAttack: // Alternates attacking and resting every second
                // Update attack timer
                attackTimer.Update(Time.deltaTime);

                // Switch attack state when timer goes off
                if (attackTimer.IsReady)
                {
                    isAttacking = !isAttacking;
                    attackTimer.Start();
                }

                // Send attack data
                weapon.OnPrimary(isAttacking ? 1 : 0);
                break;

            case AiBehavior.FollowPath: 
                break;
        }
    }
}
