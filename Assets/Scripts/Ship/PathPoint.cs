using UnityEngine;

public class PathPoint : MonoBehaviour
{
    [SerializeField] private PathPoint _targetPont;
    private Transform _transform;

    public PathPoint TargetPont => _targetPont;
    
    public Transform Transform 
    {
        get
        {
            if (_transform == null)
                _transform = transform;
            
            return _transform;
        }
    }
}