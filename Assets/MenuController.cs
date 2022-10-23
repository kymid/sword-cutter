using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;
    public GameObject LoseMenu;

    private void Start()
    {
        instance = this;
    }
    public void Activate(GameObject go)
    {
        go.SetActive(!go.activeInHierarchy);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }
}
