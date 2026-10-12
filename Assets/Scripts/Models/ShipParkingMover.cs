using System;
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

        var mover = new Mover(ship, _path.GetPathPoints(point), _duration, onEscape, onPointReached);
        StartCoroutine(mover.Move());
    }
}