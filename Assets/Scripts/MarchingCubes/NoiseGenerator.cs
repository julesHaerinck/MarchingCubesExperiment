using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
	[SerializeField] float _amplitude  = 5f;
	[SerializeField] float _frequency  = 0.005f;
	[SerializeField] int   _octaves    = 8;
	[SerializeField, Range(0f,1f)] 
	float _groundPercentage = 0.2f;

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
		NoiseCompute.SetFloat("_Amplitude"    , _amplitude);
		NoiseCompute.SetFloat("_Frequency"    , _frequency);
		NoiseCompute.SetFloat("_GroundPercent", _groundPercentage);
		NoiseCompute.SetInt  ("_Octaves"      , _octaves);



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
