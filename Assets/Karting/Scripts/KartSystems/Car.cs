﻿using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.VFX;
using UnityEngine.InputSystem;

namespace KartGame.KartSystems
{
    public enum CarType
    {
        Pizza,
        Mail
    }

    public class ArcadeKart : MonoBehaviour
    {
        [System.Serializable]
        public class StatPowerup
        {
            public ArcadeKart.Stats modifiers;
            public string PowerUpID;
            public float ElapsedTime;
            public float MaxTime;
            public Action callback;

            public StatPowerup() { }

            public StatPowerup(ArcadeKart.Stats modifiers, string id, float maxTime)
            {
                this.modifiers = modifiers;
                this.PowerUpID = id;
                this.MaxTime = maxTime;
            }

            public void Reset()
            {
                ElapsedTime = 0;
            }
        }

        [System.Serializable]
        public struct Stats
        {
            [Header("Vehicle Stats")]
            [Tooltip("How much health the player has.")]
            public float MaxHealth;
            [NonSerialized]
            public float Health;

            [Tooltip("How quickly the vehicle reaches top speed.")]
            public float Acceleration;

            [Min(0.001f), Tooltip("Top speed attainable when moving forward.")]
            public float TopSpeed;

            [Tooltip("How tightly the vehicle can turn left or right.")]
            public float Steer;

            [Tooltip("How powerful the vehicle is in collisions.")]
            public float Weight;

            public SubStats subStats;

            [System.Serializable]
            public struct SubStats
            {
                [Min(0.001f), Tooltip("Top speed attainable when moving backward.")]
                public float ReverseSpeed;

                [Tooltip("How quickly the kart reaches top speed, when moving backward.")]
                public float ReverseAcceleration;

                [Tooltip("How quickly the kart starts accelerating from 0. A higher number means it accelerates faster sooner.")]
                [Range(0.2f, 1)]
                public float AccelerationCurve;

                [Tooltip("How quickly the kart slows down when the brake is applied.")]
                public float Braking;

                [Tooltip("How quickly the kart will reach a full stop when no inputs are made.")]
                public float CoastingDrag;

                [Range(0.0f, 1.0f)]
                [Tooltip("The amount of side-to-side friction.")]
                public float Grip;

                [Tooltip("Additional gravity for when the kart is in the air.")]
                public float AddedGravity;

                public static SubStats operator +(SubStats a, SubStats b)
                {
                    return new SubStats
                    {
                        ReverseSpeed = a.ReverseSpeed + b.ReverseSpeed,
                        ReverseAcceleration = a.ReverseAcceleration + b.ReverseAcceleration,
                        AccelerationCurve = a.AccelerationCurve + b.AccelerationCurve,
                        Braking = a.Braking + b.Braking,
                        CoastingDrag = a.CoastingDrag + b.CoastingDrag,
                        Grip = a.Grip + b.Grip,
                        AddedGravity = a.AddedGravity + b.AddedGravity
                    };
                }
            }

            // allow for stat adding for powerups.
            public static Stats operator +(Stats a, Stats b)
            {
                return new Stats
                {
                    MaxHealth = a.MaxHealth + b.MaxHealth,
                    Health = a.Health + b.Health,
                    Acceleration = a.Acceleration + b.Acceleration,
                    TopSpeed = a.TopSpeed + b.TopSpeed,
                    Steer = a.Steer + b.Steer,
                    Weight = a.Weight + b.Weight,
                    subStats = a.subStats + b.subStats
                };
            }
        }

        public Camera camera;
        public GameObject kartVisual;
        public Rigidbody Rigidbody { get; private set; }
        public InputData Input { get; private set; }
        public float AirPercent { get; private set; }
        public float GroundPercent { get; private set; }

