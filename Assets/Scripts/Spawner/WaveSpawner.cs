using System.Collections;
using UnityEngine;

// Điều khiển toàn bộ nhịp của màn chơi 15 phút:
// sinh wave, tăng sức mạnh quái, tạo mini boss và boss cuối.
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

    // Số giây còn lại để GameHUD hiển thị đồng hồ đếm ngược.
    public float RemainingTime => Mathf.Max(0f, matchDuration - elapsedTime);

    // Bắt đầu chuỗi wave khi Scene chạy.
    void Start()
    {
        StartCoroutine(RunWaves());
    }

    // Đếm thời gian trận đấu; khi hết giờ thì dừng wave và gọi boss cuối.
    void Update()
    {
        if (finalBossSpawned)
            return;

        elapsedTime += Time.deltaTime;
        if (elapsedTime < matchDuration)
            return;

        elapsedTime = matchDuration;
        finalBossSpawned = true;
        StopAllCoroutines();
        SpawnFinalBoss();
    }

    // Cứ mỗi 20 giây tạo wave mới; mỗi wave thứ 5 có thêm mini boss.
    IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(3f);

        while (!finalBossSpawned)
        {
            currentWave++;
            Debug.Log($"=== WAVE {currentWave} START ===");

            int runners = Mathf.Min(3 + currentWave * 2, 15);
            int brutes = Mathf.Max(0, currentWave - 1);
            int swallowers = currentWave < 2 ? 0 : Mathf.Min(1 + (currentWave - 2) / 2, 4);

            SpawnGroup(glitchRunnerPrefab, runners);
            SpawnGroup(bruteMutantPrefab, brutes);
            SpawnSoulSwallowers(swallowers);

            if (currentWave % 5 == 0)
                SpawnMiniBoss();

            yield return new WaitForSeconds(20f);
        }
    }

    // Sinh một nhóm quái từ prefab tại các SpawnPoint ngẫu nhiên.
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

    // Chờ quái hoàn tất Start rồi tăng chỉ số dựa trên tiến độ 15 phút.
    IEnumerator ApplyTimeDifficulty(GameObject enemyObject)
    {
        yield return null;
        if (enemyObject == null)
            yield break;

        EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();
        if (enemy == null)
            yield break;

        float progress = Mathf.Clamp01(elapsedTime / matchDuration);
        enemy.ApplyDifficulty(
            Mathf.Lerp(1f, 2.35f, progress),
            Mathf.Lerp(1f, 1.8f, progress),
            Mathf.Lerp(1f, 1.12f, progress));
    }

    // Sinh Soul Swallower từ prefab; nếu chưa có prefab thì tạo hình tạm bằng code.
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

    // Mini boss dùng Brute Mutant làm hình tạm và xuất hiện mỗi 5 wave.
    void SpawnMiniBoss()
    {
        GameObject source = bruteMutantPrefab != null ? bruteMutantPrefab : glitchRunnerPrefab;
        SpawnElite(source, false);
    }

    // Boss cuối xuất hiện đúng một lần khi đồng hồ về 00:00.
    void SpawnFinalBoss()
    {
        GameObject source = bruteMutantPrefab != null ? bruteMutantPrefab : glitchRunnerPrefab;
        SpawnElite(source, true);
        Debug.Log("=== FINAL BOSS HAS APPEARED ===");
    }

    // Tạo một quái elite tại SpawnPoint ngẫu nhiên.
    void SpawnElite(GameObject source, bool finalBoss)
    {
        if (source == null || spawnPoints == null || spawnPoints.Length == 0)
            return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject elite = Instantiate(source, spawnPoint.position, Quaternion.identity);
        StartCoroutine(ConfigureEliteNextFrame(elite, finalBoss));
    }

    // Chờ chỉ số gốc được khởi tạo rồi áp dụng bộ chỉ số mini boss hoặc boss cuối.
    IEnumerator ConfigureEliteNextFrame(GameObject enemyObject, bool finalBoss)
    {
        yield return null;
        if (enemyObject == null)
            yield break;

        EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();
        if (enemy == null)
            yield break;

        float progress = Mathf.Clamp01(elapsedTime / matchDuration);
        enemy.ApplyDifficulty(
            Mathf.Lerp(1f, 2.35f, progress),
            Mathf.Lerp(1f, 1.8f, progress),
            Mathf.Lerp(1f, 1.12f, progress));

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
