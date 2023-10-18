using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
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

		NoiseCompute.SetBuffer(0, "_Weights", _weightsBuffer);
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
