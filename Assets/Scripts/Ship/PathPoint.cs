using UnityEngine;

public class PathPoint : MonoBehaviour
{
    [SerializeField] private PathPoint _targetPont;
    [SerializeField] private Vector3 _incomingHandle;
    [SerializeField] private Vector3 _outgoingHandle;
    [SerializeField] private bool _hasIncomingHandle;
    [SerializeField] private bool _hasOutgoingHandle;
    [SerializeField] private UnityEngine.Events.UnityEvent _onVisited = new UnityEngine.Events.UnityEvent();
    private Transform _transform;

    public PathPoint TargetPont => _targetPont;
    public UnityEngine.Events.UnityEvent OnVisited => _onVisited;

    public Vector3 GetOutgoingControlPoint(PathPoint nextPoint)
    {
        if (_hasOutgoingHandle)
            return Transform.TransformPoint(_outgoingHandle);

        return Vector3.Lerp(Transform.position, nextPoint.Transform.position, 1f / 3f);
    }

    public Vector3 GetIncomingControlPoint(Vector3 previousPosition)
    {
        if (_hasIncomingHandle)
            return Transform.TransformPoint(_incomingHandle);

        return Vector3.Lerp(previousPosition, Transform.position, 2f / 3f);
    }

    public void SetOutgoingControlPoint(Vector3 worldPosition)
    {
        _outgoingHandle = Transform.InverseTransformPoint(worldPosition);
        _hasOutgoingHandle = true;
    }

    public void SetIncomingControlPoint(Vector3 worldPosition)
    {
        _incomingHandle = Transform.InverseTransformPoint(worldPosition);
        _hasIncomingHandle = true;
    }

    public void InvokeVisited()
    {
        _onVisited?.Invoke();
    }

    public static Vector3 EvaluateBezier(Vector3 start, Vector3 control1, Vector3 control2, Vector3 end, float t)
    {
        float inverseT = 1f - t;
        return inverseT * inverseT * inverseT * start
            + 3f * inverseT * inverseT * t * control1
            + 3f * inverseT * t * t * control2
            + t * t * t * end;
    }

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