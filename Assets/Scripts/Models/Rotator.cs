using UnityEngine;

public class Rotator
{
    private readonly Transform _transform;

    public Rotator(Transform transform)
    {
        _transform = transform;
    }
    
    public void LookAtPoint(Vector3 targetPosition)
    {
        var direction = targetPosition - _transform.position;
        if (direction.sqrMagnitude < 0.000001f)
            return;

        _transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}
