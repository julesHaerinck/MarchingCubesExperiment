using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public struct CubeData
{
    public PointData[] cubePoints;
}

public struct PointData
{
    public Vector3 position;
    public float   IsoLevel;
}

public struct TriangleData
{
    public PointData[] vertices;
}

//[ExecuteInEditMode]
public class MarchingCubes : MonoBehaviour
{
    public Vector3 BoundingBox = Vector3.one;
    public MeshFilter meshFilter;
    public float IsoSurfaceLimit = 1f;
    [Range(0.2f, 5f)]
    public float Resolution = 1f;

    //public float GizmosSphereSize = .5f;
    public float SphereSize = .5f;

    private List<CubeData> CubeList = new List<CubeData>();
    private List<TriangleData> TriangleList = new List<TriangleData>();

    private Vector3Int steps;
    private PointData[,,] points;
    private Mesh NewMesh;

    // Start is called before the first frame update
    void Start()
    {
        NewMesh = new Mesh();
        
        steps = new Vector3Int(
                (int)(BoundingBox.x / Resolution),
                (int)(BoundingBox.y / Resolution),
                (int)(BoundingBox.z / Resolution)
            );
        points = new PointData[steps.x+2, steps.y+2, steps.z+2];

        // x
        for(int i = 0; i <= steps.x + 1; i++)
        {
            // y
            for(int j = 0; j <= steps.y + 1; j++)
            {
                // z
                for(int k = 0; k <= steps.z + 1; k++)
                {
                    Vector3 pointPosition = new Vector3(-BoundingBox.x/2 + i* Resolution, -BoundingBox.y/2 + j * Resolution, -BoundingBox.z/2 + k * Resolution);
                    points[i, j, k] = new PointData();
                    points[i, j, k].position = pointPosition;
                    points[i, j, k].IsoLevel = pointPosition.y;
                }
            }
        }

        //Debug.Log(points.Length);
        CreateCubes();

        foreach(CubeData cube in CubeList)
            CalculateCube(cube);
        //Debug.Log(CubeList.Count);
    }

    // Update is called once per frame
    void Update()
    {


        //foreach(CubeData cube in VoxelList)
        //{
        //    DrawCube(cube);
        //}

        //Vector3 steps = BoundingBox / Resolution;
        //Debug.Log($"{(int)steps.x}");
    }

    // Visualize the points present in the boinding box
    private void OnDrawGizmos()
    {
        Vector3 steps = BoundingBox / Resolution;

        // x
        for(int i = 0; i <= (int)steps.x + 1; i++)
        {
            // y
            for(int j = 0; j <= (int)steps.y + 1; j++)
            {
                // z
                for(int k = 0; k <= (int)steps.z + 1; k++)
                {


                    Vector3 pointPosition = new Vector3(-BoundingBox.x/2 + i* Resolution, -BoundingBox.y/2 + j * Resolution, -BoundingBox.z/2 + k * Resolution);
                    Gizmos.color = new Color(pointPosition.y, 0, 0, 1);
                    Gizmos.DrawSphere(pointPosition, SphereSize);
                }
            }
        }
    }

    private void DrawCubes()
    {
        // TODO 
        // creat a new mesh with the calculated triangles
        //NewMesh.triangles = TriangleList.ToArray();
        //NewMesh.vert
    }

