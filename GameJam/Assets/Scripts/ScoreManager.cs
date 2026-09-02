using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreTextP1;
    [SerializeField] private TMP_Text scoreTextP2;
    [SerializeField] private TMP_Text timer;
    [SerializeField] private GameObject GameOver;
    public int scoreP1 = 0;
    public int scoreP2 = 0;
    [SerializeField] private float startTime = 60f;
    private float timeRemaining;
    private bool timerEnded = false;
    // Expose timer state for other systems
    public float TimeRemaining => timeRemaining;
    public bool IsTimerEnded => timerEnded;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeRemaining = startTime;
        timer.text = Mathf.CeilToInt(timeRemaining).ToString();
        scoreP1 = 0; scoreP2 = 0;
        GameOver.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            scoreP1 += 100;
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            scoreP1 -= 100;
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            scoreP2 += 100;
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            scoreP2 -= 100;
        }

        scoreTextP1.text = "Player 1: " + scoreP1.ToString();
        scoreTextP2.text = "Player 2: " + scoreP2.ToString();

        // Countdown timer
        if (!timerEnded)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                timerEnded = true;
                Debug.Log("Timer reached zero");
                GameOver.SetActive(true);
                // wait a short time before showing final result / switching scenes
                StartCoroutine(GameOverSequence());
            }

            timer.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
    }

    public void ChangeScore(int playerNumber, int amount)
    {
        if (playerNumber == 1)
        {
            scoreP1 += amount;
        }
        else if (playerNumber == 2)
        {
            scoreP2 += amount;
        }
    }

    private System.Collections.IEnumerator GameOverSequence()
    {
        // give players a moment to see GameOver UI
        yield return new WaitForSeconds(3f);

        if (scoreP1 > scoreP2)
        {
            Debug.Log("Player 1 Wins!");
            SceneManager.LoadScene("P1Win");
        }
        else if (scoreP2 > scoreP1)
        {
            Debug.Log("Player 2 Wins!");
            SceneManager.LoadScene("P2Win");
        }
        else
        {
            Debug.Log("It's a tie!");
            // tie handling - stay on game over or add a tie scene if desired
        }
    }
}
