using System;
using UnityEngine;

public class BattleShipMover : MonoBehaviour
{
    [SerializeField] float _duration;
    [SerializeField] Path _path;

    public void Move(
        Transform ship,
        Action onEscape,
        Action<PathPoint> onPointReached = null)
    {
        if (ship == null)
            return;

        var mover = new Mover(ship, _path.Points, _duration, onEscape, onPointReached);
        StartCoroutine(mover.Move());
    }
}