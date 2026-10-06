using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipParkingMover : MonoBehaviour
{
    [SerializeField] ExitConnectorsHolder _exitConnectorsHolder;
    [SerializeField] float _duration;
    [SerializeField] Path _path;

    public void EscapeFromParking(
        Transform ship,
        int line,
        bool isMovingAlongX,
        bool isPositiveDirection,
        Action onEscape,
        Action<PathPoint> onPointReached = null)
    {
        if (ship == null)
            return;

        var point = isMovingAlongX
            ? _exitConnectorsHolder.GetPathPoint_Y(line, isPositiveDirection)
            : _exitConnectorsHolder.GetPathPoint_X(line, isPositiveDirection);

        var rotator = ship.GetComponent<Rotator>();
        if (rotator == null)
            rotator = ship.gameObject.AddComponent<Rotator>();

        var mover = new Mover(ship, _path.GetPathPoints(point), _duration, onEscape, rotator, onPointReached);
        StartCoroutine(mover.Move());
    }
}

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
        Rotator rotator,
        Action<PathPoint> onPointReached = null)
    {
        _moveObject = moveObject;
        _path = path;
        _duration = duration;
        _onComplete = onComplete;
        _rotator = rotator;
        _onPointReached = onPointReached;
    }

    public IEnumerator Move()
    {
        if (_moveObject == null)
            yield break;

        var startPosition = _moveObject.position;
        var previousPosition = startPosition;
        var totalLength = 0f;

        foreach (var point in _path)
        {
            totalLength += Vector3.Distance(previousPosition, point.Transform.position);
            previousPosition = point.Transform.position;
        }

        var elapsed = 0f;
        var segmentStartTime = 0f;

        foreach (var targetPoint in _path)
        {
            if (_moveObject == null)
                yield break;

            var segmentLength = Vector3.Distance(startPosition, targetPoint.Transform.position);
            var segmentDuration = totalLength > 0f ? _duration * segmentLength / totalLength : 0f;
            var segmentEndTime = segmentStartTime + segmentDuration;
            _rotator.LookAtPoint(targetPoint.Transform.position);

            while (elapsed < segmentEndTime)
            {
                if (_moveObject == null)
                    yield break;

                _rotator.LookAtPoint(targetPoint.Transform.position);
                var progress = (elapsed - segmentStartTime) / segmentDuration;
                _moveObject.position = Vector3.Lerp(startPosition, targetPoint.Transform.position, progress);
                yield return null;
                elapsed += Time.deltaTime;
            }

            if (_moveObject == null)
                yield break;

            _moveObject.position = targetPoint.Transform.position;
            startPosition = targetPoint.Transform.position;
            segmentStartTime = segmentEndTime;
            _onPointReached?.Invoke(targetPoint);
        }

        _onComplete?.Invoke();
    }
}