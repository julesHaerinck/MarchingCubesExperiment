using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TreeEditor;
using UnityEngine;
using UnityEngine.UIElements;

/*
public struct CubeData
{
    public PointData[] cubePoints;
}

public struct PointData
{
    public Vector3 position;
    public float   IsoLevel;
}
*/

//[ExecuteInEditMode]
namespace MarchingCube
{
    public class MarchingCubesGeneration : MonoBehaviour
    {
        /*************
        Public Fields
        **************/
        [Header("Awake")]
        public Vector3      BoundingBox = Vector3.one;
        public MeshFilter   MainMeshFilter;
        public MeshCollider MainMeshCollider;
        public float SphereSize = .5f;
        public float NoiseMultiplier = 1f;
        public bool  AddSpheres = false;

        [Range(0.2f, 5f)]
        public float      Resolution = 1f;
        public GameObject SphereParent;

        [Header("Update")]
        [Range(0.001f, 1f)]
        public float IsoSurfaceLimit = 1f;
        [Range(-10f, 10f)]
        public float ValueSlider = 0f;


        /***************
         Private Fields
         ***************/
        private List<Cube>     _cubeList     = new List<Cube>();
        private List<int>      _triangleList = new List<int>();
        private List<Vector3>  _verticesList = new List<Vector3>();
        private Renderer[,,]   _sphereVisualizer;
        private Vector3Int     _steps;
        private Point[,,]      _pointList;
        private Mesh           _newMesh;
        
        // Start is called before the first frame update
        void Awake()
        {
            _newMesh = new Mesh();

            _steps = new Vector3Int(
                (int)(BoundingBox.x / Resolution),
                (int)(BoundingBox.y / Resolution),
                (int)(BoundingBox.z / Resolution)
                );

            _pointList = new Point[_steps.x + 1, _steps.y + 1, _steps.z + 1];
            _sphereVisualizer = new Renderer[_steps.x + 1, _steps.y + 1, _steps.z + 1];

            //Debug.Log($"X : {steps.x}, Y : {steps.y}, Z : {steps.z}");
            for(int y = 0; y <= _steps.y; y++)
            {
                for(int z = 0; z <= _steps.z; z++)
                {
                    for(int x = 0; x <= _steps.x; x++)
                    {
                        Vector3 pointPosition = new Vector3(
                            -BoundingBox.x/2 + x * Resolution, 
                            -BoundingBox.y/2 + y * Resolution,
                            -BoundingBox.z/2 + z * Resolution
                            );                        

                        float noiseSample = -pointPosition.y;

                        //Debug.Log($"X : {x}, Y : {y}, Z : {z}");
                        Point point = new Point(noiseSample, pointPosition);
                        _pointList[x,y,z] = point;
                        
                        if(AddSpheres)
                        {
                            GameObject PointView = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            PointView.name = pointPosition.ToString();
                            PointView.transform.position = pointPosition;
                            PointView.transform.localScale = new Vector3(SphereSize, SphereSize, SphereSize);
                            PointView.GetComponent<Renderer>().material.SetColor("_Color", new Color(noiseSample, noiseSample, noiseSample));
                            PointView.transform.parent = SphereParent.transform;
                            _sphereVisualizer[x, y, z] = PointView.GetComponent<Renderer>();
                        }
                    }
                }
            }

            CreateCubes();
            
            foreach(Cube cube in _cubeList)
            {
                CalculateCube(cube);
            }
            
            DrawCubes();
        }

        // Update is called once per frame
        void Update()
        {
            // 
            foreach(Cube cube in _cubeList)
            {
                CalculateCube(cube);
            }

            DrawCubes();
        }

