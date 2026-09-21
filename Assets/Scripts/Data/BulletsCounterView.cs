using UnityEngine;

public class BulletsCounterView : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI _bulletsCount;

    public void SetCount(int count)
    {
        if (count <= 0)
        {
            _bulletsCount.gameObject.SetActive(false);
            return;
        }
        
        _bulletsCount.text = count.ToString();
    }
}