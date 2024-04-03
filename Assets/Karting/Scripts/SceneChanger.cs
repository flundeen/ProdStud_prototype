using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void ChangeScene(string sceneName){
        SceneManager.LoadScene(sceneName);
        Debug.Log("Changing scenes to " + sceneName);
    }

    public void UnloadScene(string sceneName){
        SceneManager.UnloadSceneAsync(sceneName);
    }

    public void Exit()
    {
        Application.Quit();
    }
    
}
