using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;

public class CaveGenerator : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;

    [SerializeField] private string seed;
    [SerializeField] private bool useRandomSeed;

    [Range(0, 100)]
    [SerializeField] private int randomFillPercent;
    [SerializeField] private int smoothNum;
    [SerializeField] private int expandRoadNum;
    [SerializeField] private int expandWallNum;

    private int[,] map;
    private const int ROAD = 0;
    private const int WALL = 1;

    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap roadTilemap;
    [SerializeField] private Tile wallTile;
    [SerializeField] private Tile roadTile;
    [SerializeField] private Color[] colors;

    private bool checker = true;

    private void Update()
    {
        // if (checker && Input.GetMouseButtonDown(0)) GenerateMap();
        if (Input.GetMouseButtonDown(0)) GenerateMap();
    }

    private void GenerateMap()
    {
        checker = false;
        map = new int[width, height];
        MapRandomFill();

        for (int i = 0; i < smoothNum; i++) SmoothMap();
        for (int i = 0; i < expandRoadNum; i++) ExpandMap(ROAD);
        for (int i = 0; i < smoothNum; i++) SmoothMap();
        for (int i = 0; i < expandWallNum; i++) ExpandMap(WALL);
        for (int i = 0; i < smoothNum; i++) SmoothMap();
        FillMap();
    }
    
    private void FillMap()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                OnDrawTile(x, y);
    }
    private void MapRandomFill() //���� ������ ���� �� Ȥ�� �� �������� �����ϰ� ä��� �޼ҵ�
    {
        if (useRandomSeed) seed = Time.time.ToString(); //�õ�

        System.Random pseudoRandom = new System.Random(seed.GetHashCode()); //�õ�� ���� �ǻ� ���� ����

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1) map[x, y] = WALL; //�����ڸ��� ������ ä��
                else map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? WALL : ROAD; //������ ���� �� Ȥ�� �� ���� ����
            }
        }
    }

    private void ExpandMap(int OBJ)
    {
        if (useRandomSeed) seed = Time.time.ToString(); //�õ�

        System.Random pseudoRandom = new System.Random(seed.GetHashCode()); //�õ�� ���� �ǻ� ���� ����

        int[,] expandPath = {
            {-1, 0 }, {1, 0 },
            {0, -1 }, {0, 1 }
        };
        bool[,] checker = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!checker[x, y] && map[x, y] == OBJ)
                {
                    checker[x, y] = true;
                    for (int i = 0; i < 4; i++)
                    {
                        int nextX = x + expandPath[i, 0], nextY = y + expandPath[i, 1];
                        if (1 <= nextX && nextX < width - 1 &&
                            1 <= nextY && nextY < height - 1 &&
                            !checker[nextX, nextY] && pseudoRandom.Next(0, 2) == 1)
                        {
                            map[nextX, nextY] = OBJ;
                            checker[nextX, nextY] = true;
                        }
                    }
                }
            }
        }
    }

    private void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);
                if (neighbourWallTiles > 6) map[x, y] = WALL; //�ֺ� ĭ �� ���� 4ĭ�� �ʰ��� ��� ���� Ÿ���� ������ �ٲ�
                else if (neighbourWallTiles < 4) map[x, y] = ROAD; //�ֺ� ĭ �� ���� 4ĭ �̸��� ��� ���� Ÿ���� �� �������� �ٲ�
            }
        }
    }

    private int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        { //���� ��ǥ�� �������� �ֺ� 8ĭ �˻�
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                { //�� ������ �ʰ����� �ʰ� ���ǹ����� �˻�
                    if (neighbourX != gridX || neighbourY != gridY) wallCount += map[neighbourX, neighbourY]; //���� 1�̰� �� ������ 0�̹Ƿ� ���� ��� wallCount ����
                }
                else wallCount++; //�ֺ� Ÿ���� �� ������ ��� ��� wallCount ����
            }
        }
        return wallCount;
    }

    private void OnDrawTile(int x, int y)
    {
        Vector3Int pos = new Vector3Int(-width / 2 + x, -height / 2 + y, 0);
        roadTilemap.SetTile(pos, null);
        wallTilemap.SetTile(pos, null);
        switch (map[x, y])
        {
            case ROAD: roadTilemap.SetTile(pos, roadTile); break;
            case WALL: wallTilemap.SetTile(pos, wallTile); break;
        }
    }
}