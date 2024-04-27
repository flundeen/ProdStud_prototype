using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using KartGame.KartSystems;

public class PlayerUI : MonoBehaviour
{
    // Component References
    public ArcadeKart car;
    public Image healthBar;
    public TMP_Text scoreLabel;
    public TMP_Text pkgTimerLabel;

    // Fields
    private float maxHealthWidth;

    // Start is called before the first frame update
    void Start()
    {
        maxHealthWidth = healthBar.rectTransform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        // Resize health bar to proportion of max health remaining
        healthBar.rectTransform.localScale = new Vector3(
            maxHealthWidth * car.baseStats.Health / car.baseStats.MaxHealth,
            healthBar.rectTransform.localScale.y, 
            healthBar.rectTransform.localScale.z);

        // Update score label to player's current score
        scoreLabel.text = GameManager.Instance.scores[car.playerId].ToString();

        // Update package timer while carrying package
        if (car.HasPackage)
        {
            pkgTimerLabel.gameObject.SetActive(true);
            pkgTimerLabel.text = Mathf.Ceil(car.PackageCountdown).ToString();
        }
        else
            pkgTimerLabel.gameObject.SetActive(false);
    }
}