        public ArcadeKart.Stats baseStats = new ArcadeKart.Stats
        {
            MaxHealth = 100f,
            Health = 100,
            TopSpeed = 10f,
            Acceleration = 5f,
            Steer = 5f,
            Weight = 5f,
            subStats = new Stats.SubStats
            {
                AccelerationCurve = 4f,
                Braking = 10f,
                ReverseAcceleration = 5f,
                ReverseSpeed = 5f,
                CoastingDrag = 4f,
                Grip = .95f,
                AddedGravity = 1f
            }
        };

        [Header("Vehicle Visual")]
        public List<GameObject> m_VisualWheels;

        [Header("Vehicle Physics")]
        [Tooltip("The transform that determines the position of the kart's mass.")]
        public Transform CenterOfMass;

        [Range(0.0f, 20.0f), Tooltip("Coefficient used to reorient the kart in the air. The higher the number, the faster the kart will readjust itself along the horizontal plane.")]
        public float AirborneReorientationCoefficient = 3.0f;

        [Header("Drifting")]
        [Range(0.01f, 1.0f), Tooltip("The grip value when drifting.")]
        public float DriftGrip = 0.4f;
        [Range(0.0f, 10.0f), Tooltip("Additional steer when the kart is drifting.")]
        public float DriftAdditionalSteer = 5.0f;
        [Range(1.0f, 30.0f), Tooltip("The higher the angle, the easier it is to regain full grip.")]
        public float MinAngleToFinishDrift = 10.0f;
        [Range(0.01f, 0.99f), Tooltip("Mininum speed percentage to switch back to full grip.")]
        public float MinSpeedPercentToFinishDrift = 0.5f;
        [Range(1.0f, 20.0f), Tooltip("The higher the value, the easier it is to control the drift steering.")]
        public float DriftControl = 10.0f;
        [Range(0.0f, 20.0f), Tooltip("The lower the value, the longer the drift will last without trying to control it by steering.")]
        public float DriftDampening = 10.0f;

        [Header("VFX")]
        [Tooltip("VFX that will be placed on the wheels when drifting.")]
        public ParticleSystem DriftSparkVFX;
        [Range(0.0f, 0.2f), Tooltip("Offset to displace the VFX to the side.")]
        public float DriftSparkHorizontalOffset = 0.1f;
        [Range(0.0f, 90.0f), Tooltip("Angle to rotate the VFX.")]
        public float DriftSparkRotation = 17.0f;
        [Tooltip("VFX that will be placed on the wheels when drifting.")]
        public GameObject DriftTrailPrefab;
        [Range(-0.1f, 0.1f), Tooltip("Vertical to move the trails up or down and ensure they are above the ground.")]
        public float DriftTrailVerticalOffset;
        [Tooltip("VFX that will spawn upon landing, after a jump.")]
        public GameObject JumpVFX;
        [Tooltip("VFX that is spawn on the nozzles of the kart.")]
        public GameObject NozzleVFX;
        [Tooltip("List of the kart's nozzles.")]
        public List<Transform> Nozzles;

        [Header("Suspensions")]
        [Tooltip("The maximum extension possible between the kart's body and the wheels.")]
        [Range(0.0f, 1.0f)]
        public float SuspensionHeight = 0.2f;
        [Range(10.0f, 100000.0f), Tooltip("The higher the value, the stiffer the suspension will be.")]
        public float SuspensionSpring = 20000.0f;
        [Range(0.0f, 5000.0f), Tooltip("The higher the value, the faster the kart will stabilize itself.")]
        public float SuspensionDamp = 500.0f;
        [Tooltip("Vertical offset to adjust the position of the wheels relative to the kart's body.")]
        [Range(-1.0f, 1.0f)]
        public float WheelsPositionVerticalOffset = 0.0f;

        [Header("Physical Wheels")]
        [Tooltip("The physical representations of the Kart's wheels.")]
        public WheelCollider FrontLeftWheel;
        public WheelCollider FrontRightWheel;
        public WheelCollider RearLeftWheel;
        public WheelCollider RearRightWheel;

