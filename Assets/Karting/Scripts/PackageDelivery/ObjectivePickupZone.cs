using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectivePickupZone : MonoBehaviour
{

    [SerializeField]
    private float pickupTimer = 5f;
    [SerializeField]
    private Material packageMaterial;
    [SerializeField]
    private GameObject package;
    [SerializeField]
    private GameObject plane;
    private Material planeMat;
    private Color inactiveColor = Color.black;
    private Color waitingColor = Color.yellow;
    private Color loadingColor = Color.cyan;
    private Color stoppedColor = Color.red;

    private bool hasPackage;
    private List<GameObject> players;
    private List<float> timers;

    private AudioSource audioSrc;
    public AudioClip loadingSFX;
    public AudioClip pickupSFX;
    private float pickupSFXdelay = 1.5f; // Keeps pickupSFX from being cut off

    public bool HasPackage { get; set; }


    void Awake()
    {
        players = new List<GameObject>();
        timers = new List<float>();
        hasPackage = false;
        audioSrc = GetComponent<AudioSource>();
        planeMat = plane.GetComponent<Renderer>().material;

        if (audioSrc != null && loadingSFX != null) 
            audioSrc.clip = loadingSFX;
    }

    void Start()
    {
        GameManager.Instance.objPickupZones.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (pickupSFXdelay > 0) pickupSFXdelay -= Time.deltaTime;

        if (hasPackage && players.Count == 1 && timers[0] > 0)
        {
            // Play loading SFX while loading progress is made
            if (audioSrc != null && loadingSFX != null)
                if (!audioSrc.isPlaying) audioSrc.Play();

            planeMat.color = loadingColor;
            packageMaterial.color = Color.green;
            timers[0] -= Time.deltaTime;
            if (timers[0] <= 0)
            {
                players[0].GetComponentInParent<KartPackage>().hasPackage = true;
                GameManager.Instance.packagePickedUp = true;
                GameManager.Instance.packageHolder = players[0].transform;
                planeMat.color = inactiveColor;
                package.GetComponent<Renderer>().enabled = false;
                hasPackage = false;
                Debug.Log("Package has been picked up");

                if (audioSrc != null)
                {
                    audioSrc.Stop();
                    if (pickupSFX != null)
                        audioSrc.PlayOneShot(pickupSFX);
                    pickupSFXdelay = 1.5f;
                }
            }
        }
        else if (players.Count > 1)
        {
            if (hasPackage)
                planeMat.color = stoppedColor;
            packageMaterial.color = Color.yellow;

            if (audioSrc != null && loadingSFX != null && pickupSFXdelay <= 0)
                audioSrc.Pause();
        }
        else
        {
            if (hasPackage)
                planeMat.color = waitingColor;
            else
                planeMat.color = inactiveColor;
            packageMaterial.color = Color.gray;
            
            if (audioSrc != null && loadingSFX != null && pickupSFXdelay <= 0)
                audioSrc.Stop();
        }
        if (!hasPackage)
        {
            package.GetComponent<Renderer>().enabled = false;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Player")
        {
            players.Add(collision.gameObject);
            timers.Add(pickupTimer);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.tag == "Player")
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].GetComponent<Collider>() == collision)
                {
                    players.RemoveAt(i);
                    timers.RemoveAt(i);
                }
            }
        }
    }

    public void AddPackage()
    {
        if (!hasPackage)
        {
            hasPackage = true;
            GameManager.Instance.packageIsPresent = true;
            package.GetComponent<Renderer>().enabled = true;
            planeMat.color = waitingColor;
        }
    }
}
