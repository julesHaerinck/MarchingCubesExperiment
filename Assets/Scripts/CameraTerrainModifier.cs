using MarchingCube;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO
// Find a better way to change the values 

public class CameraTerrainModifier : MonoBehaviour
{
    public float RaycastDistance = 10f;
    public float HitSphereRadius = 5f;
    public float ModifyStrength  = 0.1f;
    public MarchingCubesGeneration MarchingCubeMamnager;

    private RaycastHit hit;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if(Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            float value = -1;
            if(Input.GetMouseButton(0))
                value = ModifyStrength;
            else
                value = -ModifyStrength;
            if(Physics.Raycast(transform.position, transform.forward, out hit, RaycastDistance))
            {
                MarchingCubeMamnager.UpdateIsoValuesFromCamera(hit.point, HitSphereRadius, value);
            }
        }
    }
}
