using Core.Ship;
using UnityEngine;

public class ShipView : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private int _size;
    [SerializeField] private BulletsCounterView _currentBullets;
    
    private ShipData _shipData;
    private RimPlacement _placement;

    public int Size => _size;
    public Renderer VisualRenderer => _meshRenderer;
    public ShipData Data => _shipData;
    public RimPlacement Placement => _placement;

    public void SetData(ShipData data)
    {
        _shipData = data;
        _meshRenderer.material.color = _shipData.Color;
        gameObject.name = $"{gameObject.name}_bullets{_shipData.CurrentBullets}";
        _currentBullets.SetCount(_shipData.CurrentBullets);
    }

    public void SetPlacement(RimPlacement placement)
    {
        _placement = placement;
    }

    public void Refresh()
    {
        _currentBullets.SetCount(_shipData.CurrentBullets);
    }
}