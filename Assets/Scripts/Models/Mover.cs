using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover
{
    private readonly Transform _moveObject;
    private readonly List<PathPoint> _path;
    private readonly float _duration;
    private readonly Action _onComplete;
    private readonly Rotator _rotator;
    private readonly Action<PathPoint> _onPointReached;

    public Mover(
        Transform moveObject,
        List<PathPoint> path,
        float duration,
        Action onComplete,
        Action<PathPoint> onPointReached = null)
    {
        _moveObject = moveObject;
        _path = path;
        _duration = duration;
        _onComplete = onComplete;
        _onPointReached = onPointReached;
        
        _rotator = new Rotator(moveObject);
    }

    public IEnumerator Move()
    {
        if (_moveObject == null)
            yield break;

        const int lengthSamples = 24;
        var initialPosition = _moveObject.position;
        var segmentLengths = new List<float>(_path.Count);
        var totalLength = 0f;
        var segmentStart = initialPosition;
        PathPoint previousPoint = null;

        foreach (var point in _path)
        {
            if (point == null || point.Transform == null)
            {
                segmentLengths.Add(0f);
                previousPoint = point;
                continue;
            }

            var segmentLength = 0f;
            var previousSample = segmentStart;
            
            for (var sample = 1; sample <= lengthSamples; sample++)
            {
                var progress = sample / (float)lengthSamples;
                var samplePosition = EvaluateSegment(segmentStart, previousPoint, point, progress);
                segmentLength += Vector3.Distance(previousSample, samplePosition);
                previousSample = samplePosition;
            }

            segmentLengths.Add(segmentLength);
            totalLength += segmentLength;
            segmentStart = point.Transform.position;
            previousPoint = point;
        }

        var elapsed = 0f;
        var segmentStartTime = 0f;
        segmentStart = initialPosition;
        previousPoint = null;

        for (var index = 0; index < _path.Count; index++)
        {
            if (_moveObject == null)
                yield break;

            var targetPoint = _path[index];
            if (targetPoint == null || targetPoint.Transform == null)
                continue;

            var segmentLength = segmentLengths[index];
            var segmentDuration = totalLength > 0f ? Mathf.Max(0f, _duration) * segmentLength / totalLength : 0f;
            var segmentEndTime = segmentStartTime + segmentDuration;

            if (segmentDuration > 0f)
            {
                while (elapsed < segmentEndTime)
                {
                    if (_moveObject == null)
                        yield break;

                    var progress = Mathf.Clamp01((elapsed - segmentStartTime) / segmentDuration);
                    var position = EvaluateSegment(segmentStart, previousPoint, targetPoint, progress);
                    var lookAhead = EvaluateSegment(segmentStart, previousPoint, targetPoint, Mathf.Min(1f, progress + 0.01f));
                    _moveObject.position = position;
                    _rotator.LookAtPoint(lookAhead);
                    yield return null;
                    elapsed += Time.deltaTime;
                }
            }

            if (_moveObject == null)
                yield break;

            _moveObject.position = targetPoint.Transform.position;
            _rotator.LookAtPoint(targetPoint.Transform.position);
            segmentStart = targetPoint.Transform.position;
            previousPoint = targetPoint;
            segmentStartTime = segmentEndTime;
            _onPointReached?.Invoke(targetPoint);
        }

        _onComplete?.Invoke();
    }

    private static Vector3 EvaluateSegment(Vector3 segmentStart, PathPoint previousPoint, PathPoint targetPoint, float progress)
    {
        var endPosition = targetPoint.Transform.position;
        if (previousPoint == null)
            return Vector3.Lerp(segmentStart, endPosition, progress);

        var control1 = previousPoint.GetOutgoingControlPoint(targetPoint);
        var control2 = targetPoint.GetIncomingControlPoint(segmentStart);
        return PathPoint.EvaluateBezier(segmentStart, control1, control2, endPosition, progress);
    }
}