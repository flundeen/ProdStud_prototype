using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveDropoff : MonoBehaviour
{
    [SerializeField]
    private float scoreTimer = 1.0f;
    [SerializeField]
    public Material dropoffMaterial;
    [SerializeField]
    private GameObject plane;
    private Material planeMat;
    private Color scoredColor = Color.green;
    private Color inactiveColor = Color.red;

    private bool scored;

    private AudioSource audioSrc;
    public AudioClip dropoffSFX;

    void Awake()
    {
        scored = false;
        audioSrc = GetComponent<AudioSource>();
        planeMat = plane.GetComponent<Renderer>().material;
    }

    void Start()
    {
        GameManager.Instance.objDropoffZones.Add(this);
    }

    // Update is called once per frame
    void Update()
    { 
        if (scored)
        {
            planeMat.color = scoredColor;
            dropoffMaterial.color = Color.green;
            scoreTimer -= Time.deltaTime;
            if (scoreTimer < 0)
            {
                scored = false;
            }
        }
        else
        {
            planeMat.color = inactiveColor;
            dropoffMaterial.color = Color.gray;
            scoreTimer = 1.0f;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Player" && collision.GetComponentInParent<KartPackage>().hasPackage)
        {
            collision.GetComponentInParent<KartPackage>().hasPackage = false;
            GameManager.Instance.packageIsPresent = false;
            GameManager.Instance.packagePickedUp = false;
            scored = true;

            GameManager.Instance.AwardPoints(collision.GetComponentInParent<Player>().id, ScoreEvent.Delivery);

            if (audioSrc != null && dropoffSFX != null)
                audioSrc.PlayOneShot(dropoffSFX);
        }
    }
}
