using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    // Fields
    private float currTime = 0;
    private float length = 0;

    // Properties
    public bool IsReady
    { get { return currTime <= 0; } }
    public float Time 
    { get { return currTime; } }

    // Constructor
    public Timer(float length, bool startAtZero = true)
    {
        this.length = length;
        if (!startAtZero)
            currTime = length;
    }

    // Methods
    public void Start()
    {
        currTime = length;
    }

    public void End()
    {
        currTime = 0;
    }
    
    public bool Update(float dt)
    {
        // Update timer while active
        if (currTime > 0)
            currTime -= dt;

        // Return indicates if timer has ended
        return currTime <= 0;
    }

    public void SetLength(float length)
    {
        this.length = length;
    }
}
