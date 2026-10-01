using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExitConnectorsHolder : MonoBehaviour
{
    [SerializeField] private List<ExitConnector> _exitConnectorsX;
    [SerializeField] private List<ExitConnector> _exitConnectorsY;
    private Dictionary<int, ExitConnector> _connectorsByLines_Y;
    private Dictionary<int, ExitConnector> _connectorsByLines_X;

    private void Awake()
    {
        _connectorsByLines_X = _exitConnectorsX.ToDictionary(l => l.Line);
        _connectorsByLines_Y = _exitConnectorsY.ToDictionary(l => l.Line);
    }

    public PathPoint GetPathPoint_X(int line)
    {
        if (_connectorsByLines_X.TryGetValue(Mathf.Abs(line), out ExitConnector connector))
        {
            return connector.GetPathPoint(line);
        }
        
        return null;
    }
    
    public PathPoint GetPathPoint_Y(int line)
    {
        if (_connectorsByLines_Y.TryGetValue(Mathf.Abs(line), out ExitConnector connector))
        {
            return connector.GetPathPoint(line);
        }
        
        return null;
    }
}