using System;
using UnityEngine;

public class ExitConnector : MonoBehaviour
{
    [SerializeField] private int _line;
    [SerializeField] private PathPoint _pathPointPositive;
    [SerializeField] private PathPoint _pathPointNegative;
    [SerializeField] private bool _isX = true;

    public int Line => _line;
    
    public PathPoint GetPathPoint(int line)
    {
        if (Mathf.Abs(line) == Mathf.Abs(_line))
        {
            if (line < _line)      
                return _pathPointNegative;
            
            return _pathPointPositive;
        }

        return null;
    }


    private void OnValidate()
    {
        if (_pathPointPositive != null)
        {
            if (_isX)
                _pathPointPositive.name = $"point_{_line},{0}_positive";
            else
                _pathPointPositive.name = $"point_{0},{_line}_positive";
        }
        if (_pathPointNegative != null)
        {
            if (_isX)
                _pathPointNegative.name = $"point_{_line},{0}_negative";
            else
                _pathPointNegative.name = $"point_{0},{_line}_negative";
            
        }
    }
}