        [Tooltip("Which layers the wheels will detect.")]
        public LayerMask GroundLayers = Physics.DefaultRaycastLayers;

        KartPackage kartPkg;

        const float k_NullInput = 0.01f;
        const float k_NullSpeed = 0.01f;
        Vector3 m_VerticalReference = Vector3.up;

        // Drift params
        public bool WantsToDrift { get; private set; } = false;
        public bool IsDrifting { get; private set; } = false;
        float m_CurrentGrip = 1.0f;
        float m_DriftTurningPower = 0.0f;
        float m_PreviousGroundPercent = 1.0f;
        readonly List<(GameObject trailRoot, WheelCollider wheel, TrailRenderer trail)> m_DriftTrailInstances = new List<(GameObject, WheelCollider, TrailRenderer)>();
        readonly List<(WheelCollider wheel, float horizontalOffset, float rotation, ParticleSystem sparks)> m_DriftSparkInstances = new List<(WheelCollider, float, float, ParticleSystem)>();

        // can the kart move?
        bool m_CanMove = true;
        List<StatPowerup> m_ActivePowerupList = new List<StatPowerup>();
        ArcadeKart.Stats m_FinalStats;

        Quaternion m_LastValidRotation;
        Vector3 m_LastValidPosition;
        Vector3 m_LastCollisionNormal;
        bool m_HasCollision;
        bool m_InAir = false;

        // Input Fields
        private float accelVal = 0;
        private float brakeVal = 0;
        private float turnVal = 0;
        private float cameraSwivel = 0;

        public int playerId = -1;

        [SerializeField]
        private GameObject arrowPrefab;
        private ArrowRotate arrow;

        //Package variables
        [SerializeField]
        private GameObject package;
        [SerializeField]
        private GameObject droppedPackage;
        private float packageTimer = 20f;
        public bool HasPackage { get { return kartPkg.hasPackage; } }
        public float PackageCountdown { get { return packageTimer; } }

        // Audio Fields
        private AudioSource audioSrc;
        public AudioClip damagedSFX;
        public AudioClip deathSFX;
        public AudioClip pkgTimerSFX;
        public AudioClip honkSFX;

        // Event fields
        public Action<int> deathCallback;

        public void AddPowerup(StatPowerup statPowerup) => m_ActivePowerupList.Add(statPowerup);
        public void SetCanMove(bool move) => m_CanMove = move;
        public float GetMaxSpeed() => Mathf.Max(m_FinalStats.TopSpeed, m_FinalStats.subStats.ReverseSpeed);

