using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

public class EnemyCellConnector : MonoBehaviour
{
    [SerializeField] private List<PathPoint> _pathPointsDown;
    [SerializeField] private List<PathPoint> _pathPointsUp;
    [SerializeField] private List<PathPoint> _pathPointsLeft;
    [SerializeField] private List<PathPoint> _pathPointsRight;

    public (int x, int y) GetCellIndex(PathPoint point)
    {
        if (point == null)
            throw new ArgumentNullException(nameof(point));
        
        if (_pathPointsDown.Contains(point))
        {
            return (_pathPointsDown.IndexOf(point),  EnemyFieldSize.MinY);
        }
        else if (_pathPointsUp.Contains(point))
        {
            return (_pathPointsUp.IndexOf(point), EnemyFieldSize.MaxY);
        }
        else if (_pathPointsLeft.Contains(point))
        {
            return ( EnemyFieldSize.MinX, _pathPointsLeft.IndexOf(point));
        }
        else if (_pathPointsRight.Contains(point))
        {
            return ( EnemyFieldSize.MaxX, _pathPointsRight.IndexOf(point));
        }
        else
        {
          Log.Error($"{typeof(EnemyCellConnector)} не содержит сведений о необходимой точке {point}");
          return (-1, -1);
        }
    }
}