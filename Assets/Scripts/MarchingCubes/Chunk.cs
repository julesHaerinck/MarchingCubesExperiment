using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeNoise : MonoBehaviour
{
    public NoiseGenerator NoiseGenerator;

    private float[] _weights;
    private int _pointsPerChunck;

    // Start is called before the first frame update
    void Start()
    {
        _weights = NoiseGenerator.GetNoise();
        _pointsPerChunck = GridMetrics.PointsPerChunk;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
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