        private void ActivateDriftVFX(bool active)
        {
            foreach (var vfx in m_DriftSparkInstances)
            {
                if (active && vfx.wheel.GetGroundHit(out WheelHit hit))
                {
                    if (!vfx.sparks.isPlaying)
                        vfx.sparks.Play();
                }
                else
                {
                    if (vfx.sparks.isPlaying)
                        vfx.sparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
                    
            }

            foreach (var trail in m_DriftTrailInstances)
                trail.Item3.emitting = active && trail.wheel.GetGroundHit(out WheelHit hit);
        }

        private void UpdateDriftVFXOrientation()
        {
            foreach (var vfx in m_DriftSparkInstances)
            {
                vfx.sparks.transform.position = vfx.wheel.transform.position - (vfx.wheel.radius * Vector3.up) + (DriftTrailVerticalOffset * Vector3.up) + (transform.right * vfx.horizontalOffset);
                vfx.sparks.transform.rotation = transform.rotation * Quaternion.Euler(0.0f, 0.0f, vfx.rotation);
            }

            foreach (var trail in m_DriftTrailInstances)
            {
                trail.trailRoot.transform.position = trail.wheel.transform.position - (trail.wheel.radius * Vector3.up) + (DriftTrailVerticalOffset * Vector3.up);
                trail.trailRoot.transform.rotation = transform.rotation;
            }
        }

        void UpdateSuspensionParams(WheelCollider wheel)
        {
            wheel.suspensionDistance = SuspensionHeight;
            wheel.center = new Vector3(0.0f, WheelsPositionVerticalOffset, 0.0f);
            JointSpring spring = wheel.suspensionSpring;
            spring.spring = SuspensionSpring;
            spring.damper = SuspensionDamp;
            wheel.suspensionSpring = spring;
        }

        void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();

            // Use stats from inspector window to set remaining stats
            baseStats.Health = baseStats.MaxHealth;
            baseStats.subStats = new Stats.SubStats
            {
                AccelerationCurve = 4f,
                Braking = 10f,
                ReverseAcceleration = baseStats.Acceleration,
                ReverseSpeed = baseStats.TopSpeed,
                CoastingDrag = 4f,
                Grip = .95f,
                AddedGravity = 1f
            };

            UpdateSuspensionParams(FrontLeftWheel);
            UpdateSuspensionParams(FrontRightWheel);
            UpdateSuspensionParams(RearLeftWheel);
            UpdateSuspensionParams(RearRightWheel);

            m_CurrentGrip = baseStats.subStats.Grip;

            if (DriftSparkVFX != null)
            {
                AddSparkToWheel(RearLeftWheel, -DriftSparkHorizontalOffset, -DriftSparkRotation);
                AddSparkToWheel(RearRightWheel, DriftSparkHorizontalOffset, DriftSparkRotation);
            }

            if (DriftTrailPrefab != null)
            {
                AddTrailToWheel(RearLeftWheel);
                AddTrailToWheel(RearRightWheel);
            }

            if (NozzleVFX != null)
            {
                foreach (var nozzle in Nozzles)
                {
                    Instantiate(NozzleVFX, nozzle, false);
                }
            }

            kartPkg = GetComponent<KartPackage>();
            audioSrc = GetComponent<AudioSource>();

            arrow = Instantiate(arrowPrefab).GetComponent<ArrowRotate>();
            arrow.attachedKart = transform;
            arrow.kartPackage = kartPkg;
        }

        void AddTrailToWheel(WheelCollider wheel)
        {
            GameObject trailRoot = Instantiate(DriftTrailPrefab, gameObject.transform, false);
            TrailRenderer trail = trailRoot.GetComponentInChildren<TrailRenderer>();
            trail.emitting = false;
            m_DriftTrailInstances.Add((trailRoot, wheel, trail));
        }

        void AddSparkToWheel(WheelCollider wheel, float horizontalOffset, float rotation)
        {
            GameObject vfx = Instantiate(DriftSparkVFX.gameObject, wheel.transform, false);
            ParticleSystem spark = vfx.GetComponent<ParticleSystem>();
            spark.Stop();
            m_DriftSparkInstances.Add((wheel, horizontalOffset, -rotation, spark));
        }

        public void AssignOwner(Player p)
        {
            playerId = p.ID;
        }