    private void CalculateCube(CubeData cube)
    {
        PointData[] vertList = new PointData[12];
        uint cubeIndex = 0;

        if(cube.cubePoints[0].IsoLevel < IsoSurfaceLimit) cubeIndex |= 1;
        if(cube.cubePoints[1].IsoLevel < IsoSurfaceLimit) cubeIndex |= 2;
        if(cube.cubePoints[2].IsoLevel < IsoSurfaceLimit) cubeIndex |= 4;
        if(cube.cubePoints[3].IsoLevel < IsoSurfaceLimit) cubeIndex |= 8;
        if(cube.cubePoints[4].IsoLevel < IsoSurfaceLimit) cubeIndex |= 16;
        if(cube.cubePoints[5].IsoLevel < IsoSurfaceLimit) cubeIndex |= 32;
        if(cube.cubePoints[6].IsoLevel < IsoSurfaceLimit) cubeIndex |= 64;
        if(cube.cubePoints[7].IsoLevel < IsoSurfaceLimit) cubeIndex |= 128;

        if(marchingCubesLookupTable.edgeTable[cubeIndex] == 0)
            return;

        //Debug.Log(marchingCubesLookupTable.edgeTable[cubeIndex] & 4);

        if((marchingCubesLookupTable.edgeTable[cubeIndex] & 1) != 0)
            vertList[0] = InterpolateTwoPoints(cube.cubePoints[0], cube.cubePoints[1]);
        if((marchingCubesLookupTable.edgeTable[cubeIndex] & 2) != 0)
            vertList[1] = InterpolateTwoPoints(cube.cubePoints[1], cube.cubePoints[2]);
        if((marchingCubesLookupTable.edgeTable[cubeIndex] & 4) != 0)
            vertList[2] = InterpolateTwoPoints(cube.cubePoints[2], cube.cubePoints[3]);
        if((marchingCubesLookupTable.edgeTable[cubeIndex] & 8) != 0)
            vertList[3] = InterpolateTwoPoints(cube.cubePoints[3], cube.cubePoints[0]);
        if((marchingCubesLookupTable.edgeTable[cubeIndex] & 16) != 0)
            vertList[4] = InterpolateTwoPoints(cube.cubePoints[4], cube.cubePoints[5]);
        if((marchingCubesLookupTable.edgeTable[cubeIndex] & 32) != 0)
            vertList[5] = InterpolateTwoPoints(cube.cubePoints[5], cube.cubePoints[6]);
        if((marchingCubesLookupTable.edgeTable[cubeIndex] & 64) != 0)
            vertList[6] = InterpolateTwoPoints(cube.cubePoints[6], cube.cubePoints[7]);
        if((marchingCubesLookupTable.edgeTable[cubeIndex] & 128) != 0)
            vertList[7] = InterpolateTwoPoints(cube.cubePoints[7], cube.cubePoints[4]);
        if((marchingCubesLookupTable.edgeTable[cubeIndex] & 256) != 0)
            vertList[8] = InterpolateTwoPoints(cube.cubePoints[0], cube.cubePoints[4]);
        if((marchingCubesLookupTable.edgeTable[cubeIndex] & 512) != 0)
            vertList[9] = InterpolateTwoPoints(cube.cubePoints[1], cube.cubePoints[5]);
        if((marchingCubesLookupTable.edgeTable[cubeIndex] & 1024) != 0)
            vertList[10] = InterpolateTwoPoints(cube.cubePoints[2], cube.cubePoints[6]);
        if((marchingCubesLookupTable.edgeTable[cubeIndex] & 2048) != 0)
            vertList[11] = InterpolateTwoPoints(cube.cubePoints[3], cube.cubePoints[7]);

        int ntriang = 0;

        for(int i = 0; marchingCubesLookupTable.triTable[cubeIndex,i] != -1 ;i += 3)
        {
            TriangleData tri = new TriangleData();
            tri.vertices = new PointData[3]
            {
                vertList[marchingCubesLookupTable.triTable[cubeIndex, i]],
                vertList[marchingCubesLookupTable.triTable[cubeIndex, i+1]],
                vertList[marchingCubesLookupTable.triTable[cubeIndex, i+2]],
            };
            ntriang++;
        }
    }

    private void CreateCubes()
    {
        for(int x = 0; x <= steps.x; x++)
        {
            for(int y = 0; y <= steps.y; y++)
            {
                for(int z = 0; z <= steps.z; z++)
                {
                    CubeData cube = new CubeData();
                    cube.cubePoints = new PointData[8] {
                        points[x  , y  , z],
                        points[x+1, y  , z],
                        points[x+1, y+1, z],
                        points[x  , y+1, z],

                        points[x  , y  , z+1],
                        points[x+1, y  , z+1],
                        points[x+1, y+1, z+1],
                        points[x  , y+1, z+1]
                    };
                    CubeList.Add(cube);
                }
            }
        }
    }

    private PointData InterpolateTwoPoints(PointData p1, PointData p2)
    {
        
        if(Compare(p1, p2))
        {
            PointData temp;
            temp = p1;
            p1 = p2;
            p2 = temp;
        }

        PointData p = new PointData();

        if(Mathf.Abs(p1.IsoLevel - p2.IsoLevel) > 0.00001)
            p.position = p1.position + (p2.position - p1.position) / (p2.IsoLevel - p1.IsoLevel)*(IsoSurfaceLimit - p1.IsoLevel);
        else
            p = p1;

        return p;
    }


    private bool Compare(PointData p1, PointData p2)
    {
        if(p1.position.x < p2.position.x)
            return true;
        else if(p1.position.x > p2.position.x)
            return false;

        if(p1.position.y < p2.position.y)
            return true;
        else if(p1.position.y > p2.position.y)
            return false;

        if(p1.position.z < p2.position.z)
            return true;
        else if(p1.position.z > p2.position.z)
            return false;


        return false;
    }
}



// Bits of code that might be usefulls later
// Will delete at some points
/*
 *         if (x < right.x)
            return true;
        else if (x > right.x)
            return false;

        if (y < right.y)
            return true;
        else if (y > right.y)
            return false;

        if (z < right.z)
            return true;
        else if (z > right.z)
            return false;

        return false;
 *                     cube.VerticePosition = new Vector3[8]
                    {
                        new Vector3(i  , j  , k  ),
                        new Vector3(i+1, j  , k  ),
                        new Vector3(i+1, j+1, k  ),
                        new Vector3(i  , j+1, k  ),

                        new Vector3(i  , j  , k+1),
                        new Vector3(i+1, j  , k+1),
                        new Vector3(i+1, j+1, k+1),
                        new Vector3(i  , j+1, k+1)
                    };

                    cube.IsosurfaceValue = new float[8] { 
                        
                    };
 */