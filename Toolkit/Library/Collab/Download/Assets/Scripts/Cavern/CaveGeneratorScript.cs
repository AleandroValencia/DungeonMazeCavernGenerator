using UnityEngine;
using System.Collections.Generic;
// Note: using System is used for IComparable interface to compare room sizes (not necessary in DG_ToolKitEditorWindow)
using System; 

public class CaveGeneratorScript : MonoBehaviour
{
    [HideInInspector]
   public CaveMeshGeneratorScript meshGenerator;

    public int width, height;
    public bool GenerateAtRunTime = false;
    public string seed;
    public bool generateRandomSeed;
    public bool generateExits;
    public bool generateEntrance;
    public bool generateGround;

    [Range(1, 10)] public int borderSize = 2;
    [Range(0, 30)] public int smoothness = 10;
    [Range(0, 100)] public int fillPercent;

    enum Meshes { EMPTY = 0, WALL = 1 };
    int[,] map;

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that initializes variables/objects
    // ------------------------------
    void Start()
    {
        if (GenerateAtRunTime)
        {
            meshGenerator = GetComponent<CaveMeshGeneratorScript>();
            generateRandomSeed = true;
            generateExits = false;
            generateEntrance = false;
            generateGround = false;
            GenerateMap();
        }
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that checks for a right mouse button click and generates a cave 
    // ------------------------------
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
            GenerateMap();
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that generates a cave and adds border 
    // ------------------------------
    public void GenerateMap()
    {
        map = new int[width, height];

        PopulateMap();

        for (int i = 0; i < smoothness; i++)
            Smooth();

        ProcessMap();

        int borderSize = 1;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int i = 0; i < borderedMap.GetLength(0); ++i)
            for (int j = 0; j < borderedMap.GetLength(1); ++j)
                if (j >= borderedMap.GetLength(1) - 5 && j <= borderedMap.GetLength(1) && generateExits)
                    // Create exit(s)
                    borderedMap[i, j] = (int)Meshes.EMPTY;
                else if (i >= borderSize && i < width + borderSize && j >= borderSize && j < height + borderSize)
                    borderedMap[i, j] = map[i - borderSize, j - borderSize];
                else
                    borderedMap[i, j] = (int)Meshes.EMPTY;

        meshGenerator.GenerateMesh(borderedMap, 1, generateGround);
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that populates the cave with walls, entrance, and exits
    // ------------------------------
    void PopulateMap()
    {
        if (generateRandomSeed)
                seed = Time.time.ToString() + UnityEngine.Random.Range(0, 1000);

        System.Random randomNum = new System.Random(seed.GetHashCode());

        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                if ((i >= 0 && i <= 5 + borderSize) || (i >= width - 5 - borderSize && i <= width))
                {
                    // Always put walls on the sides of the cave
                    map[i, j] = (int)Meshes.WALL;
                }
                else if (i >= width / 2 - 5 && i <= width / 2 + 5 && j >= 0 && j <= 7 && generateEntrance)
                {
                    // Create entrance
                    map[i, j] = (int)Meshes.EMPTY;
                }
                else if (i == 0 || i == width - 1 || j == 0 || j == height - 1)
                {
                    // Always put walls around the cave
                    map[i, j] = (int)Meshes.WALL;
                }
                else
                {
                    // Obtain random number (either 1 [wall] or 0 [empty])
                    map[i, j] = (randomNum.Next(0, 100) < fillPercent) ? (int)Meshes.WALL : (int)Meshes.EMPTY;
                }
            }
        }
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that smoothes out the cave
    // ------------------------------
    void Smooth()
    {
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                int counter = GetNeighbours(i, j);

                if (counter > 4)
                    map[i, j] = (int)Meshes.WALL;
                else if (counter < 4)
                    map[i, j] = (int)Meshes.EMPTY;
            }
        }
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that gets how many neighboring tile that a certain tile has which are walls 
    // ------------------------------
    int GetNeighbours(int gridX, int gridY)
    {
        int counter = 0;

        for (int i = gridX - 1; i <= gridX + 1; ++i)
        {
            for (int j = gridY - 1; j <= gridY + 1; ++j)
            {
                if (IsInRange(i, j))
                {
                    // Check if this is the current tile
                    if (i != gridX || j != gridY)
                    {
                        counter += map[i, j];
                    }
                }
                else
                {
                    // Increase wall count if this is an edge 
                    ++counter;
                }
            }
        }

        return counter;
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that checks to see if an index is within range (width and height of the cave)
    // ------------------------------
    bool IsInRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    struct Coord
    {
        public int tileX, tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that eliminates certain regions of the cave
    // ------------------------------
    void ProcessMap()
    {
        List<Room> survivingRooms = new List<Room>();
        List<List<Coord>> wallRegions = GetRegions((int)Meshes.WALL);
        List<List<Coord>> roomRegions = GetRegions((int)Meshes.EMPTY);

        const int numOfWalls = 50;
        const int numOfEmpty = 50;

        foreach (List<Coord> wallRegion in wallRegions)
        {
            // Check if this region contains less than 50 walls
            if (wallRegion.Count < numOfWalls)
            {
                // If so, set this wall region to empty
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = (int)Meshes.EMPTY;
                }
            }
        }

        foreach (List<Coord> roomRegion in roomRegions)
        {
            // Check if this region contains less than 50 walls
            if (roomRegion.Count < numOfEmpty)
            {
                // If so, set this empty region to wall
                foreach (Coord tile in roomRegion)
                {
                    //map[tile.tileX, tile.tileY] = (int)Meshes.WALL; 
                }
            }
            else
            {
                // Otherwise, this region has survived the elmination process, add it to the list of surviving rooms
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }

        //survivingRooms.Sort();
        survivingRooms[0].isMainRoom = true;
        survivingRooms[0].isAccessibleFromMainRoom = true;

        ConnectClosestRooms(survivingRooms);
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that connects closest surviving rooms together
    // ------------------------------
    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibility = false)
    {
        // Track what the best distance between the rooms 
        int bestDistance = 0;

        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool connectionFound = false;

        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibility)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibility)
            {
                connectionFound = false;

                if (roomA.connectedRooms.Count > 0)
                    continue;
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                    continue;             

                for (int i = 0; i < roomA.edgeTiles.Count; ++i)
                {
                    for (int j = 0; j < roomB.edgeTiles.Count; ++j)
                    {
                        Coord tileA = roomA.edgeTiles[i];
                        Coord tileB = roomB.edgeTiles[j];
                        int dstBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        // Check if the distance between the rooms is less than the best distance (if a new best connection was found or we have not yet found a possible connection)
                        if (dstBetweenRooms < bestDistance || !connectionFound)
                        {
                            bestDistance = dstBetweenRooms;
                            connectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            // Check if a connection was found
            if (connectionFound && !forceAccessibility)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (connectionFound && forceAccessibility)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibility)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that creates a passage between two unconnected rooms 
    // ------------------------------
    void CreatePassage(Room unconnectedRoomA, Room unconnectedRoomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(unconnectedRoomA, unconnectedRoomB);

        List<Coord> line = TwoPointLine(tileA, tileB);

        foreach (Coord c in line)
            Circle(c, 5);
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that converts a coordinate into a world position
    // ------------------------------
    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + 0.5f + tile.tileX, 2, -height / 2 + 0.5f + tile.tileY);
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that returns a list of regions of a given type of tile 
    // ------------------------------
    List<List<Coord>> GetRegions(int _tyleType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                if (mapFlags[i, j] == (int)Meshes.EMPTY && map[i, j] == _tyleType)
                {
                    List<Coord> newRegion = GetRegionTiles(i, j);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = (int)Meshes.WALL;
                    }
                }
            }
        }

        return regions;
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: A floodfill search function that returns a list of coordinates
    // ------------------------------
    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = (int)Meshes.WALL;

        // Check if the queue is not empty
        while (queue.Count > 0)
        {
            // Retrieve the first item of the queue 
            Coord tile = queue.Dequeue();

            // Add the new tile
            tiles.Add(tile);

            // Iterate through adjacent tiles
            for (int i = tile.tileX - 1; i <= tile.tileX + 1; ++i)
            {
                for (int j = tile.tileY - 1; j <= tile.tileY + 1; ++j)
                {
                    if (IsInRange(i, j) && (j == tile.tileY || i == tile.tileX))
                    {
                        if (mapFlags[i, j] == 0 && map[i, j] == tileType)
                        {
                            mapFlags[i, j] = 1; // Tile has been processed
                            queue.Enqueue(new Coord(i, j)); // Add it to the queue
                        }
                    }
                }
            }
        }

        // Return list of coordinates
        return tiles;
    }

    // IComparable is the interface for comparable objects
    class Room : IComparable<Room>
    {
        public Room() { /* Do Nothing */ }

        // Coordinates of all points that the room cointains 
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;

        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();
            edgeTiles = new List<Coord>();

            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (x >= 1 && y >= 1)
                            {
                                if (map[x, y] == (int)Meshes.WALL)
                                {
                                    edgeTiles.Add(tile);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        // ------------------------------
        // Author: Rony Hanna
        // Description: Function that sets the accessibility of a room  
        // ------------------------------
        public void SetAccessibility()
        {
            // Check if the room is not already accessible from main room
            if (!isAccessibleFromMainRoom)
            {
                // Set accessibility to true 
                isAccessibleFromMainRoom = true;

                // Iterate through all the connected rooms 
                foreach (Room connectedRoom in connectedRooms)
                {
                    // Set connected rooms as accessible from main room
                    connectedRoom.SetAccessibility();
                }
            }
        }

        // ------------------------------
        // Author: Rony Hanna
        // Description: Function that connects two rooms
        // ------------------------------
        public static void ConnectRooms(Room A, Room B)
        {
            if (A.isAccessibleFromMainRoom)
            {
                B.SetAccessibility();
            }
            else if (B.isAccessibleFromMainRoom)
            {
                A.SetAccessibility();
            }

            A.connectedRooms.Add(B);
            B.connectedRooms.Add(A);
        }

        // ------------------------------
        // Author: Rony Hanna
        // Description: Function that checks if rooms are connected
        // ------------------------------
        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        // ------------------------------
        // Author: Rony Hanna
        // Description: Function used to determine how two objects should be sorted
        // ------------------------------
        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that helps create a path between the rooms using Bresenham's line algorithm
    // ------------------------------
    List<Coord> TwoPointLine(Coord start, Coord finish)
    {
        List<Coord> line = new List<Coord>();
        int x = 0, y = 0, dx = 0, dy = 0;
        int step = 0, gradientStep = 0, longest = 0, shortest = 0, gradientAccumulation = 0;
        bool bInverted = false;

        x = start.tileX;
        y = start.tileY;

        dx = finish.tileX - start.tileX;
        dy = finish.tileY - start.tileY;

        step = Math.Sign(dx);
        gradientStep = Math.Sign(dy);

        longest = Mathf.Abs(dx);
        shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);

            bInverted = true;
        }

        gradientAccumulation = longest / 2;

        for (int i = 0; i < longest; ++i)
        {
            line.Add(new Coord(x, y));

            if (bInverted)
                y += step;
            else
                x += step;
            
            gradientAccumulation += shortest;

            if (gradientAccumulation >= longest)
            {
                if (bInverted)
                    x += gradientStep;
                else
                    y += gradientStep;

                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that helps clear the map around the path points to ensure the path is clear
    // ------------------------------
    void Circle(Coord c, int r)
    {
        for (int i = -r; i <= r; ++i)
        {
            for (int j = -r; j <= r; ++j)
            {
                if (i * i + j * j <= r * r)
                {
                    int drawX = c.tileX + i;
                    int drawY = c.tileY + j;

                    if (IsInRange(drawX, drawY))
                        map[drawX, drawY] = 0;
                }
            }
        }
    }
}