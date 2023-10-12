using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Marching cubes Algorithm take from the website 
// https://paulbourke.net/geometry/polygonise/

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
		public float SphereSize      = .5f;
		public float NoiseMultiplier = 1f;
		public bool  AddSpheres      = false;

		[Range(0.2f, 5f)]
		public float      Resolution = 1f;
		public GameObject SphereParent;

		[Header("Update")]
		[Range(-10f, 10f)]
		public float IsoSurfaceLimit = 1f;
		[Range(-10f, 10f)]
		public float ValueSlider     = 0f;        
		[Range(-10f, 10f)]
		public float ValueSlider2    = 0f;


		/***************
		 Private Fields
		 ***************/
		// TODO 12/10/23
		// Since there are a lot of arrays, maybe check if there is an impact on memory usage

		private List<Cube>    _cubeList      = new List<Cube>();    // all the cubes in the defined space
		private List<int>     _cubeIndexList = new List<int>();     // only the cubes that need to be used to create the mesh
		private List<int>     _triangleList  = new List<int>();     // list of index corresponding to the vertice used by triangle
		private List<Vector3> _verticesList  = new List<Vector3>(); // list of vertices used by the mesh
		private Point[]       _vertPointList = new Point[12];       // list of vertex used during cube calculation
        private bool          _shouldUpdate  = false;

        private Renderer[,,] _sphereVisualizer;  // array of renderer representing the spheres (to help visulize)
		private Vector3Int   _steps;             // number of steps defining how many points are in each directions
		private Point[,,]    _pointList;         // array of all the points in the space
		private Mesh         _newMesh;           // terrain mesh
		
		// Uses the point coordinates as the key and stores an array of 8 int
		// corresponding the the index of all the cubes associated to them (a point can only have up to 8 cubes)
		private Dictionary<string, int[]> _listOfCubesPerPoints = new Dictionary<string, int[]>();

		// Start is called before the first frame update
		void Awake()
		{
			_newMesh = new Mesh();

			// calculates how many steps there can be, inside the bounds defined, depending on the resolution
			_steps = new Vector3Int(
					(int)(BoundingBox.x / Resolution),
					(int)(BoundingBox.y / Resolution),
					(int)(BoundingBox.z / Resolution)
				);

            _pointList        = new Point[_steps.x + 1, _steps.y + 1, _steps.z + 1];
			_sphereVisualizer = new Renderer[_steps.x + 1, _steps.y + 1, _steps.z + 1];

            // Y is up, so it is incremented last
            for(int y = 0; y <= _steps.y; y++) 
			{
				for(int z = 0; z <= _steps.z; z++)
				{
					for(int x = 0; x <= _steps.x; x++)
					{
						// for some reason, when it is just {x}{y}{z}, some keys are duplicates.
						// adding commas seems to fix the issue
                        _listOfCubesPerPoints.Add($"{x},{y},{z}", new int[8] { -1, -1, -1, -1, -1, -1, -1, -1 });

						Vector3 pointPosition = new Vector3(
								-BoundingBox.x/2 + x * Resolution, 
								-BoundingBox.y/2 + y * Resolution,
								-BoundingBox.z/2 + z * Resolution
							);                        

						float noiseSample = -pointPosition.y;

						Point point = new Point(noiseSample, pointPosition);
						_pointList[x,y,z] = point;

						if(AddSpheres)
						{
							GameObject PointView = GameObject.CreatePrimitive(PrimitiveType.Sphere);
							PointView.name = $"{x},{y},{z}";
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
			if(Input.GetMouseButton(0) || Input.GetMouseButton(1))
			{
				if(_shouldUpdate)
                {
                    foreach(int cubeIndex in _cubeIndexList)
                    {
						//Debug.Log(cubeIndex);
                        CalculateCube(_cubeList[cubeIndex]);
                        DebugDrawCube(_cubeList[cubeIndex]);
                    }
                    _cubeIndexList.Clear();
                    DrawCubes();
                    //EditorApplication.isPaused = true;

                }
                _shouldUpdate = false;
			}
		}

		/// <summary>
		/// Creates the mesh based on the cubes calculations
		/// </summary>
		private void DrawCubes()
		{
			_newMesh = new Mesh();
			MainMeshFilter.mesh.Clear();

			_newMesh.vertices = _verticesList.ToArray();
			_newMesh.triangles = _triangleList.ToArray();
			
			MainMeshFilter.mesh = _newMesh;
			MainMeshFilter.mesh.RecalculateNormals();
			MainMeshCollider.sharedMesh = _newMesh;

			_verticesList.Clear();
			_triangleList.Clear();
		}

		/// <summary>
		/// Calculates and looks up the appropriate arragment of vertices and triangles based on the cubes 
		/// points values and appends to a list of vertices and triangles for later use to build the mesh
		/// </summary>
		/// <param name="cube">The cube to calculate</param>
		private void CalculateCube(Cube cube, int passes = -1)
		{
			
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
			
			
			if((marchingCubesLookupTable.edgeTable[cubeIndex] & 1) != 0)
				_vertPointList[0]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(0)), GetPointFromIndex(cube.GetValueAtRow(1)));
			if((marchingCubesLookupTable.edgeTable[cubeIndex] & 2) != 0)
				_vertPointList[1]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(1)), GetPointFromIndex(cube.GetValueAtRow(2)));
			if((marchingCubesLookupTable.edgeTable[cubeIndex] & 4) != 0)
				_vertPointList[2]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(2)), GetPointFromIndex(cube.GetValueAtRow(3)));
			if((marchingCubesLookupTable.edgeTable[cubeIndex] & 8) != 0)
				_vertPointList[3]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(3)), GetPointFromIndex(cube.GetValueAtRow(0)));
			if((marchingCubesLookupTable.edgeTable[cubeIndex] & 16) != 0)
				_vertPointList[4]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(4)), GetPointFromIndex(cube.GetValueAtRow(5)));
			if((marchingCubesLookupTable.edgeTable[cubeIndex] & 32) != 0)
				_vertPointList[5]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(5)), GetPointFromIndex(cube.GetValueAtRow(6)));
			if((marchingCubesLookupTable.edgeTable[cubeIndex] & 64) != 0)
				_vertPointList[6]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(6)), GetPointFromIndex(cube.GetValueAtRow(7)));
			if((marchingCubesLookupTable.edgeTable[cubeIndex] & 128) != 0)
				_vertPointList[7]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(7)), GetPointFromIndex(cube.GetValueAtRow(4)));
			if((marchingCubesLookupTable.edgeTable[cubeIndex] & 256) != 0)
				_vertPointList[8]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(0)), GetPointFromIndex(cube.GetValueAtRow(4)));
			if((marchingCubesLookupTable.edgeTable[cubeIndex] & 512) != 0)
				_vertPointList[9]  = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(1)), GetPointFromIndex(cube.GetValueAtRow(5)));
			if((marchingCubesLookupTable.edgeTable[cubeIndex] & 1024) != 0)
				_vertPointList[10] = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(2)), GetPointFromIndex(cube.GetValueAtRow(6)));
			if((marchingCubesLookupTable.edgeTable[cubeIndex] & 2048) != 0)
				_vertPointList[11] = InterpolateTwoPoints(GetPointFromIndex(cube.GetValueAtRow(3)), GetPointFromIndex(cube.GetValueAtRow(7)));
			
			int ntriang = 0;

            // Inefficient mesh generation.
            // The vertices and triangles are added indiscriminately instead of ignoring the points already 
            // in the vertice list, resulting in meshes with 1700 vertices instead of 300
            for(int i = 0; marchingCubesLookupTable.triTable[cubeIndex, i] != -1; i += 3)
			{
				_verticesList.Add(_vertPointList[marchingCubesLookupTable.triTable[cubeIndex, i    ]].Position);
				_verticesList.Add(_vertPointList[marchingCubesLookupTable.triTable[cubeIndex, i + 1]].Position);
				_verticesList.Add(_vertPointList[marchingCubesLookupTable.triTable[cubeIndex, i + 2]].Position);
				
				_triangleList.Add(_verticesList.Count - 3);
				_triangleList.Add(_verticesList.Count - 2);
				_triangleList.Add(_verticesList.Count - 1);
				ntriang++;
			}
		}

		/// <summary>
		/// Creates the list of cubes to later execute the algorithm
		/// Also links the points to the cubes that uses them
		/// </summary>
		private void CreateCubes()
		{
			string pointName;
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

						int cubeIndex = _cubeList.Count - 1;

                        pointName = $"{x  },{y  },{z}";
						_listOfCubesPerPoints[pointName][_listOfCubesPerPoints[pointName].Count(n => n != -1)] = cubeIndex;
						pointName = $"{x+1},{y  },{z}";															 
						_listOfCubesPerPoints[pointName][_listOfCubesPerPoints[pointName].Count(n => n != -1)] = cubeIndex;
						pointName = $"{x+1},{y+1},{z}";															 
						_listOfCubesPerPoints[pointName][_listOfCubesPerPoints[pointName].Count(n => n != -1)] = cubeIndex;
						pointName = $"{x  },{y+1},{z}";															 
						_listOfCubesPerPoints[pointName][_listOfCubesPerPoints[pointName].Count(n => n != -1)] = cubeIndex;
						pointName = $"{x  },{y  },{z+1}";														 
						_listOfCubesPerPoints[pointName][_listOfCubesPerPoints[pointName].Count(n => n != -1)] = cubeIndex;
						pointName = $"{x+1},{y  },{z + 1}";														 
						_listOfCubesPerPoints[pointName][_listOfCubesPerPoints[pointName].Count(n => n != -1)] = cubeIndex;
						pointName = $"{x+1},{y+1},{z + 1}";														 
						_listOfCubesPerPoints[pointName][_listOfCubesPerPoints[pointName].Count(n => n != -1)] = cubeIndex;
						pointName = $"{x  },{y+1},{z + 1}";														 
						_listOfCubesPerPoints[pointName][_listOfCubesPerPoints[pointName].Count(n => n != -1)] = cubeIndex;
						
					}
				}
			}
		  
		}

		/// <summary>
		/// Calculates the middle point between the two given points
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

		// TODO 12/10/23
		// replace the function by the < operator overload
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
		private void DebugDrawCube(Cube cube, float time = 1f)
		{
			Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(0)).Position, GetPointFromIndex(cube.GetValueAtRow(1)).Position, new Color(1, 0, 0, 1), time);
			Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(1)).Position, GetPointFromIndex(cube.GetValueAtRow(2)).Position, new Color(1, 0, 0, 1), time);
			Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(2)).Position, GetPointFromIndex(cube.GetValueAtRow(3)).Position, new Color(1, 0, 0, 1), time);
			Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(3)).Position, GetPointFromIndex(cube.GetValueAtRow(0)).Position, new Color(1, 0, 0, 1), time);
																																						
			Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(4)).Position, GetPointFromIndex(cube.GetValueAtRow(5)).Position, new Color(1, 0, 0, 1), time);
			Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(5)).Position, GetPointFromIndex(cube.GetValueAtRow(6)).Position, new Color(1, 0, 0, 1), time);
			Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(6)).Position, GetPointFromIndex(cube.GetValueAtRow(7)).Position, new Color(1, 0, 0, 1), time);
			Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(7)).Position, GetPointFromIndex(cube.GetValueAtRow(4)).Position, new Color(1, 0, 0, 1), time);
																																						
			Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(0)).Position, GetPointFromIndex(cube.GetValueAtRow(4)).Position, new Color(1, 0, 0, 1), time);
			Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(1)).Position, GetPointFromIndex(cube.GetValueAtRow(5)).Position, new Color(1, 0, 0, 1), time);
			Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(2)).Position, GetPointFromIndex(cube.GetValueAtRow(6)).Position, new Color(1, 0, 0, 1), time);
			Debug.DrawLine(GetPointFromIndex(cube.GetValueAtRow(3)).Position, GetPointFromIndex(cube.GetValueAtRow(7)).Position, new Color(1, 0, 0, 1), time);
		}

		private Point GetPointFromIndex(int[] index)
		{
			return _pointList[index[0], index[1], index[2]];
		}

		// TODO 12/10/23
		// comment the function
		/// <summary>
		/// 
		/// </summary>
		/// <param name="hit"></param>
		/// <param name="sphereRadius"></param>
		/// <param name="strenght"></param>
		public void UpdateIsoValuesFromCamera(Vector3 hit, float sphereRadius = 1f, float strenght = 0.1f)
		{
			_cubeIndexList.Clear();
            _shouldUpdate = true;
			for(int y = 0; y <= _steps.y; y++)
			{
				for(int z = 0; z <= _steps.z; z++)
				{
					for(int x = 0; x <= _steps.x; x++)
					{
                        Vector3 pointPosition = _pointList[x, y, z].Position;

                        if(!CheckIfPointIsInSphere(hit, pointPosition, sphereRadius))
                            continue;

                        string pointName = $"{x},{y},{z}";
						_listOfCubesPerPoints[pointName].ToList().ForEach(p => { if(p == -1) return; _cubeIndexList.Add(p); Debug.Log(p); });


                        float isoValue = _pointList[x, y, z].IsosurfaceValue + strenght;
                        if(isoValue > 1)
							isoValue = 1;
						else if(isoValue <= 0)
							isoValue = 0;
			
						_pointList[x, y, z].IsosurfaceValue = isoValue;
			
						if(AddSpheres)
						{
							_sphereVisualizer[x, y, z].material.color = new Color(isoValue, isoValue, isoValue);
						}
					}
				}
			}
			// removes any duplicate cube index
			Debug.Log("a :" +_cubeIndexList.Count);
            _cubeIndexList.Distinct();
			Debug.Log("b :" + _cubeIndexList.Count);
		}
		
		/// <summary>
		/// Checks if a point is inside the sphere
		/// </summary>
		/// <param name="sphereOrigin">Origin position of the sphere</param>
		/// <param name="point">Position of the point you want to check</param>
		/// <param name="sphereRadius">Radius of the sphere</param>
		/// <returns>true if point is in sphere, false if not</returns>
		private bool CheckIfPointIsInSphere(Vector3 sphereOrigin, Vector3 point, float sphereRadius)
		{
			//float x2 = (point.x - sphereOrigin.x) * (point.x - sphereOrigin.x);
			//float y2 = (point.y - sphereOrigin.y) * (point.y - sphereOrigin.y);
			//float z2 = (point.z - sphereOrigin.z) * (point.z - sphereOrigin.z);
			//
			//if((x2 + y2 + z2) < (sphereRadius * sphereRadius))
			//	return true;
			//else
			//	return false;
			if(Vector3.Distance(sphereOrigin, point) > sphereRadius)
				return false;
			else
				return true;
		}
	}
}



//IndexCubesBasedOnPoints();
//{
//	foreach(var i in _listOfCubesPerPoints)
//	{
//		string temp = "{ ";
//		var j = i.Value;
//		foreach(int val in j)
//		{
//			temp += $"{val}, ";
//		}
//		Debug.Log(temp + " }");
//	}
//}