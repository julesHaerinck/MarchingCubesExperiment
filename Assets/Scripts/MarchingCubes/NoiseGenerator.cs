using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NoiseGenerator : MonoBehaviour
{
	[SerializeField]
	private float _amplitude  = 5f;
	[SerializeField, Range(0f, 1f)]
	private float _frequency  = 0.005f;
	[SerializeField, Range(1, 15)]
	private int   _octaves    = 8;
	[SerializeField, Range(0f,1f)]
	private float _groundPercentage = 0.2f;
	[SerializeField]
	private float _hardGroundPosition = 0f;
	[SerializeField]
	private float _hardWallsWeight = 10f;
	[SerializeField]
	private bool  _walls = true;


    public ComputeShader NoiseCompute;

	ComputeBuffer _weightsBuffer;

	private void Awake()
	{
		CreateBuffers();
	}

	private void OnDestroy()
	{
		ReleaseBuffers();
	}

	private void CreateBuffers()
	{
		_weightsBuffer = new ComputeBuffer(
			GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk, sizeof(float));
	}

	private void ReleaseBuffers()
	{ 
		_weightsBuffer.Release();
	}

	public float[] GetNoise()
	{
		float[] noiseValues = new float[GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk];

		NoiseCompute.SetBuffer(0, "_Weights"  , _weightsBuffer);
		NoiseCompute.SetInt  ("_ChunkSize"    , GridMetrics.PointsPerChunk);
        NoiseCompute.SetInt  ("_Octaves"       , _octaves);
        NoiseCompute.SetFloat("_Amplitude"     , _amplitude);
		NoiseCompute.SetFloat("_Frequency"     , _frequency);
		NoiseCompute.SetFloat("_GroundPercent" , _groundPercentage);
		NoiseCompute.SetFloat("_GroundPosition", _hardGroundPosition);
		NoiseCompute.SetFloat("_WallsWeight"   , _hardWallsWeight);
		NoiseCompute.SetBool ("_Walls"         , _walls);
		



        NoiseCompute.Dispatch(
				0, 
				GridMetrics.PointsPerChunk / GridMetrics.NumThreads, 
				GridMetrics.PointsPerChunk / GridMetrics.NumThreads, 
				GridMetrics.PointsPerChunk / GridMetrics.NumThreads
			);
		_weightsBuffer.GetData(noiseValues);

		return noiseValues;
	}
}
