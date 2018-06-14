using UnityEngine;
using System.Collections.Generic;

public class CaveMeshGeneratorScript : MonoBehaviour
{
    public SquareGrid squareGrid;
    public MeshFilter walls, cave, ground;
    float timerTest;

    List<Vector3> vertices;
    List<int> triangles;

    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();

    [Range(1, 100)] public int texTiling;
    int[,] mapCopy;

    // ------------------------------
    // Author: Rony Hanna
    // Description: Starter function that initializes certain variables
    // ------------------------------
    void Start()
    {
        texTiling = 10;
        timerTest = 0.0f;
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that generates the mesh of the cave
    // ------------------------------
    public void GenerateMesh(int[,] map, float squareSize, bool createGround = false)
    {
        mapCopy = map;

        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        squareGrid = new SquareGrid(map, squareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int i = 0; i < squareGrid.squares.GetLength(0); ++i)
            for (int j = 0; j < squareGrid.squares.GetLength(1); ++j)
                TriangulateSquare(squareGrid.squares[i, j]);

        Mesh mesh = new Mesh();
        cave.mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        // Generate UVs
        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; ++i)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].x) * texTiling;
            float percentY = Mathf.InverseLerp(-map.GetLength(1) / 2 * squareSize, map.GetLength(1) / 2 * squareSize, vertices[i].z) * texTiling;
            uvs[i] = new Vector2(percentX, percentY);
        }

        mesh.uv = uvs;

        CreateWall();

        if (createGround)
            CreateGround(map, 1);
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that creates the wall mesh of the cave and generates UV coordinates for the walls
    // ------------------------------
    void CreateWall()
    {
        // Delete old wall colliders from previously generated cave
        MeshCollider currentCollider = GetComponent<MeshCollider>();
        DestroyImmediate(currentCollider);

        CalculateMeshOutlines();

        List<Vector3> vert = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();
        float wallHeight = 5;

        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = vert.Count;

                vert.Add(vertices[outline[i]]);
                vert.Add(vertices[outline[i + 1]]);
                vert.Add(vertices[outline[i]] - Vector3.up * wallHeight);
                vert.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight);  

                wallTriangles.Add(startIndex + 0);  wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);  wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);  wallTriangles.Add(startIndex + 0);
            }
        }

        wallMesh.vertices = vert.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        walls.mesh = wallMesh;

        // Generate UV coordinates for the walls so that texture may be applied 
        Vector2[] uvs = new Vector2[vert.Count];
        int c = 0;

        for (int i = 0; i < vert.Count; ++i)
        {
            switch (c)
            {
                case 0:
                    uvs[i] = new Vector2(0, 3.0f);
                    c++;
                    break;

                case 1:
                    uvs[i] = new Vector2(0.5f, 3.0f);
                    c++;
                    break;

                case 2:
                    uvs[i] = new Vector2(0, 0);
                    c++;
                    break;

                case 3:
                    uvs[i] = new Vector2(0.5f, 0);
                    c++;
                    break;

                case 4:
                    uvs[i] = new Vector2(0.5f, 3.0f);
                    c++;
                    break;

                case 5:
                    uvs[i] = new Vector2(1, 3.0f);
                    c++;
                    break;

                case 6:
                    uvs[i] = new Vector2(0.5f, 0);
                    c++;
                    break;

                case 7:
                    uvs[i] = new Vector2(1, 0);
                    c = 0;
                    break;
            }
        }

        wallMesh.uv = uvs;

        MeshCollider wallCollider = gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
    }

    public void CreateGround(int[,] map, float squareSize)
    {
        triangleDictionary.Clear();
        checkedVertices.Clear();

        squareGrid = new SquareGrid(map, squareSize, -5);

        vertices.Clear();
        triangles.Clear();

        for (int i = 0; i < squareGrid.squares.GetLength(0); ++i)
            for (int j = 0; j < squareGrid.squares.GetLength(1); ++j)
                TriangulateSquare(squareGrid.squares[i, j]);

        Mesh mesh = new Mesh();
        ground.mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        MeshCollider groundCollider = ground.gameObject.GetComponent<MeshCollider>();

        if (!groundCollider)
            groundCollider = ground.gameObject.AddComponent<MeshCollider>();

        groundCollider.sharedMesh = mesh;

        Vector2[] uvs = new Vector2[vertices.Count];

        for (int i = 0; i < vertices.Count; ++i)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].x) * texTiling;
            float percentY = Mathf.InverseLerp(-map.GetLength(1) / 2 * squareSize, map.GetLength(1) / 2 * squareSize, vertices[i].z) * texTiling;
            uvs[i] = new Vector2(percentX, percentY);
        }

        mesh.uv = uvs;
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that triangulates a square
    // ------------------------------
    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0: break; // If no point is selected then we do not have a mesh

            // 1 point selected:
            case 1: MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft); break;
            case 2: MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight); break;
            case 4: MeshFromPoints(square.topRight, square.centreRight, square.centreTop); break;
            case 8: MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft); break;

            // 2 points selected:
            case 3: MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft); break;
            case 6: MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom); break;
            case 9: MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft); break;
            case 12: MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft); break;
            case 5: MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft); break;
            case 10: MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft); break;

            // 3 points selected:
            case 7: MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft); break;
            case 11: MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft); break;
            case 13: MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft); break;
            case 14: MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft); break;

            // 4 points selected:
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;
        }
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that creates triangles based on the number of points
    // ------------------------------
    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
            CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5)
            CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5]);
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that takes an array of points and assign each point's vertex index
    // ------------------------------
    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; ++i)
        {
            // Check if the point has got a vertex index (default = -1)
            if (points[i].vertexIndex == -1)
            {
                // Set point's vertex index equal to the amount of items in vertices 
                points[i].vertexIndex = vertices.Count;

                // Add new vertex to vertices list
                vertices.Add(points[i].position);
            }
        }
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that creates a triangle from the assigned points
    // ------------------------------
    void CreateTriangle(Node a, Node b, Node c)
    {
        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);

        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        // Add triangle to dictionary
        RegisterTriangle(triangle.A, triangle);
        RegisterTriangle(triangle.B, triangle);
        RegisterTriangle(triangle.C, triangle);
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that registers a newly created triangle to a dictionary of triangles
    // ------------------------------
    void RegisterTriangle(int vertexID, Triangle triangle)
    {
        // Check if the triangle dictionary contains this vertex index  
        if (triangleDictionary.ContainsKey(vertexID))
        {
            // Add new triangle 
            triangleDictionary[vertexID].Add(triangle);
        }
        else
        {
            // Create new list of triangles and add triangle to the dictionary of triangles along with its vertex index 
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(vertexID, triangleList);
        }
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that iterates through vertices, checks if its an outline, follows the outline, and add to the outline list
    // ------------------------------
    void CalculateMeshOutlines()
    {
        // Loop through the vertices
        for (int i = 0; i < vertices.Count; ++i)
        {
            if (!checkedVertices.Contains(i))
            {
                int newVertex = GetConnectedOutlineVertex(i);

                // Check if the new vertex exist
                if (newVertex != -1)
                {
                    // Add the vertex
                    checkedVertices.Add(i);

                    List<int> newVertexOutline = new List<int>();
                    newVertexOutline.Add(i);
                    outlines.Add(newVertexOutline);

                    FollowOutline(newVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(i);
                }
            }
        }
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that follows the entire outline
    // ------------------------------
    void FollowOutline(int vIndex, int oIndex)
    {
        // Add vertex index to list of outlines
        outlines[oIndex].Add(vIndex);
        checkedVertices.Add(vIndex);

        int nextVertex = GetConnectedOutlineVertex(vIndex);

        if (nextVertex != -1)
        {
            FollowOutline(nextVertex, oIndex);
        }
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that gets a list of all triangles containing a vertex index
    // ------------------------------
    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> triangles = triangleDictionary[vertexIndex];

        for (int i = 0; i < triangles.Count; ++i)
        {
            Triangle triangle = triangles[i];

            for (int j = 0; j < 3; ++j)
            {
                int vertexB = triangle[j];

                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that checks if the edge formed by two vertices is an outline edge
    // ------------------------------
    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; ++i)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                ++sharedTriangleCount;

                if (sharedTriangleCount > 1)
                    break;
            }
        }

        return sharedTriangleCount == 1;
    }

    struct Triangle
    {
        public int A;
        public int B;
        public int C;
        int[] verts;

        public Triangle(int a, int b, int c)
        {
            verts = new int[3];
            verts[0] = a;
            verts[1] = b;
            verts[2] = c;

            A = a;
            B = b;
            C = c;
        }

        // ------------------------------
        // Author: Rony Hanna
        // Description: Function that gets the vertices of a triangle
        // ------------------------------
        public int this[int i] { get { return verts[i]; } }

        // ------------------------------
        // Author: Rony Hanna
        // Description: Function used to determine if a triangle contains a certain index
        // ------------------------------
        public bool Contains(int vertexIndex) { return vertexIndex == A || vertexIndex == B || vertexIndex == C; }
    }

    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int i = 0; i < nodeCountX; ++i)
            {
                for (int j = 0; j < nodeCountY; ++j)
                {
                    Vector3 pos = new Vector3(-mapWidth / 2 + i * squareSize + squareSize / 2, 0, -mapHeight / 2 + j * squareSize + squareSize / 2);
                    controlNodes[i, j] = new ControlNode(pos, map[i, j] == 1, squareSize);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];

            for (int i = 0; i < nodeCountX - 1; ++i)
            {
                for (int j = 0; j < nodeCountY - 1; ++j)
                {
                    squares[i, j] = new Square(controlNodes[i, j + 1], controlNodes[i + 1, j + 1], controlNodes[i + 1, j], controlNodes[i, j]);
                }
            }
        }

        public SquareGrid(int[,] map, float squareSize, float wallHeight)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int i = 0; i < nodeCountX; ++i)
            {
                for (int j = 0; j < nodeCountY; ++j)
                {
                    Vector3 pos = new Vector3(-mapWidth / 2 + i * squareSize + squareSize / 2, wallHeight, -mapHeight / 2 + j * squareSize + squareSize / 2);  
                    controlNodes[i, j] = new ControlNode(pos, map[i, j] == 0, squareSize);  
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];

            for (int i= 0; i < nodeCountX - 1; ++i)
            {
                for (int j = 0; j < nodeCountY - 1; ++j)
                {
                    squares[i, j] = new Square(controlNodes[i, j + 1], controlNodes[i + 1, j + 1], controlNodes[i + 1, j], controlNodes[i, j]);
                }
            }
        }
    }

    public class Square
    {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;

            if (topLeft.active)
                configuration += 8;

            if (topRight.active)
                configuration += 4;

            if (bottomRight.active)
                configuration += 2;

            if (bottomLeft.active)
                configuration += 1;
        }
    }

    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos)
        {
            position = _pos;
        }
    }

    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 _pos, bool _active, float squareSize) : 
            base(_pos)
        {
            active = _active;
            above = new Node(position + Vector3.forward * squareSize / 2.0f);
            right = new Node(position + Vector3.right * squareSize / 2.0f);
        }
    }
}