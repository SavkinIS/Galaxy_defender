using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private const int LeftMouseButton = 0;
    private const int RightMouseButton = 1;
    
    public event Action<Vector2> MouseClicked;

    private void Update()
    {
        if (Input.GetMouseButtonDown(LeftMouseButton))
        {
            MouseClicked?.Invoke(Input.mousePosition);
        }
    }
}