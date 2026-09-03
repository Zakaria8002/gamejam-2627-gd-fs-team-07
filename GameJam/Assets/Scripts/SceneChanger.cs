using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] GameObject RulesPanel;
    // Update is called once per frame
    void Update()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if ((sceneName == "P1Win" || sceneName == "P2Win") && Input.anyKeyDown)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Rules()
    {
        RulesPanel.SetActive(true);
    }

    public void CloseRules()
    {
        RulesPanel.SetActive(false);
    }
}
