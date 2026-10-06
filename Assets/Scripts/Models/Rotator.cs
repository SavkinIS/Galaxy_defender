using UnityEngine;

public class Rotator : MonoBehaviour
{
    public void LookAtPoint(Vector3 targetPosition)
    {
        var direction = targetPosition - transform.position;
        if (direction.sqrMagnitude < 0.000001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}
