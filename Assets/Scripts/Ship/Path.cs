using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    [SerializeField] private List<PathPoint> _points;

    private Vector3[] _pointsPosition = null;
    
    public Vector3[] Points
    {
        get
        {
            if (_pointsPosition == null)
            {
                _pointsPosition = new Vector3[_points.Count];

                for (var index = 0; index < _points.Count; index++)
                {
                    var point = _points[index];
                    _pointsPosition[index] = point.Transform.position;
                }
            }
            
            return _pointsPosition;
        }
    }

    public Vector3[] GetPointsByPoint(PathPoint startPoint)
    {
        List<Vector3> points = new List<Vector3>();

        int startIndex = _points.IndexOf(startPoint);
        
        for (int i = startIndex; i >= 0; i--)
        {
            var point = _points[i];
            if (point.TargetPont != null) 
                points.Add(_points[i].Transform.position);
        }
        
        return points.ToArray();
    }
}