        /// <summary>
        /// Creates the mesh based on the cubes calculations
        /// </summary>
        private void DrawCubes()
        {
            MainMeshFilter.mesh.Clear();
            //Debug.Log(VerticeList.Count);
            _newMesh.vertices = _verticesList.ToArray();
            _newMesh.triangles = _triangleList.ToArray();
            MainMeshFilter.mesh = _newMesh;
            MainMeshFilter.mesh.RecalculateNormals();
            MainMeshCollider.sharedMesh = _newMesh;
            //Debug.Log(MainMeshFilter.mesh.normals[0]);
            //NewMesh.triangles = TriangleList.ToArray();
            //NewMesh.vert
            _verticesList.Clear();
            _triangleList.Clear();
        }

        // Inefficient mesh generation.
        // The vertices and triangles are added indiscriminately instead of ignoring the points already 
        // in the vertice list, resulting in meshes with 1700 vertices instead of 300
        /// <summary>
        /// Calculates and looks up the appropriate arragment of vertices and triangles based on the cubes 
        /// points values and appends to a list of vertices and triangles for later use to build the mesh
        /// </summary>
        /// <param name="cube">The cube to calculate</param>
        private void CalculateCube(Cube cube, int passes = -1)
        {
            Point[] vertPointList = new Point[12];
            uint cubeIndex = 0;
            
            if(GetPointFromIndex(cube.GetValueAtRow(0)).IsosurfaceValue < IsoSurfaceLimit)
                cubeIndex |= 1;
            if(GetPointFromIndex(cube.GetValueAtRow(1)).IsosurfaceValue < IsoSurfaceLimit)
                cubeIndex |= 2;
            if(GetPointFromIndex(cube.GetValueAtRow(2)).IsosurfaceValue < IsoSurfaceLimit)
                cubeIndex |= 4;
            if(GetPointFromIndex(cube.GetValueAtRow(3)).IsosurfaceValue < IsoSurfaceLimit)
                cubeIndex |= 8;
            if(GetPointFromIndex(cube.GetValueAtRow(4)).IsosurfaceValue < IsoSurfaceLimit)
                cubeIndex |= 16;
            if(GetPointFromIndex(cube.GetValueAtRow(5)).IsosurfaceValue < IsoSurfaceLimit)
                cubeIndex |= 32;
            if(GetPointFromIndex(cube.GetValueAtRow(6)).IsosurfaceValue < IsoSurfaceLimit)
                cubeIndex |= 64;
            if(GetPointFromIndex(cube.GetValueAtRow(7)).IsosurfaceValue < IsoSurfaceLimit)
                cubeIndex |= 128;
            
            if(marchingCubesLookupTable.edgeTable[cubeIndex] == 0)
                return;
            
            //Debug.Log($"Cube passed at pass nb : {passes}");
            
            if((marchingCubesLookupTable.edgeTable[cubeIndex] & 1) != 0)
                vertPointList[0]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(0)), GetPointFromIndex(cube.GetValueAtRow(1)));
            if((marchingCubesLookupTable.edgeTable[cubeIndex] & 2) != 0)
                vertPointList[1]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(1)), GetPointFromIndex(cube.GetValueAtRow(2)));
            if((marchingCubesLookupTable.edgeTable[cubeIndex] & 4) != 0)
                vertPointList[2]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(2)), GetPointFromIndex(cube.GetValueAtRow(3)));
            if((marchingCubesLookupTable.edgeTable[cubeIndex] & 8) != 0)
                vertPointList[3]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(3)), GetPointFromIndex(cube.GetValueAtRow(0)));
            if((marchingCubesLookupTable.edgeTable[cubeIndex] & 16) != 0)
                vertPointList[4]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(4)), GetPointFromIndex(cube.GetValueAtRow(5)));
            if((marchingCubesLookupTable.edgeTable[cubeIndex] & 32) != 0)
                vertPointList[5]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(5)), GetPointFromIndex(cube.GetValueAtRow(6)));
            if((marchingCubesLookupTable.edgeTable[cubeIndex] & 64) != 0)
                vertPointList[6]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(6)), GetPointFromIndex(cube.GetValueAtRow(7)));
            if((marchingCubesLookupTable.edgeTable[cubeIndex] & 128) != 0)
                vertPointList[7]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(7)), GetPointFromIndex(cube.GetValueAtRow(4)));
            if((marchingCubesLookupTable.edgeTable[cubeIndex] & 256) != 0)
                vertPointList[8]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(0)), GetPointFromIndex(cube.GetValueAtRow(4)));
            if((marchingCubesLookupTable.edgeTable[cubeIndex] & 512) != 0)
                vertPointList[9]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(1)), GetPointFromIndex(cube.GetValueAtRow(5)));
            if((marchingCubesLookupTable.edgeTable[cubeIndex] & 1024) != 0)
                vertPointList[10] = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(2)), GetPointFromIndex(cube.GetValueAtRow(6)));
            if((marchingCubesLookupTable.edgeTable[cubeIndex] & 2048) != 0)
                vertPointList[11] = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(3)), GetPointFromIndex(cube.GetValueAtRow(7)));
            
            int ntriang = 0;
            
            for(int i = 0; marchingCubesLookupTable.triTable[cubeIndex, i] != -1; i += 3)
            {
                _verticesList.Add(vertPointList[marchingCubesLookupTable.triTable[cubeIndex, i    ]].Position);
                _verticesList.Add(vertPointList[marchingCubesLookupTable.triTable[cubeIndex, i + 1]].Position);
                _verticesList.Add(vertPointList[marchingCubesLookupTable.triTable[cubeIndex, i + 2]].Position);
                
                _triangleList.Add(_verticesList.Count - 3);
                _triangleList.Add(_verticesList.Count - 2);
                _triangleList.Add(_verticesList.Count - 1);
                ntriang++;
                //Debug.Log(tri.vertices[1].position);
                //Debug.Log(marchingCubesLookupTable.triTable[cubeIndex, i]);
            }
        }

        /// <summary>
        /// Creates the list of cubes to later execute the algorithm
        /// </summary>
        private void CreateCubes()
        {
            for(int y = 0; y < _steps.y; y++)
            {
                for(int z = 0; z < _steps.z; z++)
                {
                    for(int x = 0; x < _steps.x; x++)
                    {
                        int[,] indexPoints = new int[8,3]
                            {
                                {x  ,y  ,z  },
                                {x+1,y  ,z  },
                                {x+1,y+1,z  },
                                {x  ,y+1,z  },
                                     
                                {x  ,y  ,z+1},
                                {x+1,y  ,z+1},
                                {x+1,y+1,z+1},
                                {x  ,y+1,z+1}
                            };

                        Cube cube = new Cube(indexPoints);
                        _cubeList.Add(cube);
                    }
                }
            }
           
        }

        private void UpdatePointsIsoValues()
        {
            //int index = 0;
            for(int y = 0; y <= _steps.y; y++)
            {
                // y
                for(int z = 0; z <= _steps.z; z++)
                {
                    // z
                    for(int x = 0; x <= _steps.x; x++)
                    {
                        float value = -1 ;
                        _pointList[x, y, z].IsosurfaceValue = -value;
                        if(AddSpheres)
                            _sphereVisualizer[x, y, z].material.color = new Color(value, value, value);
                        //index++
                        //Debug.Log(points[i, j, k].IsoLevel);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the middle point between the two points given
        /// </summary>
        /// <param name="p1">PointData 1</param>
        /// <param name="p2">PointData 2</param>
        /// <returns>New PointData</returns>
        private Point InterpolateTwoPoints(Point p1, Point p2)
        {
            if(Compare(p1, p2))
            {
                Point temp;
                temp = p1;
                p1 = p2;
                p2 = temp;
            }

            Point p = new Point();
        
            if(Mathf.Abs(p1.IsosurfaceValue - p2.IsosurfaceValue) > 0.00001)
                p.Position = p1.Position + (p2.Position - p1.Position) / (p2.IsosurfaceValue - p1.IsosurfaceValue) * (IsoSurfaceLimit - p1.IsosurfaceValue);
            else
                p = p1;
        
            return p;
        }

        /// <summary>
        /// Compares two points. Similar to the < operator
        /// </summary>
        /// <param name="p1">PointData one</param>
        /// <param name="p2">PointData two</param>
        /// <returns>true if p2 is greater than p1</returns>
        private bool Compare(Point p1, Point p2)
        {
            if(p1.Position.x < p2.Position.x)
                return true;
            else if(p1.Position.x > p2.Position.x)
                return false;
        
            if(p1.Position.y < p2.Position.y)
                return true;
            else if(p1.Position.y > p2.Position.y)
                return false;
        
            if(p1.Position.z < p2.Position.z)
                return true;
            else if(p1.Position.z > p2.Position.z)
                return false;
        
        
            return false;
        }

        /// <summary>
        /// Helper function. Draws the cube given in parameter
        /// </summary>
        /// <param name="cube">The cube to draw in Unity scene window</param>
        private void DebugDrawCube(Cube cube)
        {
            Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(0)).Position, GetPointFromIndex(cube.GetValueAtRow(1)).Position, new Color(1, 0, 0, 1), 10f);
            Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(1)).Position, GetPointFromIndex(cube.GetValueAtRow(2)).Position, new Color(1, 0, 0, 1), 10f);
            Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(2)).Position, GetPointFromIndex(cube.GetValueAtRow(3)).Position, new Color(1, 0, 0, 1), 10f);
            Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(3)).Position, GetPointFromIndex(cube.GetValueAtRow(0)).Position, new Color(1, 0, 0, 1), 10f);
           
            Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(4)).Position, GetPointFromIndex(cube.GetValueAtRow(5)).Position, new Color(1, 0, 0, 1), 10f);
            Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(5)).Position, GetPointFromIndex(cube.GetValueAtRow(6)).Position, new Color(1, 0, 0, 1), 10f);
            Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(6)).Position, GetPointFromIndex(cube.GetValueAtRow(7)).Position, new Color(1, 0, 0, 1), 10f);
            Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(7)).Position, GetPointFromIndex(cube.GetValueAtRow(4)).Position, new Color(1, 0, 0, 1), 10f);
           
            Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(0)).Position, GetPointFromIndex(cube.GetValueAtRow(4)).Position, new Color(1, 0, 0, 1), 10f);
            Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(1)).Position, GetPointFromIndex(cube.GetValueAtRow(5)).Position, new Color(1, 0, 0, 1), 10f);
            Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(2)).Position, GetPointFromIndex(cube.GetValueAtRow(6)).Position, new Color(1, 0, 0, 1), 10f);
            Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(3)).Position, GetPointFromIndex(cube.GetValueAtRow(7)).Position, new Color(1, 0, 0, 1), 10f);
        }
        private Point GetPointFromIndex(int[] index)
        {
            return _pointList[index[0], index[1], index[2]];
        }

        public void UpdateIsoValuesFromCamera(Vector3 hit, float sphereRadius, float strenght)
        {
            
            for(int y = 0; y <= _steps.y; y++)
            {
                for(int z = 0; z <= _steps.z; z++)
                {
                    for(int x = 0; x <= _steps.x; x++)
                    {
                        Vector3 pointPosition = _pointList[x, y, z].Position;

                        if(CalculateIfPointIsInSphere(hit, pointPosition, sphereRadius))
                            _pointList[x, y, z].IsosurfaceValue += strenght;

                        if(AddSpheres)
                        {
                            float value = _pointList[x, y, z].IsosurfaceValue;
                            _sphereVisualizer[x, y, z].material.color = new Color(value, value, value);
                        }
                    }
                }
            }
        }

        private bool CalculateIfPointIsInSphere(Vector3 sphereOrigin, Vector3 point, float sphereRadius)
        {
            float x2 = (point.x - sphereOrigin.x) * (point.x - sphereOrigin.x);
            float y2 = (point.y - sphereOrigin.y) * (point.y - sphereOrigin.y);
            float z2 = (point.z - sphereOrigin.z) * (point.z - sphereOrigin.z);

            if((x2 + y2 + z2) < (sphereRadius * sphereRadius))
                return true;
            else
                return false;
        }
    }
}