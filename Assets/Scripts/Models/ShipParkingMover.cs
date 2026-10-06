using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
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
        Action onEscape)
    {
        var point = isMovingAlongX
            ? _exitConnectorsHolder.GetPathPoint_Y(line, isPositiveDirection)
            : _exitConnectorsHolder.GetPathPoint_X(line, isPositiveDirection);

        var mover = new Mover(ship, _path.GetPointsByPoint(point), onEscape);

        //mover.Move();
        
        ship.DOPath(_path.GetPointsByPoint(point), _duration, PathType.CatmullRom, PathMode.Full3D).SetEase(Ease.Linear)
            .OnComplete(() => onEscape?.Invoke());
    }
}

public class Mover
{
    private readonly Transform _moveObject;
    private readonly Vector3[] _path;
    private readonly Action _onComplete;


    public Mover(Transform moveObject, Vector3[] path, Action onComplete)
    {
        _moveObject =  moveObject;
        _path = path;
        _onComplete = onComplete;
    }

    public void Move()
    {
        MonoBehaviour mono = _moveObject.GetComponent<MonoBehaviour>();

    
    }

   
}