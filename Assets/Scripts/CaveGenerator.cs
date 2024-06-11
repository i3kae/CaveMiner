using System;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class CaveGenerator : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;

    [Range(0, 100)]
    [SerializeField] private int randomFillPercent;
    [SerializeField] private string seed;
    [SerializeField] private bool useRandomSeed;
    [SerializeField] private int smoothNum;
    [SerializeField] private int expandRoadNum;
    [SerializeField] private int expandWallNum;
    [SerializeField] private int portalDistance;
    [SerializeField] private int mapPadding;

    [SerializeField] private Tile wallTile;
    [SerializeField] private Tile roadTile;
    [SerializeField] private Tile portalTile;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap roadTilemap;
    [SerializeField] private Tilemap portalTilemap;

    private int[,] map;
    private const int ROAD = 0;
    private const int WALL = 1;
    private const int PORTAL = 2;
    private System.Random pseudoRandom;

    private void Awake()
    {
        if (useRandomSeed) seed = DateTime.Now.Ticks.ToString(); //시드
        pseudoRandom = new System.Random(seed.GetHashCode());
        GenerateMap();
    }

    private void GenerateMap()
    {
        map = new int[width, height];
        int[] mineralValues = new int[4];
        do
        {
            MapRandomFill();
            for (int i = 0; i < smoothNum; i++) SmoothMap();
            for (int i = 0; i < expandRoadNum; i++) ExpandMap(ROAD);
            for (int i = 0; i < smoothNum; i++) SmoothMap();
            for (int i = 0; i < expandWallNum; i++) ExpandMap(WALL);
            for (int i = 0; i < smoothNum; i++) SmoothMap();
        } while (!IsValidMap());
        GenerateEntrance();
        for (int i = 0; i < smoothNum; i++) SmoothMap();
        FillMap();
    }
    public class pair
    {
        public int first { get; set; }
        public int second { get; set; }

        public pair(int first, int second)
        {
            this.first = first;
            this.second = second;
        }
    }
    private bool IsValidMap()
    {
        int[,] mover = {
            {-1, 0 }, {1, 0 },
            {0, -1 }, {0, 1 }
        };
        int[,] checker = new int[width, height];
        int maxMapCnt = 0, maxMapValue = 0;
        int nowValue = 1;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == ROAD && checker[x, y] == 0)
                {
                    int cnt = 0;
                    Queue<pair> q = new Queue<pair>();
                    q.Enqueue(new pair(x, y));
                    for (; q.Count > 0; cnt++)
                    {
                        pair nowPos = q.Dequeue();
                        checker[nowPos.first, nowPos.second] = nowValue;
                        for (int i = 0; i < 4; i++)
                        {
                            int nextX = nowPos.first + mover[i, 0], nextY = nowPos.second + mover[i, 1];
                            if (map[nextX, nextY] == ROAD && checker[nextX, nextY] == 0)
                            {
                                checker[nextX, nextY] = nowValue;
                                q.Enqueue(new pair(nextX, nextY));
                            }
                        }
                    }
                    if (cnt > maxMapCnt)
                    {
                        maxMapCnt = cnt;
                        maxMapValue = nowValue;
                    }
                    nowValue++;
                }
            }
        }
        if (maxMapCnt >= (width * height) / 4)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (map[x, y] == ROAD && checker[x, y] != maxMapValue)
                    {
                        map[x, y] = WALL;
                    }
                }
            }
            return true;
        }
        return false;
    }
    private void GenerateEntrance()
    {
        // 차후 4 방면 중 한 곳에서 생성되도록 수정
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
        int entranceX, entranceY = 0;
        entranceX = pseudoRandom.Next(portalDistance + 3, width - portalDistance - 3);
        for (int i = entranceX - portalDistance; i <= entranceX + portalDistance; i++)
        {
            map[i, entranceY] = PORTAL;
        }

        bool flag = true;
        int x, y;
        for (x = entranceX, y = entranceY; flag || (y - entranceY) < 20;)
        {
            if (map[x, y] == ROAD) flag = false;
            for (int i = -portalDistance - 1; i <= portalDistance + 1; i++)
            {
                if (map[x, y] == PORTAL) break;
                if (1 <= x + i && x + i < width - 1) map[x + i, y] = ROAD;
            }
            y++;
            x += pseudoRandom.Next(-1, 2);
        }
        Vector3 playerPosition = new Vector3(-width / 2 + entranceX, -height / 2 + (entranceY + 5), 0);
        player.GetComponent<PlayerController>().SetPosition(playerPosition);
        camera.GetComponent<CameraController>().SetPosition(playerPosition);
    }

    private void FillMap()
    {
        for (int x = -mapPadding; x < width + mapPadding; x++)
            for (int y = -mapPadding / 2; y < height + mapPadding / 2; y++)
                OnDrawTile(x, y);
    }
    private void MapRandomFill()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == PORTAL) continue;
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1) map[x, y] = WALL;
                else map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? WALL : ROAD;
            }
        }
    }

    private void ExpandMap(int OBJ)
    {
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
                            checker[nextX, nextY] = true;
                            if (map[nextX, nextY] == PORTAL) continue;
                            map[nextX, nextY] = OBJ;
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
                if (map[x, y] == PORTAL) continue;
                int neighbourWallTiles = GetSurroundingWallCount(x, y);
                if (neighbourWallTiles > 6) map[x, y] = WALL;
                else if (neighbourWallTiles < 4) map[x, y] = ROAD;
            }
        }
    }

    private int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        { 
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                { 
                    if (map[neighbourX, neighbourY] == PORTAL) continue; 
                    if (neighbourX != gridX || neighbourY != gridY) wallCount += map[neighbourX, neighbourY]; 
                }
                else wallCount++;
            }
        }
        return wallCount;
    }

    private void OnDrawTile(int x, int y)
    {
        Vector3Int pos = new Vector3Int(-width / 2 + x, -height / 2 + y, 0);

        if (!(x >= 0 && x < width && y >= 0 && y < height))
        {
            wallTilemap.SetTile(pos, wallTile);
            return;
        }

        switch (map[x, y])
        {
            case ROAD: roadTilemap.SetTile(pos, roadTile); break;
            case WALL: wallTilemap.SetTile(pos, wallTile); break;
            case PORTAL: portalTilemap.SetTile(pos, portalTile); break;
        }
    }
}