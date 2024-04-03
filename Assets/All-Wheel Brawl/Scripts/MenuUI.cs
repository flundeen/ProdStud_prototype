using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnStartBtnClick()
    {
        Debug.Log("Starting Game!");
        SceneManager.LoadSceneAsync(1); // Assumes Title = 0, Select Screen = 1, Game = 2
    }
}
