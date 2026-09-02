using UnityEngine;

public class CoinFlip : MonoBehaviour
{
    [Header("Flip Parameters")]
    [SerializeField] private float ascentDuration = 2f; // time going up
    [SerializeField] private float ascentSpeed = 3f; // configurable speed that influences peak height
    [SerializeField] private float descentDuration = 2f; // time coming back down (default same as ascent)

    private Vector3 startPosition;
    private float peakHeight;
    private float elapsed;

    private enum FlipState { Ascending, Descending, Idle }
    private FlipState state = FlipState.Idle;

    void Start()
    {
        startPosition = transform.position;
        // determine peak height from speed and duration (tunable by changing ascentSpeed)
        peakHeight = ascentSpeed * ascentDuration;
        elapsed = 0f;
        state = FlipState.Ascending;
    }

    void Update()
    {
        if (state == FlipState.Ascending)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / ascentDuration);
            // ease out so it slows near the end: using SmoothStep (ease in-out) biased to ease out
            float eased = EaseOutQuad(t);
            float currentHeight = Mathf.Lerp(0f, peakHeight, eased);
            transform.position = startPosition + Vector3.up * currentHeight;

            if (t >= 1f)
            {
                // start descending
                state = FlipState.Descending;
                elapsed = 0f;
            }
        }
        else if (state == FlipState.Descending)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / descentDuration);
            // ease in so it slowly start falling and then speed up, giving a natural feel into the fall
            float eased = EaseInQuad(t);
            float currentHeight = Mathf.Lerp(peakHeight, 0f, eased);
            transform.position = startPosition + Vector3.up * currentHeight;

            if (t >= 1f)
            {
                // ensure exact original position and stop
                transform.position = startPosition;
                state = FlipState.Idle;
                // optionally disable this component if no longer needed
                // enabled = false;
            }
        }
    }

    // Quadratic ease-out: decelerating to zero velocity
    private static float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    // Quadratic ease-in: accelerating from zero velocity
    private static float EaseInQuad(float t)
    {
        return t * t;
    }
}
