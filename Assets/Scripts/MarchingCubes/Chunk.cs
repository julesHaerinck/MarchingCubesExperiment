using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Chunk : MonoBehaviour
{
	public MeshFilter     MeshFilter;
	public NoiseGenerator NoiseGenerator;
	public ComputeShader  MarchingCubes;
	public float          IsoLevel;
	public bool           ShowGizmos = false;

	private ComputeBuffer _trianglesBuffer;
	private ComputeBuffer _trianglesCountBuffer;
	private ComputeBuffer _weightsBuffer;

	private float[] _weights;
	private int _pointsPerChunck;
	private int _numThreads;


	struct Triangles
	{
		public Vector3 a;
		public Vector3 b;
		public Vector3 c;

		public static int SizeOf => sizeof(float) * 3 * 3;
	}

	private void Awake()
	{
        _pointsPerChunck = GridMetrics.PointsPerChunk;
        _numThreads = GridMetrics.NumThreads;
        CreateBuffers();
	}

	// Start is called before the first frame update
	void Start()
	{
		_weights = NoiseGenerator.GetNoise();
		MeshFilter.sharedMesh = ConstructMesh();
	}

	// Update is called once per frame
	void Update()
	{
        _weights = NoiseGenerator.GetNoise();
        MeshFilter.sharedMesh = ConstructMesh();
    }

	private void OnDestroy()
	{
		ReleaseBuffers();
	}

	private Mesh ConstructMesh()
	{
		MarchingCubes.SetBuffer(0, "_Triangles", _trianglesBuffer);
		MarchingCubes.SetBuffer(0, "_Weights", _weightsBuffer);

		MarchingCubes.SetInt("_ChunkSize", _pointsPerChunck);
		MarchingCubes.SetFloat("_IsoLevel", IsoLevel);

		_weightsBuffer.SetData(_weights);
		_trianglesBuffer.SetCounterValue(0);

		MarchingCubes.Dispatch(
				0,
				_pointsPerChunck / _numThreads,
				_pointsPerChunck / _numThreads,
				_pointsPerChunck / _numThreads
			);

		Triangles[] triangles = new Triangles[ReadTriangleCount()];
		_trianglesBuffer.GetData( triangles );

		return CreateMeshFromTriangles( triangles );
	}

	private Mesh CreateMeshFromTriangles(Triangles[] tri) 
	{
		Vector3[] vertices  = new Vector3[tri.Length * 3];
		int[]     triangles = new int[ tri.Length * 3];

		for(int i = 0; i < tri.Length; i++)
		{
			int startIndex = i * 3;

			vertices [startIndex    ] = tri[i].a;
			vertices [startIndex + 1] = tri[i].b;
			vertices [startIndex + 2] = tri[i].c;

			triangles[startIndex    ] = startIndex;
			triangles[startIndex + 1] = startIndex + 1;
			triangles[startIndex + 2] = startIndex + 2;
		}

		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		return mesh;

	}

	private int ReadTriangleCount()
	{
		int[] triCount = { 0 };
		ComputeBuffer.CopyCount(_trianglesBuffer, _trianglesCountBuffer, 0);
		_trianglesCountBuffer.GetData(triCount);
		return triCount[0];
	}

	private void CreateBuffers()
	{
		//                                  (5 *) because there are at most 5 triangles per cubes
		_trianglesBuffer = new ComputeBuffer(5 * (_pointsPerChunck * _pointsPerChunck * _pointsPerChunck), Triangles.SizeOf, ComputeBufferType.Append);
		_trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
		_weightsBuffer = new ComputeBuffer(_pointsPerChunck * _pointsPerChunck * _pointsPerChunck, sizeof(float));
	}

	private void ReleaseBuffers()
	{
		_trianglesBuffer.Release();
		_trianglesCountBuffer.Release();
		_weightsBuffer.Release();
	}

	private void OnDrawGizmos()
	{
		if(!ShowGizmos)
			return;

	    if(_weights == null || _weights.Length == 0)
	        return;
	
	    for(int x = 0; x < _pointsPerChunck; x++)
	    {
	        for(int y = 0; y < _pointsPerChunck; y++)
	        {
	            for(int z = 0; z < _pointsPerChunck; z++)
	            {
	                // converts 3D index to 1D
	                int index = x + _pointsPerChunck * (y + _pointsPerChunck * z);
	
	                float noiseValue = _weights[index];
	                Gizmos.color = new Color(noiseValue, noiseValue, noiseValue);
	                Gizmos.DrawCube(new Vector3(x,y,z), Vector3.one * .2f);
	            }
	        }
	    }
	}
}
