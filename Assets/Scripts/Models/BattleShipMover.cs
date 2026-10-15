using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleShipMover : MonoBehaviour
{
    [SerializeField] float _duration;
    [SerializeField] Path _path;

    private Dictionary<Transform, Coroutine> _movementCoroutine = new ();
    
    public void Move(
        Transform ship,
        Action onEscape,
        Action<PathPoint> onPointReached = null)
    {
        if (ship == null)
            return;

        var mover = new Mover(ship, _path.Points, _duration, onEscape, onPointReached);
        _movementCoroutine[ship] = StartCoroutine(mover.Move());
    }

    public void StopMove(Transform ship)
    {
        if (ship == null)
            return;

        if (_movementCoroutine.TryGetValue(ship, out var coroutine))
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
            
            _movementCoroutine.Remove(ship);
        }
    }
}