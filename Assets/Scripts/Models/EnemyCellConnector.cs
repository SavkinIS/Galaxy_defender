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

    private Dictionary<PathPoint, ((int x, int y) startCellIndex, (int x, int y) direction)> _connections;

    private void Awake()
    {
        if (_connections == null)
            BuildConnections();
    }

    public bool TryGetLine(PathPoint point, out (int x, int y) startCellIndex,
        out (int x, int y) direction)
    {
        if (point == null)
            throw new ArgumentNullException(nameof(point));

        if (_connections == null)
            BuildConnections();

        if (_connections.TryGetValue(point, out var connection))
        {
            startCellIndex = connection.startCellIndex;
            direction = connection.direction;
            return true;
        }

        startCellIndex = (-1, -1);
        direction = (0, 0);
        return false;
    }

    public (int x, int y) GetSearchDirection(PathPoint point)
    {
        TryGetLine(point, out _, out var direction);
        return direction;
    }

    public (int x, int y) GetCellIndex(PathPoint point)
    {
        if (TryGetLine(point, out var startCellIndex, out _))
            return startCellIndex;

        Log.Error($"{typeof(EnemyCellConnector)} не содержит сведений о необходимой точке {point}");
        return (-1, -1);
    }

    private void BuildConnections()
    {
        var capacity = _pathPointsDown.Count + _pathPointsUp.Count +
                       _pathPointsLeft.Count + _pathPointsRight.Count;
        _connections = new Dictionary<PathPoint, ((int x, int y) startCellIndex, (int x, int y) direction)>(capacity);
        
        AddConnections(_pathPointsDown, (EnemyFieldSize.MinX, EnemyFieldSize.MinY), (1, 0), (0, 1));
        AddConnections(_pathPointsUp, (EnemyFieldSize.MaxX - 1, EnemyFieldSize.MinY), (0, 1), (-1, 0));
        AddConnections(_pathPointsLeft, (EnemyFieldSize.MaxX - 1, EnemyFieldSize.MaxY - 1), (-1, 0), (0, -1));
        AddConnections(_pathPointsRight, (EnemyFieldSize.MinX, EnemyFieldSize.MaxY - 1), (0, -1), (1, 0));
    }

    private void AddConnections(List<PathPoint> points, (int x, int y) startCellIndex,
        (int x, int y) indexStep, (int x, int y) direction)
    {
        for (var index = 0; index < points.Count; index++)
        {
            var point = points[index];

            if (point == null || _connections.ContainsKey(point))
                continue;

            var cellIndex = (startCellIndex.x + index * indexStep.x,
                startCellIndex.y + index * indexStep.y);
            _connections.Add(point, (cellIndex, direction));
        }
    }
}