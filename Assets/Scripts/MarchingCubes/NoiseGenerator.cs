using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NoiseGenerator : MonoBehaviour
{
	[Header ("Noise 1")]
	[SerializeField] private NOISE_TYPE _noiseType;
	[SerializeField] private FRACTAL_TYPE _fractalType;
	[SerializeField] private float _amplitude  = 5f;
	[SerializeField, Range(0f, 1f)] private float _frequency        = 0.005f;
	[SerializeField]			    private float _lacunarity       = 0f;
	[SerializeField]			    private float _gain             = 0f;
	[SerializeField]			    private float _weightedStrength = 0f;
	[SerializeField, Range(1 , 15)] private int   _octaves          = 8;


	[Header ("Misc Controles")]
	[SerializeField, Range(0f,1f)]
	private float _groundPercentage = 0.2f;
	[SerializeField]
	private float _hardGroundPosition = 0f;
	[SerializeField]
	private float _hardWallsWeight = 10f;
	[SerializeField]
	private bool  _walls = true;

	enum NOISE_TYPE
	{
		OPENSIMPLEX2,
		OPENSIMPLEX2S,
		CELLULAR,
		PERLIN,
		VALUE_CUBIC,
		VALUE
    }

	enum FRACTAL_TYPE
	{
		NONE,
		FBM,
		RIDGED,
		PINGPONG,
		DOMAIN_WARP_PROGRESSIVE,
		DOMAIN_WARP_INDEPENDENT
    }


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

        NoiseCompute.SetInt  ("_Octaves"         , _octaves);
        NoiseCompute.SetInt  ("_NoiseType"       , (int)_noiseType);
        NoiseCompute.SetInt  ("_FractalType"     , (int)_fractalType);
        NoiseCompute.SetFloat("_Amplitude"       , _amplitude);
		NoiseCompute.SetFloat("_Frequency"       , _frequency);
		NoiseCompute.SetFloat("_Lacunarity"      , _lacunarity);
		NoiseCompute.SetFloat("_Gain"            , _gain);
		NoiseCompute.SetFloat("_WeightedStrength", _weightedStrength);

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
