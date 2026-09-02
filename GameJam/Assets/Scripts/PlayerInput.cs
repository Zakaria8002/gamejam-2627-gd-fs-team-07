using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerInput : MonoBehaviour
{
    public enum ControlScheme { WASD, Arrows, Joystick }

    [SerializeField] private ControlScheme controlScheme = ControlScheme.WASD;
    [SerializeField] private int joystickIndex = 1; // 1-based index for joysticks
    [SerializeField] private float Speed = 10f;
    [SerializeField] private int PlayerNumber = 1;
    private Rigidbody2D rb;
    private Collider2D ownCollider;
    private SpriteRenderer ownSpriteRenderer;
    [SerializeField] private float actionCooldown = 0.2f;
    private float lastActionTime = -Mathf.Infinity;
    [Header("Action Scale Feedback")]
    [SerializeField] private float scaleMultiplier = 1.3f;
    [SerializeField] private float scaleGrowDuration = 0.3f;
    [SerializeField] private float scaleShrinkDuration = 0.3f;
    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    public ScoreManager scoreManager; // Reference to the ScoreManager script

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ownCollider = GetComponent<Collider2D>();
        ownSpriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();
    }

    private void Update()
    {
        Vector2 movement = Vector2.zero;

        switch (controlScheme)
        {
            case ControlScheme.WASD:
                movement.x = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
                movement.y = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
                break;

            case ControlScheme.Arrows:
                movement.x = (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
                movement.y = (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
                break;

            case ControlScheme.Joystick:
                var names = Input.GetJoystickNames();
                if (joystickIndex >= 1 && joystickIndex <= names.Length && !string.IsNullOrEmpty(names[joystickIndex - 1]))
                {
                    // Expectation: define axes in Input Manager like "Joy1_Horizontal", "Joy1_Vertical", "Joy2_Horizontal", etc.
                    string hAxis = $"Joy{joystickIndex}_Horizontal";
                    string vAxis = $"Joy{joystickIndex}_Vertical";

                    try
                    {
                        movement.x = Input.GetAxis(hAxis);
                        movement.y = Input.GetAxis(vAxis);
                    }
                    catch
                    {
                        // Fallback: if custom axes aren't configured, try the default "Horizontal"/"Vertical" (may mix inputs).
                        movement.x = Input.GetAxis("Horizontal");
                        movement.y = Input.GetAxis("Vertical");
                    }
                }
                else
                {
                    // No joystick connected at that index: fallback to arrows to avoid no-input situation
                    movement.x = (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
                    movement.y = (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
                }
                break;
        }

        // Keep movement speed uniform when moving diagonally
        if (movement.sqrMagnitude > 1f) movement = movement.normalized;

        // apply velocity
        rb.linearVelocity = movement * Speed;

        // Check action button per control scheme
        bool actionPressed = false;
        switch (controlScheme)
        {
            case ControlScheme.WASD:
                actionPressed = Input.GetKeyDown(KeyCode.E);
                break;
            case ControlScheme.Arrows:
                actionPressed = Input.GetKeyDown(KeyCode.RightShift);
                break;
            case ControlScheme.Joystick:
                // map to JoystickN Button0 (first button) by default
                if (joystickIndex >= 1 && joystickIndex <= 8)
                {
                    int baseKey = (int)KeyCode.Joystick1Button0 + (joystickIndex - 1) * 20; // 20 buttons per joystick block in KeyCode
                    KeyCode joyButton = (KeyCode)baseKey;
                    actionPressed = Input.GetKeyDown(joyButton);
                }
                break;
        }

        if (actionPressed)
        {
            if (Time.time - lastActionTime >= actionCooldown)
            {

                SoundManager.Instance.GunSound(); // Play the action sound
                CheckOverlapAndLog();
                lastActionTime = Time.time;
            }
        }
    }

    private void CheckOverlapAndLog()
    {
        Bounds bounds;
        if (ownCollider != null)
        {
            bounds = ownCollider.bounds;
        }
        else if (ownSpriteRenderer != null)
        {
            bounds = ownSpriteRenderer.bounds;
        }
        else
        {
            Debug.LogWarning("PlayerInput: no Collider2D or SpriteRenderer found to check overlaps.");
            return;
        }

        Vector2 a = bounds.min;
        Vector2 b = bounds.max;

        Collider2D[] hits = Physics2D.OverlapAreaAll(a, b);
        foreach (var hit in hits)
        {
            if (hit == null) continue;
            if (hit.gameObject == this.gameObject) continue;

            string tag = hit.gameObject.tag;
            if (tag == "Coin")
            {
                Debug.Log("100");
                // notify target so it can disappear
                var tb = hit.GetComponent<TargetBehaviour>();
                if (tb != null) tb.OnChecked();
                if (scoreManager != null) scoreManager.ChangeScore(PlayerNumber, 100);
            }
            else if (tag == "FlippingCoin")
            {
                Debug.Log("300");
                var tb = hit.GetComponent<TargetBehaviour>();
                if (tb != null) tb.OnChecked();
                if (scoreManager != null) scoreManager.ChangeScore(PlayerNumber, 300);
            }
            else if (tag == "Cactus")
            {
                Debug.Log("-100");
                var tb = hit.GetComponent<TargetBehaviour>();
                if (tb != null) tb.OnChecked();
                if (scoreManager != null) scoreManager.ChangeScore(PlayerNumber, -100);
            }
        }

        // Play a small scale feedback after the overlap check
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleFeedback());
    }

    private System.Collections.IEnumerator ScaleFeedback()
    {
        Vector3 target = originalScale * scaleMultiplier;

        float t = 0f;
        while (t < scaleGrowDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, target, Mathf.Clamp01(t / scaleGrowDuration));
            yield return null;
        }
        transform.localScale = target;

        t = 0f;
        while (t < scaleShrinkDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(target, originalScale, Mathf.Clamp01(t / scaleShrinkDuration));
            yield return null;
        }

        transform.localScale = originalScale;
        scaleCoroutine = null;
    }
}