        void FixedUpdate()
        {
            // Skip processing if not alive
            if (baseStats.Health <= 0) return;

            UpdateSuspensionParams(FrontLeftWheel);
            UpdateSuspensionParams(FrontRightWheel);
            UpdateSuspensionParams(RearLeftWheel);
            UpdateSuspensionParams(RearRightWheel);

            // apply our powerups to create our finalStats
            TickPowerups();

            // apply drift if speed is boosted
            ActivateDriftVFX(m_ActivePowerupList.Find(s => s.PowerUpID == "On-Time Speed Boost") != null);

            // apply our physics properties
            Rigidbody.centerOfMass = transform.InverseTransformPoint(CenterOfMass.position);

            int groundedCount = 0;
            if (FrontLeftWheel.isGrounded && FrontLeftWheel.GetGroundHit(out WheelHit hit))
                groundedCount++;
            if (FrontRightWheel.isGrounded && FrontRightWheel.GetGroundHit(out hit))
                groundedCount++;
            if (RearLeftWheel.isGrounded && RearLeftWheel.GetGroundHit(out hit))
                groundedCount++;
            if (RearRightWheel.isGrounded && RearRightWheel.GetGroundHit(out hit))
                groundedCount++;

            // calculate how grounded and airborne we are
            GroundPercent = (float) groundedCount / 4.0f;
            AirPercent = 1 - GroundPercent;

            // apply vehicle physics
            if (m_CanMove) MoveVehicle();
            GroundAirbourne();

            m_PreviousGroundPercent = GroundPercent;

            UpdateDriftVFXOrientation();

            // Update package logic
            if (kartPkg.hasPackage)
            {
                if (baseStats.Health > 0)
                {
                    package.GetComponent<Renderer>().enabled = true;
                }
                float lastTime = packageTimer;
                packageTimer -= Time.fixedDeltaTime;

                // Has package timer expired?
                if (packageTimer <= 0)
                {
                    Debug.Log("Package exploded!");
                    Die(-1); // -1 for non-player attack
                }
                else
                {
                    // Play timer tick SFX once per second
                    if (Mathf.Ceil(lastTime) > Mathf.Ceil(packageTimer))
                    {
                        if (audioSrc != null && pkgTimerSFX != null)
                            audioSrc.PlayOneShot(pkgTimerSFX);
                    }
                }
            }
            else
            {
                package.GetComponent<Renderer>().enabled = false;
                packageTimer = 20f;
            }
        }

        public void SetTransform(Transform t)
        {
            transform.SetPositionAndRotation(t.position, t.rotation);
            Rigidbody.position = t.position;
            Rigidbody.rotation = t.rotation;
        }

        void OnHonk()
        {
            if (audioSrc != null && honkSFX != null)
                audioSrc.PlayOneShot(honkSFX);
        }

        public bool TakeDamage(AttackInfo info)
        {
            // If own attack or already dead, does not count
            if (baseStats.Health <= 0 || info.attackerId == playerId)
                return false;

            if(baseStats.Health - info.damage <= 0)
                Die(info.attackerId);
            else
            {
                baseStats.Health -= info.damage;
                Debug.Log("Player " + playerId + " has taken " + info.damage + " points of damage. " + baseStats.Health + " remaining.");
                if (audioSrc != null && damagedSFX != null)
                    audioSrc.PlayOneShot(damagedSFX);
            }

            // Attack successful
            return true;
        }

        void Die(int attackerId)
        {
            Debug.Log("Player " + playerId + " died!");

            baseStats.Health = 0;
            Rigidbody.velocity = Vector3.zero;

            // Send death notification if this car belongs to player
            deathCallback?.Invoke(attackerId);

            // Drop package if carrying one
            if (kartPkg.hasPackage)
            {
                kartPkg.hasPackage = false;
                GameObject dropped = Instantiate(droppedPackage, transform.position + Vector3.up, transform.rotation);
                GameManager.Instance.packageHolder = dropped.transform;
                GameManager.Instance.packagePickedUp = false;
                package.GetComponent<Renderer>().enabled = false;
            }

            // Death SFX
            if (audioSrc != null && deathSFX != null)
                audioSrc.PlayOneShot(deathSFX);

            // Turn off visibility
            // FixedUpdate processing skipped when dead
            kartVisual.SetActive(false);

            // If non-player car, completely disable (assuming no respawn)
            if (playerId == -1) gameObject.SetActive(false);
        }

        public void ResetCar()
        {
            baseStats.Health = baseStats.MaxHealth;
            kartVisual.SetActive(true);
            kartPkg.hasPackage = false;
            packageTimer = 20f;
        }


