using System;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private int _x = 0;
    [SerializeField] private int _y = 0;
    public int Y => _y;
    public int X => _x;
    public bool IsActive => gameObject.activeSelf;

    public void SetIndex(int x, int y)
    {
        _x = x;
        _y = y;
    }
    
    public void SetColor(Color color, bool hasColor)
    {
        if (_meshRenderer == null)
            throw new NullReferenceException();
        
        _meshRenderer.material.color = color;

        if (hasColor)
            Enable();
        else
            Disable();
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