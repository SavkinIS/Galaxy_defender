using UnityEngine;

public class ParkingCell : MonoBehaviour
{
    [SerializeField] private int _x = 0;
    [SerializeField] private int _y = 0;
    public int Y => _y;
    public int X => _x;

    public void SetIndex(int x, int y)
    {
        _x = x;
        _y = y;
    }

    public void SetOccupied(bool occupied)
    {
        gameObject.SetActive(occupied == false);
    }
}