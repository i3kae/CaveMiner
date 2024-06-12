using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class CaveController : MonoBehaviour
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
    [SerializeField] private int[] mineralSpotsMid;
    [SerializeField] private int[] mineralValuesMid;

    [SerializeField] private Tile wallTile;
    [SerializeField] private Tile roadTile;
    [SerializeField] private Tile portalTile;
    [SerializeField] private Tile coalTile;
    [SerializeField] private Tile ironTile;
    [SerializeField] private Tile goldTile;
    [SerializeField] private Tile diamondTile;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap roadTilemap;
    [SerializeField] private Tilemap portalTilemap;
    [SerializeField] private Tilemap mineralTilemap;

    [SerializeField] private int[,] map;
    [SerializeField] private int[,] mineralMap;
    [SerializeField] private float[] mineralHPs;
    private const int ROAD = 0;
    private const int WALL = 1;
    private const int PORTAL = 2;
    private const int COAL = 3;
    private const int IRON = 4;
    private const int GOLD = 5;
    private const int DIAMOND = 6;

    private System.Random pseudoRandom;

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

    public class triple
    {
        public int first { get; set; }
        public int second { get; set; }
        public int third { get; set; }

        public triple(int first, int second, int third)
        {
            this.first = first;
            this.second = second;
            this.third = third;
        }
    }

    private void Awake()
    {
        if (useRandomSeed) seed = DateTime.Now.Ticks.ToString(); //시드
        pseudoRandom = new System.Random(seed.GetHashCode());
        GenerateMap();
    }

    private void GenerateMap()
    {
        pair playerPos;
        map = new int[width, height];
        mineralMap = new int[width, height];
        do
        {
            MapRandomFill();
            for (int i = 0; i < smoothNum; i++) SmoothMap();
            for (int i = 0; i < expandRoadNum; i++) ExpandMap(ROAD);
            for (int i = 0; i < smoothNum; i++) SmoothMap();
            for (int i = 0; i < expandWallNum; i++) ExpandMap(WALL);
            for (int i = 0; i < smoothNum; i++) SmoothMap();
        } while (!IsValidMap());
        
        playerPos = GenerateEntrance();
        for (int i = 0; i < smoothNum; i++) SmoothMap();

        GenerateMinerals(playerPos);
        FillMap();
    }

    private int[] initMineralSpots()
    {
        int[] mineralSpots = new int[4];
        int spotCnt = 0;
        for (int i = 0; i < 4; i++)
        {
            mineralSpots[i] = pseudoRandom.Next(mineralSpotsMid[i] - mineralSpotsMid[i] / 4, mineralSpotsMid[i] + mineralSpotsMid[i] / 2);
            spotCnt += mineralSpots[i];
        }
        mineralHPs = new float[spotCnt * 2];
        return mineralSpots;
    }

    private void GenerateMinerals(pair playerPos)
    {
        int[,] mover = {
            {-1, 0 }, {1, 0 },
            {0, -1 }, {0, 1 }
        };
        bool[,] checker = new bool[width, height];
        int maxDistance = CalcCaveDistance(playerPos);
        int[] mineralSpots = initMineralSpots();
        int mineralNumber = 1;
        Queue<triple> Q = new Queue<triple>();
        Q.Enqueue(new triple(playerPos.first, playerPos.second, 0));
        while (Q.Count > 0)
        {
            triple nowPos = Q.Dequeue();
            if (nowPos.third > maxDistance)
            {
                maxDistance = nowPos.third;
            }
            for (int i = 0; i < 4; i++)
            {
                int nextX = nowPos.first + mover[i, 0], nextY = nowPos.second + mover[i, 1];
                if (!(0 <= nextX && nextX < width && 0 <= nextY && nextY < height)) continue;
                if (map[nextX, nextY] == ROAD && !checker[nextX, nextY])
                {
                    checker[nextX, nextY] = true;
                    Q.Enqueue(new triple(nextX, nextY, nowPos.third + 1));
                }
                if (map[nextX, nextY] == WALL && pseudoRandom.Next(0, 45) == 0)
                {
                    if (nowPos.third >= maxDistance / 100 && mineralSpotsMid[0] > 0 && pseudoRandom.Next(0, 3) == 1)
                    {
                        SpreadMineral(COAL, nextX, nextY, mineralNumber);
                        mineralHPs[mineralNumber++] = 5.0f;
                        mineralSpotsMid[0]--;
                    }
                    else if (nowPos.third >= maxDistance / 50 && mineralSpotsMid[1] > 0 && pseudoRandom.Next(0, 4) == 1)
                    {
                        SpreadMineral(IRON, nextX, nextY, mineralNumber);
                        mineralHPs[mineralNumber++] = 8.0f;
                        mineralSpotsMid[1]--;
                    }
                    else if (nowPos.third >= maxDistance / 5 && mineralSpotsMid[2] > 0 && pseudoRandom.Next(0, 6) == 1)
                    {
                        SpreadMineral(GOLD, nextX, nextY, mineralNumber);
                        mineralHPs[mineralNumber++] = 10.0f;
                        mineralSpotsMid[2]--;
                    }
                    else if (nowPos.third >= maxDistance / 1.4f && mineralSpotsMid[3] > 0 && pseudoRandom.Next(0, 6) == 1)
                    {
                        SpreadMineral(DIAMOND, nextX, nextY, mineralNumber);
                        mineralHPs[mineralNumber++] = 30.0f;
                        mineralSpotsMid[3]--;
                    }
                }
            }
        }
    }

    public void removeMineral(int x, int y, int mineralNumber)
    {
        int[,] mover = {
            {-1, 0 }, {1, 0 },
            {0, -1 }, {0, 1 }
        };
        Queue<pair> Q = new Queue<pair>();
        Q.Enqueue(new pair(x, y));
        map[x, y] = ROAD;
        mineralMap[x, y] = 0;
        OnDrawTile(x, y);
        while (Q.Count > 0)
        {
            pair nowPos = Q.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                int nextX = nowPos.first + mover[i, 0], nextY = nowPos.second + mover[i, 1];
                if (!(0 <= nextX && nextX < width && 0 <= nextY && nextY < height)) continue;
                if (mineralMap[nextX, nextY] == mineralNumber)
                {
                    map[nextX, nextY] = ROAD;
                    mineralMap[nextX, nextY] = 0;
                    OnDrawTile(nextX, nextY);
                    Q.Enqueue(new pair(nextX, nextY));
                }
            }
        }
    }

    private void SpreadMineral(int mineral, int x, int y, int number)
    {
        int mineralCnt = pseudoRandom.Next(mineralValuesMid[mineral - COAL] - mineralValuesMid[mineral - COAL] / 4, 
            mineralValuesMid[mineral - COAL] + mineralValuesMid[mineral - COAL] / 4);
        int[,] mover = {
            {-1, 0 }, {1, 0 },
            {0, -1 }, {0, 1 }
        };
        Queue<pair> Q = new Queue<pair>();
        Q.Enqueue(new pair(x, y));
        map[x, y] = mineral;
        mineralMap[x, y] = number;
        for (; Q.Count > 0 && mineralCnt > 0; mineralCnt--)
        {
            pair nowPos = Q.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                int nextX = nowPos.first + mover[i, 0], nextY = nowPos.second + mover[i, 1];
                if (!(0 <= nextX && nextX < width && 0 <= nextY && nextY < height)) continue;
                if (map[nextX, nextY] == WALL)
                {
                    map[nextX, nextY] = mineral;
                    mineralMap[nextX, nextY] = number;
                    Q.Enqueue(new pair(nextX, nextY));
                }
            }
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
                            if (!(0 <= nextX && nextX < width && 0 <= nextY && nextY < height)) continue;
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
    private pair GenerateEntrance()
    {
        // 차후 4 방면 중 한 곳에서 생성되도록 수정
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
        int entranceX, entranceY = 0;
        entranceX = pseudoRandom.Next(width / 2 - portalDistance, width / 2 + portalDistance);
        for (int i = entranceX - portalDistance; i <= entranceX + portalDistance; i++)
        {
            map[i, entranceY] = PORTAL;
        }

        bool flag = true;
        int x, y;
        for (x = entranceX, y = entranceY; flag || (y - entranceY) < 30;)
        {
            if (x < 0) x = 0;
            else if (x >= width) x = width - 1;
            
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
        return new pair(entranceX, entranceY);
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
                    if (neighbourX != gridX || neighbourY != gridY) wallCount += ((map[neighbourX, neighbourY] == WALL) ? 1 : 0); 
                }
                else wallCount++;
            }
        }
        return wallCount;
    }

    private int CalcCaveDistance(pair playerPos)
    {
        int[,] mover = {
            {-1, 0 }, {1, 0 },
            {0, -1 }, {0, 1 }
        };
        bool[,] checker = new bool[width, height];
        int maxDistance = 0;
        Queue<triple> Q = new Queue<triple>();
        Q.Enqueue(new triple(playerPos.first, playerPos.second, 0));
        while (Q.Count > 0)
        {
            triple nowPos = Q.Dequeue();
            if (nowPos.third > maxDistance)
            {
                maxDistance = nowPos.third;
            }
            for (int i = 0; i < 4; i++)
            {
                int nextX = nowPos.first + mover[i, 0], nextY = nowPos.second + mover[i, 1];
                if (!(0 <= nextX && nextX < width && 0 <= nextY && nextY < height)) continue;
                if (map[nextX, nextY] == ROAD && !checker[nextX, nextY])
                {
                    checker[nextX, nextY] = true;
                    Q.Enqueue(new triple(nextX, nextY, nowPos.third + 1));
                }
            }
        }
        return maxDistance;
    }
    private void OnDrawTile(int x, int y)
    {
        Vector3Int pos = new Vector3Int(-width / 2 + x, -height / 2 + y, 0);
        mineralTilemap.SetTile(pos, null);
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
            case COAL: mineralTilemap.SetTile(pos, coalTile); break;
            case IRON: mineralTilemap.SetTile(pos, ironTile); break;
            case GOLD: mineralTilemap.SetTile(pos, goldTile); break;
            case DIAMOND: mineralTilemap.SetTile(pos, diamondTile); break;
        }
    }

    public float GetMineralHP(int number)
    {
        return mineralHPs[number];
    }
    public int GetMineralNumber(int x, int y)
    {
        return mineralMap[x, y];
    }
    public int GetMineral(int x, int y)
    {
        return map[x, y];
    }

    public int GetWidth()
    {
        return width;
    }
    public int GetHeight()
    {
        return height;
    }

    public void SetMineralHP(int number, float value)
    {
        mineralHPs[number] = value;
    }
}