        void TickPowerups()
        {
            // remove all elapsed powerups
            m_ActivePowerupList.RemoveAll((p) => 
            {
                if(p.ElapsedTime > p.MaxTime)
                {
                    Debug.Log("Removing " + p.PowerUpID);
                }
                return p.ElapsedTime > p.MaxTime; 
            });

            // zero out powerups before we add them all up
            var powerups = new Stats();

            // add up all our powerups
            for (int i = 0; i < m_ActivePowerupList.Count; i++)
            {
                var p = m_ActivePowerupList[i];

                // add elapsed time
                p.ElapsedTime += Time.fixedDeltaTime;

                // add up the powerups
                powerups += p.modifiers;
            }

            // add powerups to our final stats
            m_FinalStats = baseStats + powerups;

            // clamp values in finalstats
            m_FinalStats.subStats.Grip = Mathf.Clamp(m_FinalStats.subStats.Grip, 0, 1);
        }

        void GroundAirbourne()
        {
            // while in the air, fall faster
            if (AirPercent >= 1)
            {
                Rigidbody.velocity += Physics.gravity * Time.fixedDeltaTime * m_FinalStats.subStats.AddedGravity;
            }
        }

        public float LocalSpeed()
        {
            if (m_CanMove)
            {
                float dot = Vector3.Dot(transform.forward, Rigidbody.velocity);
                if (Mathf.Abs(dot) > 0.1f)
                {
                    float speed = Rigidbody.velocity.magnitude;
                    return dot < 0 ? -(speed / m_FinalStats.subStats.ReverseSpeed) : (speed / m_FinalStats.TopSpeed);
                }
                return 0f;
            }
            else
            {
                // use this value to play kart sound when it is waiting the race start countdown.
                return Input.Acceleration > 0 ? 1.0f : 0.0f;
            }
        }

        void OnCollisionEnter(Collision collision) => m_HasCollision = true;
        void OnCollisionExit(Collision collision) => m_HasCollision = false;

        void OnCollisionStay(Collision collision)
        {
            m_HasCollision = true;
            m_LastCollisionNormal = Vector3.zero;
            float dot = -1.0f;

            foreach (var contact in collision.contacts)
            {
                if (Vector3.Dot(contact.normal, Vector3.up) > dot)
                    m_LastCollisionNormal = contact.normal;
            }
        }

