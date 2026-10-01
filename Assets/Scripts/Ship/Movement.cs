using System.Linq;
using DG.Tweening;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private float _duration;
    [SerializeField] private Path _toBattle;
    [SerializeField] private Path _roundBattle;

    public void MoveToBattle()
    {
        transform.DOPath(_toBattle.Points.ToArray(), _duration, PathType.CubicBezier, PathMode.Full3D);
    }
    public void MoveRoundBattle()
    {
        transform.DOPath(_roundBattle.Points.ToArray(), _duration, PathType.CubicBezier, PathMode.Full3D);
    }
}