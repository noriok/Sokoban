using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleManager : MonoBehaviour {

    void Start() {
        var button = GameObject.Find("Button");
        button.GetComponent<Button>().onClick.AddListener(() => {
            SceneManager.LoadScene("Game");

        });
    }

    void Update() {
    }
}
