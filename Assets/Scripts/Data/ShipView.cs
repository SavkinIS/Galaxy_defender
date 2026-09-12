using UnityEngine;

public class ShipView : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private int _size;
    
    private ShipData _shipData;
    
    public int Size => _size;

    public void SetData(ShipData data)
    {
        _shipData = data;
        _meshRenderer.material.color = _shipData.Color;
        gameObject.name = $"{gameObject.name}_bullets{_shipData.CurrentBullets}";
    }
}