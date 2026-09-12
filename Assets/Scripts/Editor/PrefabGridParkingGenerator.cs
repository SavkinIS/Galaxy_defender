#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrefabGridParkingGenerator : MonoBehaviour
{
    [SerializeField] private ParkingCell _prefab;
    [SerializeField] private int _columns = 10;
    [SerializeField] private int _rows = 10;
    [SerializeField] private float _size = 2.5f;
    
    [SerializeField] private ParkingGrid _grid;
    
    public List <ParkingCell> Prefabs = new  List<ParkingCell>();

    [ContextMenu("Generate Grid")]
    private void GenerateGrid()
    {
        
        _grid.SetSize(_columns, _rows);
        Prefabs.Clear();
        
        if (_prefab == null)
        {
            Debug.LogError("Prefab не назначен!");
            return;
        }

        ClearGrid();

        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _columns; x++)
            {
                ParkingCell instance = PrefabUtility.InstantiatePrefab(
                    _prefab,
                    transform
                ) as ParkingCell;
                
                instance.SetIndex(x, y);
                instance.transform.localScale = new Vector3(_size, _size, _size);
                
                instance.transform.localPosition = new Vector3(
                    x * _size,
                    0f,
                    y * _size
                );
                
                instance.name = $"Cell_{x}_{y}";
                
                Prefabs.Add(instance);
            }
        }

        _grid.SetCells(Prefabs);
        
        EditorUtility.SetDirty(gameObject);
    }

    [ContextMenu("Clear Grid")]
    private void ClearGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}

#endif