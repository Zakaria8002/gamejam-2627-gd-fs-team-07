using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if ((sceneName == "P1Win" || sceneName == "P2Win") && Input.anyKeyDown)
        {
            SceneManager.LoadScene("Game");
        }
    }
}
