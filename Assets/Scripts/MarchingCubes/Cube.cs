using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace MarchingCube
{
    public class Cube
    {
        private int[,] _pointList = new int[8,3];
        /// <summary>
        /// Store the points of the cube.
        /// Each cube has 8 points. 
        /// Each point has 3 values (x,y,z)
        /// Therefore, an array of [8,3]
        /// </summary>
        public int[,] PointList { get => _pointList; set { _pointList = value; } }

        public Cube(int[,] pointList)
        {
            _pointList = pointList;
        }


        /// <summary>
        /// Returns the value at a specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns>-1 if given index is outside of array</returns>
        public int GetValueAtIndex(int row, int col)
        {
            // TODO
            // Add checks to see if the parameters are inside the array
            //if(_pointList.Length < index)
            //    return -1;
            //else
            return _pointList[row,col];
        }
        /// <summary>
        /// Returns the values at a specified row
        /// </summary>
        /// <param name="row">The rows to get the values from</param>
        /// <returns>Int array of 3 values</returns>
        public int[] GetValueAtRow(int row)
        {
            return new int[3] { _pointList[row,0], _pointList[row, 1] , _pointList[row, 2] };
        }
        /// <summary>
        /// Set the given value at a specific point in the array
        /// </summary>
        /// <param name="index">Index at which to store the value</param>
        /// <param name="value">value to store</param>
        public void SetValueAtIndex(int row, int col, int value)
        {
            // TODO
            // Add checks to see if the parameters are inside the array
            //if(_pointList.Length < index)
            //    return;
            //else
            _pointList[row, col] = value;
        }
    }
}