using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MarchingCube
{
    public class Point
    {
        private float   _isosurfaceValue;
        private Vector3 _position;

        public float IsosurfaceValue { get => _isosurfaceValue; set { _isosurfaceValue = value; } }
        public Vector3 Position      { get => _position;        set { _position = value; } }

        public Point(float isosurfaceValue, Vector3 position)
        {
            _isosurfaceValue = isosurfaceValue;
            _position = position;
        }
        public Point()
        {
            _isosurfaceValue = 0;
            _position = Vector3.zero;
        }

    }
}