using UnityEngine;

public static class RaycastLaunch
{
    public static bool TryGetClickedObject<T>(Vector2 screenPosition, out T hitObject) where T : MonoBehaviour
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.TryGetComponent(out T component))
            {
                hitObject = component;
                return true;
            }
        }
        
        hitObject = null;
        return false;
    }
}