using KartGame.KartSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
    Shot,
    Homing,
    Lob,
    Ram
}

public struct AttackInfo
{
    public ArcadeKart attacker;
    public int attackerId; // -1 if not player
    public int damage;

    public AttackInfo(ArcadeKart attacker, int attackerId, int damage)
    {
        this.attacker = attacker;
        this.attackerId = attackerId;
        this.damage = damage;
    }
}

public class Weapon : MonoBehaviour
{
    // Fields
    protected InputData inputs;
    protected ArcadeKart car;

    // Primary Fields
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

    public void SetSource(ArcadeKart car)
    {
        this.car = car;
    }

    public void SendInput(InputData input)
    {
        inputs = input;
    }

    virtual public void OnPrimary()
    {
        isPrimaryActive = !isPrimaryActive;

        return;
    }

    virtual public void OnGadget()
    {
        return;
    }
}
