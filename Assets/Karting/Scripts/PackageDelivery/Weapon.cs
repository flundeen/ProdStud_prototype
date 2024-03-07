using KartGame.KartSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum AttackType
{
    Shot,
    Homing,
    Lob,
    Ram,
    Hazard
}

public struct AttackInfo
{
    public int attackerId; // -1 if not player
    public int targetId; // Used by homing attacks
    public AttackType type;
    public int damage;
    public int speed;

    public AttackInfo(int attackerId, AttackType type, int damage, int speed, int targetId = -1)
    {
        this.attackerId = attackerId;
        this.targetId = targetId;
        this.type = type;
        this.damage = damage;
        this.speed = speed;
    }
}

public class Weapon : MonoBehaviour
{
    // Fields
    protected int playerId;
    protected ArcadeKart car;
    protected Vector2 aimVector = Vector2.zero;

    // Primary Fields
    public int primaryDamage;
    public float primaryCooldown;
    protected Timer primaryClock;
    protected bool isPrimaryActive;

    // Gadget Fields
    public float gadgetCooldown;
    protected Timer gadgetClock;

    void Awake()
    {
        primaryClock = new Timer(primaryCooldown);
        gadgetClock = new Timer(gadgetCooldown);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(int playerId, ArcadeKart car)
    {
        this.playerId = playerId;
        this.car = car;
    }

    virtual public void OnPrimary(InputValue val)
    {
        isPrimaryActive = val.Get<float>() > 0;
    }

    // For AI use
    public void OnPrimary(float val)
    {
        isPrimaryActive = val > 0;
    }

    virtual public void OnGadget()
    {
        return;
    }

    virtual public void OnAim(InputValue val)
    {
        aimVector = val.Get<Vector2>();
    }

    virtual public void ResetWeapons()
    {
        primaryClock.End();
        gadgetClock.End();
    }
}
