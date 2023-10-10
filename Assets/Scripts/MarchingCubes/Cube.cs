using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Cube
{
    /// <summary>
    /// Stores the index of the 8 points making the cube
    /// </summary>
    private int[] _pointList;
    public int[] PointList { get; set; }

    public Cube(int[] pointList)
    {
        _pointList = pointList;
    }


    /// <summary>
    /// Returnes the value at a specified index
    /// </summary>
    /// <param name="index"></param>
    /// <returns>-1 if given index is outside of array</returns>
    public int GetValueAtIndex(int index)
    {
        if(_pointList.Length < index)
            return -1;
        else   
            return _pointList[index];
    }
    /// <summary>
    /// Set the given value at a specific point in the array
    /// </summary>
    /// <param name="index">Index at which to store the value</param>
    /// <param name="value">value to store</param>
    public void SetValueAtIndex(int index, int value)
    {
        if(_pointList.Length < index)
            return;
        else
            _pointList[index] = value;
    }

    /*
    /// <summary>
    /// Get the full list of indexes 
    /// </summary>
    /// <returns></returns>
    public int[] GetPointList()
    {
        return _pointList;
    }
    /// <summary>
    /// Set a new list of indexes
    /// </summary>
    /// <param name="list">new list of indexes</param>
    public void SetIndexList(int[] list)
    {
        if(_pointList.Length == list.Length)
            _pointList = list;
        else
            return;
    }
    */

}
