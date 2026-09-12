#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrefabGridGenerator : MonoBehaviour
{
    [SerializeField] private Cell _prefab;
    [SerializeField] private int _columns = 39;
    [SerializeField] private int _rows = 30;
    [SerializeField] private float _spacing = 1f;
    [SerializeField] private CellMap _cellMap;
    
    public List <Cell> Prefabs = new  List<Cell>();

    [ContextMenu("Generate Grid")]
    private void GenerateGrid()
    {
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
                Cell instance = PrefabUtility.InstantiatePrefab(
                        _prefab,
                        transform
                    ) as Cell;

                instance.transform.localPosition = new Vector3(
                    x * _spacing,
                    0f,
                    y * _spacing
                );
                
                instance.SetIndex(x, y);

                instance.name = $"Cell_{x}_{y}";
                
                Prefabs.Add(instance);
                _cellMap.SetCell(instance);
            }
        }

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