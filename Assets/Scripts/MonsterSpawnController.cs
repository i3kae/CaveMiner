using UnityEngine;

public class MonsterSpawnController : MonoBehaviour
{
    [SerializeField] private GameObject ghost;
    [SerializeField] private GameObject crazy;
    [SerializeField] private CaveController cave;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float spawnInterval = 60f;
    [SerializeField] private float elapsedTime = 0f;

    private Camera mainCamera;

    void Start()
    {
        cave = GameObject.FindAnyObjectByType<CaveController>();
        width = cave.GetWidth();
        height = cave.GetHeight();

        mainCamera = Camera.main;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= spawnInterval && spawnInterval >= 0.0001f)
        {
            SpawnPrefab();
            elapsedTime = 0f;
            spawnInterval *= 0.6f;
        }
    }

    void SpawnPrefab()
    {
        Vector2 spawnPosition = GetRandomSpawnPosition();

        GameObject prefabToSpawn;

        if (Random.Range(1, 10) > 2) prefabToSpawn = crazy;
        else prefabToSpawn = ghost;
        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
    }

    Vector2 GetRandomSpawnPosition()
    {
        Vector2 randomPosition;
        bool isInsideViewport = true;

        do
        {
            randomPosition = new Vector2(Random.Range(0, width - 1), Random.Range(0, height - 1));
            if (cave.GetMineral((int)randomPosition.x, (int)randomPosition.y) != 0) continue;
            randomPosition.x += -width / 2;
            randomPosition.y += -height / 2;
            isInsideViewport = IsInView(randomPosition);
        } while (isInsideViewport);

        return randomPosition;
    }

    bool IsInView(Vector2 position)
    {
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(position);

        return viewportPosition.x >= 0 && viewportPosition.x <= 1 &&
               viewportPosition.y >= 0 && viewportPosition.y <= 1;
    }
}