        void MoveVehicle()
        {
            float accelInput = accelVal - brakeVal;

            // manual acceleration curve coefficient scalar
            float accelerationCurveCoeff = 5;
            Vector3 localVel = transform.InverseTransformVector(Rigidbody.velocity);

            bool accelDirectionIsFwd = accelInput >= 0;
            bool localVelDirectionIsFwd = localVel.z >= 0;

            // use the max speed for the direction we are going--forward or reverse.
            float maxSpeed = localVelDirectionIsFwd ? m_FinalStats.TopSpeed : m_FinalStats.subStats.ReverseSpeed;
            float accelPower = accelDirectionIsFwd ? m_FinalStats.Acceleration : m_FinalStats.subStats.ReverseAcceleration;

            float currentSpeed = Rigidbody.velocity.magnitude;
            float accelRampT = currentSpeed / maxSpeed;
            float multipliedAccelerationCurve = m_FinalStats.subStats.AccelerationCurve * accelerationCurveCoeff;
            float accelRamp = Mathf.Lerp(multipliedAccelerationCurve, 1, accelRampT * accelRampT);

            bool isBraking = (localVelDirectionIsFwd && brakeVal > 0) || (!localVelDirectionIsFwd && accelVal > 0);

            // if we are braking (moving reverse to where we are going)
            // use the braking accleration instead
            float finalAccelPower = isBraking ? m_FinalStats.subStats.Braking : accelPower;

            float finalAcceleration = finalAccelPower * accelRamp;

            // apply inputs to forward/backward
            float turningPower = IsDrifting ? m_DriftTurningPower : turnVal * m_FinalStats.Steer;

            Quaternion turnAngle = Quaternion.AngleAxis(turningPower, transform.up);
            Vector3 fwd = turnAngle * transform.forward;
            Vector3 movement = fwd * accelInput * finalAcceleration * ((m_HasCollision || GroundPercent > 0.0f) ? 1.0f : 0.0f);

            // forward movement
            bool wasOverMaxSpeed = currentSpeed >= maxSpeed;

            // if over max speed, cannot accelerate faster.
            if (wasOverMaxSpeed && !isBraking) 
                movement *= 0.0f;

            Vector3 newVelocity = Rigidbody.velocity + movement * Time.fixedDeltaTime;
            newVelocity.y = Rigidbody.velocity.y;

            //  clamp max speed if we are on ground
            if (GroundPercent > 0.0f && !wasOverMaxSpeed)
            {
                newVelocity = Vector3.ClampMagnitude(newVelocity, maxSpeed);
            }

            // coasting is when we aren't touching accelerate
            if (Mathf.Abs(accelInput) < k_NullInput && GroundPercent > 0.0f)
            {
                newVelocity = Vector3.MoveTowards(newVelocity, new Vector3(0, Rigidbody.velocity.y, 0), Time.fixedDeltaTime * m_FinalStats.subStats.CoastingDrag);
            }

            Rigidbody.velocity = newVelocity;

            // Drift
            WantsToDrift = brakeVal > 0 && Vector3.Dot(Rigidbody.velocity, transform.forward) > 0.0f;
            if (GroundPercent > 0.0f)
            {
                if (m_InAir)
                {
                    m_InAir = false;
                    Instantiate(JumpVFX, transform.position, Quaternion.identity);
                }

                // manual angular velocity coefficient
                float angularVelocitySteering = 0.4f;
                float angularVelocitySmoothSpeed = 20f;

                // turning is reversed if we're going in reverse and pressing reverse
                if (!localVelDirectionIsFwd && !accelDirectionIsFwd) 
                    angularVelocitySteering *= -1.0f;

                var angularVel = Rigidbody.angularVelocity;

                // move the Y angular velocity towards our target
                angularVel.y = Mathf.MoveTowards(angularVel.y, turningPower * angularVelocitySteering, Time.fixedDeltaTime * angularVelocitySmoothSpeed);

                // apply the angular velocity
                Rigidbody.angularVelocity = angularVel;

                // rotate rigidbody's velocity as well to generate immediate velocity redirection
                // manual velocity steering coefficient
                float velocitySteering = 25f;

                // If the karts lands with a forward not in the velocity direction, we start the drift
                if (GroundPercent >= 0.0f && m_PreviousGroundPercent < 0.1f)
                {
                    Vector3 flattenVelocity = Vector3.ProjectOnPlane(Rigidbody.velocity, m_VerticalReference).normalized;
                    if (Vector3.Dot(flattenVelocity, transform.forward * Mathf.Sign(accelInput)) < Mathf.Cos(MinAngleToFinishDrift * Mathf.Deg2Rad))
                    {
                        IsDrifting = true;
                        m_CurrentGrip = DriftGrip;
                        m_DriftTurningPower = 0.0f;
                    }
                }

                // Drift Management
                if (!IsDrifting)
                {
                    if ((WantsToDrift || isBraking) && currentSpeed > maxSpeed * MinSpeedPercentToFinishDrift)
                    {
                        IsDrifting = true;
                        m_DriftTurningPower = turningPower + (Mathf.Sign(turningPower) * DriftAdditionalSteer);
                        m_CurrentGrip = DriftGrip;

                        ActivateDriftVFX(true);
                    }
                }

                if (IsDrifting)
                {
                    float turnInputAbs = Mathf.Abs(turnVal);
                    if (turnInputAbs < k_NullInput)
                        m_DriftTurningPower = Mathf.MoveTowards(m_DriftTurningPower, 0.0f, Mathf.Clamp01(DriftDampening * Time.fixedDeltaTime));

                    // Update the turning power based on input
                    float driftMaxSteerValue = m_FinalStats.Steer + DriftAdditionalSteer;
                    m_DriftTurningPower = Mathf.Clamp(m_DriftTurningPower + (turnVal * Mathf.Clamp01(DriftControl * Time.fixedDeltaTime)), -driftMaxSteerValue, driftMaxSteerValue);

                    bool facingVelocity = Vector3.Dot(Rigidbody.velocity.normalized, transform.forward * Mathf.Sign(accelInput)) > Mathf.Cos(MinAngleToFinishDrift * Mathf.Deg2Rad);

                    bool canEndDrift = true;
                    if (isBraking)
                        canEndDrift = false;
                    else if (!facingVelocity)
                        canEndDrift = false;
                    else if (turnInputAbs >= k_NullInput && currentSpeed > maxSpeed * MinSpeedPercentToFinishDrift)
                        canEndDrift = false;

                    if (canEndDrift || currentSpeed < k_NullSpeed)
                    {
                        // No Input, and car aligned with speed direction => Stop the drift
                        IsDrifting = false;
                        m_CurrentGrip = m_FinalStats.subStats.Grip;
                    }

                }

                // rotate our velocity based on current steer value
                Rigidbody.velocity = Quaternion.AngleAxis(turningPower * Mathf.Sign(localVel.z) * velocitySteering * m_CurrentGrip * Time.fixedDeltaTime, transform.up) * Rigidbody.velocity;
            }
            else
            {
                m_InAir = true;
            }

            bool validPosition = false;
            if (Physics.Raycast(transform.position + (transform.up * 0.1f), -transform.up, out RaycastHit hit, 3.0f, 1 << 9 | 1 << 10 | 1 << 11)) // Layer: ground (9) / Environment(10) / Track (11)
            {
                Vector3 lerpVector = (m_HasCollision && m_LastCollisionNormal.y > hit.normal.y) ? m_LastCollisionNormal : hit.normal;
                m_VerticalReference = Vector3.Slerp(m_VerticalReference, lerpVector, Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime * (GroundPercent > 0.0f ? 10.0f : 1.0f)));    // Blend faster if on ground
            }
            else
            {
                Vector3 lerpVector = (m_HasCollision && m_LastCollisionNormal.y > 0.0f) ? m_LastCollisionNormal : Vector3.up;
                m_VerticalReference = Vector3.Slerp(m_VerticalReference, lerpVector, Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime));
            }

