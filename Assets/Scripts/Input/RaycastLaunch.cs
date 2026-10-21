using UnityEngine;

public static class RaycastLaunch
{
    public static bool TryGetClickedObject<T>(Vector2 screenPosition, out T hitObject) where T : MonoBehaviour
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // The click may land on a child object (overlays, decorations), so the component
            // is looked up through the whole hierarchy.
            var component = hit.transform.GetComponentInParent<T>();

            if (component != null)
            {
                hitObject = component;
                return true;
            }
        }
        
        hitObject = null;
        return false;
    }
}