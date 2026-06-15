using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI hintText;        // ← ADD THIS
    public float hintDisplayDuration = 4f;

    [Header("Timed Messages")]
    [SerializeField] private TextMeshProUGUI timedMessageText;
    [SerializeField] private string timedMessage = "Hurry up! Time running low!";
    [SerializeField] private float showAtSecond = 30f; // shows after 30 seconds
    [SerializeField] private float timedMessageDuration = 3f;


    [SerializeField] private TextMeshProUGUI timedMessageText2;
    [SerializeField] private string timedMessage2 = "Almost out of time!";
    [SerializeField] private float showAtSecond2 = 60f;
    [SerializeField] private float timedMessageDuration2 = 3f;

    [Header("Player Reference")]
    [SerializeField] private GameObject player;

    [Header("Settings")]
    public float timeLimit = 60f;

    private float timeRemaining;
    private bool  gameActive = true;

    [Header("Targets")]
// Total targets in scene — auto-counted on Start
    private int totalTargets;
    private int targetsDestroyed = 0;

    void Awake()
    {
        instance = this;
    }

    void Start()
{

    if (timedMessageText != null)
{
    timedMessageText.gameObject.SetActive(false);
    Invoke(nameof(ShowTimedMessage), showAtSecond);
}

 if (timedMessageText2 != null)
{
    timedMessageText2.gameObject.SetActive(false);
    Invoke(nameof(ShowTimedMessage2), showAtSecond2);
}
    timeRemaining = timeLimit;
    if (gameOverPanel != null)
        gameOverPanel.SetActive(false);

    totalTargets    = GameObject.FindGameObjectsWithTag("Target").Length;
    targetsDestroyed = 0;

    // Show hint text then hide after duration
    if (hintText != null)
    {
        hintText.text = "Find and eliminate all hidden red targets before time runs out!";
        hintText.gameObject.SetActive(true);
        Invoke(nameof(HideHint), hintDisplayDuration);
    }
}


private void ShowTimedMessage()
{
    timedMessageText.text = timedMessage;
    timedMessageText.gameObject.SetActive(true);
    Invoke(nameof(HideTimedMessage), timedMessageDuration);
}
private void HideTimedMessage()
{
    timedMessageText.gameObject.SetActive(false);
}


private void ShowTimedMessage2()
{
    timedMessageText2.text = timedMessage2;
    timedMessageText2.gameObject.SetActive(true);
    Invoke(nameof(HideTimedMessage2), timedMessageDuration2);
}
private void HideTimedMessage2()
{
    timedMessageText2.gameObject.SetActive(false);
}


private void HideHint()
{
    if (hintText != null)
        hintText.gameObject.SetActive(false);
}

    void Update()
    {
        if (!gameActive) return;

        timeRemaining -= Time.deltaTime;
        timeRemaining = Mathf.Max(timeRemaining, 0f);

        if (timerText != null)
            timerText.text = "Time: " + Mathf.CeilToInt(timeRemaining);

        if (timeRemaining <= 0f)
            EndGame(false);
    }

public void EndGame(bool won)
{
    gameActive = false;

    if (gameOverPanel != null)
        gameOverPanel.SetActive(true);

    if (resultText != null)
        resultText.text = won ? "You Win!" : "Time's Up!";

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible   = true;

    // Disable all player scripts so clicking
    // doesn't shoot or rotate camera anymore
    if (player != null)
    {
        var scripts = player.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in scripts)
        {
            script.enabled = false;
        }
    }
}
    // Called by Target script when destroyed
public void TargetDestroyed()
{
    targetsDestroyed++;

    // Only win when ALL targets are destroyed
    if (targetsDestroyed >= totalTargets)
    {
        EndGame(true);
    }
}

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}