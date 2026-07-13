using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject glitchRunnerPrefab;
    public GameObject bruteMutantPrefab;
    public GameObject soulSwallowerPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Match")]
    public int currentWave;
    public float matchDuration = 900f;
    public float elapsedTime;
    public bool finalBossSpawned;

    private GameMode activeMode = GameMode.Menu;
    private bool wavesRunning;

    public bool IsEndless => activeMode == GameMode.Endless;
    public float RemainingTime => IsEndless ? elapsedTime : Mathf.Max(0f, matchDuration - elapsedTime);

    void Start()
    {
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.GameIsRunning)
            BeginMode(GameFlowManager.Instance.CurrentMode);
    }

    void Update()
    {
        if (!wavesRunning || activeMode == GameMode.Menu)
            return;

        elapsedTime += Time.deltaTime;

        if (IsEndless)
            return;

        if (elapsedTime < matchDuration)
            return;

        elapsedTime = matchDuration;
        wavesRunning = false;
        StopAllCoroutines();

        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.WinTimedMode();
        else
            Time.timeScale = 0f;
    }

    public void BeginMode(GameMode mode)
    {
        activeMode = mode;
        currentWave = 0;
        elapsedTime = 0f;
        finalBossSpawned = false;
        wavesRunning = mode != GameMode.Menu;

        StopAllCoroutines();

        if (wavesRunning)
            StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(3f);

        while (wavesRunning)
        {
            currentWave++;
            Debug.Log($"=== WAVE {currentWave} START ===");

            int runners = Mathf.Min(3 + currentWave * 2, IsEndless ? 28 : 15);
            int brutes = Mathf.Max(0, currentWave - 1);
            int swallowers = currentWave < 2 ? 0 : Mathf.Min(1 + (currentWave - 2) / 2, IsEndless ? 7 : 4);

            SpawnGroup(glitchRunnerPrefab, runners);
            SpawnGroup(bruteMutantPrefab, brutes);
            SpawnSoulSwallowers(swallowers);

            if (currentWave % 5 == 0)
                SpawnMiniBoss();

            yield return new WaitForSeconds(IsEndless ? 18f : 20f);
        }
    }

    void SpawnGroup(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0 || spawnPoints == null || spawnPoints.Length == 0)
            return;

        for (int i = 0; i < count; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Vector3 position = spawnPoint.position + (Vector3)Random.insideUnitCircle * 1.5f;
            GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
            StartCoroutine(ApplyTimeDifficulty(enemy));
        }
    }

    IEnumerator ApplyTimeDifficulty(GameObject enemyObject)
    {
        yield return null;
        if (enemyObject == null)
            yield break;

        EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();
        if (enemy == null)
            yield break;

        float progress = GetDifficultyProgress();
        enemy.ApplyDifficulty(
            Mathf.Lerp(1f, IsEndless ? 3.4f : 2.35f, progress),
            Mathf.Lerp(1f, IsEndless ? 2.4f : 1.8f, progress),
            Mathf.Lerp(1f, IsEndless ? 1.22f : 1.12f, progress));
    }

    float GetDifficultyProgress()
    {
        if (IsEndless)
            return Mathf.Clamp01(elapsedTime / 900f);

        return Mathf.Clamp01(elapsedTime / Mathf.Max(1f, matchDuration));
    }

    void SpawnSoulSwallowers(int count)
    {
        if (count <= 0)
            return;

        if (soulSwallowerPrefab != null)
        {
            SpawnGroup(soulSwallowerPrefab, count);
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

        for (int i = 0; i < count; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject swallower = new GameObject("SoulSwallower");
            swallower.layer = LayerMask.NameToLayer("Enemy");
            swallower.transform.position = spawnPoint.position + (Vector3)Random.insideUnitCircle * 1.5f;
            swallower.transform.localScale = Vector3.one * 0.85f;

            SpriteRenderer renderer = swallower.AddComponent<SpriteRenderer>();
            SpriteRenderer sourceRenderer = bruteMutantPrefab == null ? null : bruteMutantPrefab.GetComponent<SpriteRenderer>();
            if (sourceRenderer != null)
            {
                renderer.sprite = sourceRenderer.sprite;
                renderer.sharedMaterial = sourceRenderer.sharedMaterial;
                renderer.sortingOrder = sourceRenderer.sortingOrder;
            }

            Rigidbody2D body = swallower.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            CircleCollider2D collider = swallower.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.48f;

            SoulSwallower enemy = swallower.AddComponent<SoulSwallower>();
            EnemyBase sourceEnemy = bruteMutantPrefab == null ? null : bruteMutantPrefab.GetComponent<EnemyBase>();
            if (sourceEnemy != null)
                enemy.soulPrefab = sourceEnemy.soulPrefab;

            StartCoroutine(ApplyTimeDifficulty(swallower));
        }
    }

    void SpawnMiniBoss()
    {
        GameObject source = bruteMutantPrefab != null ? bruteMutantPrefab : glitchRunnerPrefab;
        SpawnElite(source, false);
    }

    void SpawnElite(GameObject source, bool finalBoss)
    {
        if (source == null || spawnPoints == null || spawnPoints.Length == 0)
            return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject elite = Instantiate(source, spawnPoint.position, Quaternion.identity);
        StartCoroutine(ConfigureEliteNextFrame(elite, finalBoss));
    }

    IEnumerator ConfigureEliteNextFrame(GameObject enemyObject, bool finalBoss)
    {
        yield return null;
        if (enemyObject == null)
            yield break;

        EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();
        if (enemy == null)
            yield break;

        float progress = GetDifficultyProgress();
        enemy.ApplyDifficulty(
            Mathf.Lerp(1f, IsEndless ? 3.4f : 2.35f, progress),
            Mathf.Lerp(1f, IsEndless ? 2.4f : 1.8f, progress),
            Mathf.Lerp(1f, IsEndless ? 1.22f : 1.12f, progress));

        if (finalBoss)
        {
            enemy.ConfigureElite("FINAL BOSS", 8f, 3f, 0.82f, 2.3f,
                new Color(0.85f, 0.05f, 0.18f), 8);
            enemy.xpReward *= 5f;
        }
        else
        {
            enemy.ConfigureElite("MINI BOSS", 3.5f, 1.8f, 0.92f, 1.65f,
                new Color(1f, 0.2f, 0.65f), 3);
            enemy.xpReward *= 2f;
        }
    }
}
