using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MarchingCube
{
    public class Point
    {
        private float   _isosurfaceValue;
        private Vector3 _position;

        public float IsosurfaceValue { get; set; }
        public Vector3 Position { get; set; }

        public Point(float isosurfaceValue, Vector3 position)
        {
            _isosurfaceValue = isosurfaceValue;
            _position = position;
        }

    }
}