using UnityEngine;

public class BulletsCounterView : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI _bulletsCount;

    private Camera _camera;

    private void LateUpdate()
    {
        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null)
            return;

        Vector3 directionToCamera = _camera.transform.position - transform.position;
        if (directionToCamera.sqrMagnitude <= Mathf.Epsilon)
            return;

        transform.rotation = Quaternion.LookRotation(directionToCamera, _camera.transform.up);
    }

    public void SetCount(int count)
    {
        if (count <= 0)
        {
            _bulletsCount.gameObject.SetActive(false);
            return;
        }
        
        _bulletsCount.text = count.ToString();
    }

    public void Enable()
    {
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}