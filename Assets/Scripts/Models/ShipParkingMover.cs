using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class ShipParkingMover : MonoBehaviour
{
    [SerializeField] ExitConnectorsHolder _exitConnectorsHolder;
    [SerializeField] float _duration;
    [SerializeField] Path _path;

    public void EscapeFromParking(Transform ship, int line, bool isX, Action onEscape)
    {
        var point = isX ? _exitConnectorsHolder.GetPathPoint_X(line) : _exitConnectorsHolder.GetPathPoint_Y(line);
        ship.DOPath(_path.GetPointsByPoint(point), _duration, PathType.Linear, PathMode.Full3D).OnComplete(() => onEscape?.Invoke());
    }
}