            validPosition = GroundPercent > 0.7f && !m_HasCollision && Vector3.Dot(m_VerticalReference, Vector3.up) > 0.9f;

            // Airborne / Half on ground management
            if (GroundPercent < 0.7f)
            {
                Rigidbody.angularVelocity = new Vector3(0.0f, Rigidbody.angularVelocity.y * 0.98f, 0.0f);
                Vector3 finalOrientationDirection = Vector3.ProjectOnPlane(transform.forward, m_VerticalReference);
                finalOrientationDirection.Normalize();
                if (finalOrientationDirection.sqrMagnitude > 0.0f)
                {
                    Rigidbody.MoveRotation(Quaternion.Lerp(Rigidbody.rotation, Quaternion.LookRotation(finalOrientationDirection, m_VerticalReference), Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime)));
                }
            }
            else if (validPosition)
            {
                m_LastValidPosition = transform.position;
                m_LastValidRotation.eulerAngles = new Vector3(0.0f, transform.rotation.y, 0.0f);
            }

            ActivateDriftVFX(IsDrifting && GroundPercent > 0.0f);
        }

        private void OnDestroy()
        {
            if (arrow != null)
                Destroy(arrow.gameObject);
        }

        void OnAccelerate(InputValue val)
        {
            accelVal = val.Get<float>();
        }
        void OnBrake(InputValue val)
        {
            brakeVal = val.Get<float>();
        }
        void OnTurn(InputValue val)
        {
            turnVal = val.Get<float>();
        }
    }
}
