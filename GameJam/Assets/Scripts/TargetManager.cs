using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetManager : MonoBehaviour
{
    [System.Serializable]
    public class TargetEntry
    {
        public GameObject prefab;
        [Tooltip("Relative weight for random selection")] public float weight = 1f;
        [Tooltip("Maximum world Y position this prefab is allowed to spawn above. Use a very large value to allow anywhere.")] public float maxSpawnY = 10000f;
        [Tooltip("Lifetime in seconds before it auto-disappears")] public float lifetime = 5f;
    }

    [Header("Available targets (weights control spawn chances)")]
    [SerializeField] private List<TargetEntry> targets = new List<TargetEntry>();

    [Header("Spawn area settings")]
    [Tooltip("Horizontal padding in viewport [0..0.5]")]
    [Range(0f, 0.5f)] public float horizontalPadding = 0.05f;

    private ScoreManager scoreManager;
    private Coroutine spawnCoroutine;

    private void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (scoreManager != null && scoreManager.IsTimerEnded)
                yield break;

            float interval = GetSpawnInterval();
            yield return new WaitForSeconds(interval);

            if (scoreManager != null && scoreManager.IsTimerEnded)
                yield break;

            SpawnRandomTarget();
        }
    }

    private float GetSpawnInterval()
    {
        if (scoreManager == null) return 5f;
        float t = scoreManager.TimeRemaining;
        if (t > 40f) return 4f; // 60-41
        if (t > 20f) return 2f; // 40-21
        return 1f; // 20-0
    }

    private void SpawnRandomTarget()
    {
        if (targets == null || targets.Count == 0) return;

        float total = 0f;
        foreach (var e in targets) if (e != null) total += Mathf.Max(0f, e.weight);
        if (total <= 0f) return;

        float r = Random.Range(0f, total);
        TargetEntry chosen = null;
        float acc = 0f;
        foreach (var e in targets)
        {
            if (e == null) continue;
            acc += Mathf.Max(0f, e.weight);
            if (r <= acc)
            {
                chosen = e;
                break;
            }
        }

        if (chosen == null || chosen.prefab == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // compute max viewport Y for this entry based on maxSpawnY
        float maxVY = 1f;
        if (!float.IsInfinity(chosen.maxSpawnY) && chosen.maxSpawnY < 9999f)
        {
            Vector3 vp = cam.WorldToViewportPoint(new Vector3(0f, chosen.maxSpawnY, 0f));
            maxVY = Mathf.Clamp01(vp.y);
            if (maxVY <= 0f) return; // cannot spawn below camera
        }

        float vx = Random.Range(horizontalPadding, 1f - horizontalPadding);
        float vy = Random.Range(0f, maxVY);
        Vector3 worldPos = cam.ViewportToWorldPoint(new Vector3(vx, vy, Mathf.Abs(cam.transform.position.z)));
        worldPos.z = 0f;

        var go = Instantiate(chosen.prefab, worldPos, Quaternion.identity);
        // ensure TargetBehaviour exists and configure
        var tb = go.GetComponent<TargetBehaviour>();
        if (tb == null) tb = go.AddComponent<TargetBehaviour>();
        tb.lifetime = chosen.lifetime;
        tb.fadeDuration = 0.5f;
    }

    // manual stop
    public void StopSpawning()
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = null;
    }
}
