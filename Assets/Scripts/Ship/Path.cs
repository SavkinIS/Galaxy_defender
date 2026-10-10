using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    [SerializeField] private List<PathPoint> _points;
    [SerializeField] private PathPoint _pointPrefab;

    public PathPoint PointPrefab => _pointPrefab;
    public int PointCount => _points == null ? 0 : _points.Count;

    public Vector3[] Points
    {
        get
        {
            var positions = new Vector3[PointCount];
            for (var index = 0; index < PointCount; index++)
                positions[index] = GetPoint(index).Transform.position;

            return positions;
        }
    }

    public PathPoint GetPoint(int index)
    {
        return _points[index];
    }

    public Vector3[] GetSegmentPoints(int segmentIndex, int resolution = 24)
    {
        if (segmentIndex < 0 || segmentIndex >= PointCount)
            return new Vector3[0];

        var startPoint = _points[segmentIndex];
        if (startPoint == null)
            return new Vector3[0];

        var endPoint = startPoint.TargetPont;
        if (endPoint == null)
            return new Vector3[0];

        resolution = Mathf.Max(1, resolution);
        var start = startPoint.Transform.position;
        var end = endPoint.Transform.position;
        var control1 = startPoint.GetOutgoingControlPoint(endPoint);
        var control2 = endPoint.GetIncomingControlPoint(start);
        var positions = new Vector3[resolution + 1];

        for (var index = 0; index <= resolution; index++)
        {
            var t = (float)index / resolution;
            positions[index] = PathPoint.EvaluateBezier(start, control1, control2, end, t);
        }

        return positions;
    }

    public float GetSegmentLength(int segmentIndex, int resolution = 24)
    {
        var positions = GetSegmentPoints(segmentIndex, resolution);
        var length = 0f;
        for (var index = 1; index < positions.Length; index++)
            length += Vector3.Distance(positions[index - 1], positions[index]);

        return length;
    }

    public Vector3[] GetPointsByPoint(PathPoint startPoint)
    {
        List<Vector3> points = new List<Vector3>();

        points.Add(startPoint.Transform.position);
        var target = startPoint.TargetPont;

        while (target != null)
        {
            points.Add(target.Transform.position);
            target = target.TargetPont;
        }
        
        return points.ToArray();
    }

    public List<PathPoint> GetPathPoints(PathPoint startPoint)
    {
        List<PathPoint> points = new  List<PathPoint>();

        points.Add(startPoint);
        var target = startPoint .TargetPont;

        while (target != null)
        {
            points.Add(target);
            target = target.TargetPont;
        }
        
        return points;
    }
}