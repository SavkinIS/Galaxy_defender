using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCreator : MonoBehaviour
{
    [SerializeField] private LevelDataSO _levelData;
    [SerializeField] private LevelTemplate _levelTemplate;

    public void Generate()
    {
        if (_levelData.Grid.Length != _levelTemplate.GridLenght)
            throw new InvalidOperationException();

        for (int index = 0; index < _levelData.Grid.Length; index++)
        {
            var data = _levelData.Grid[index];

            Color color = new Color();

            if (data.HasColor && data.ColorIndex < _levelData.ColorMappings.Length)
                color = _levelData.ColorMappings[data.ColorIndex].Color;

            _levelTemplate.SetBlockData(data, index, color);
        }
    }
}


public class LevelTemplate : MonoBehaviour
{
    [SerializeField] private List<Block> _blocks;

    public int GridLenght => _blocks.Count;

    public void SetBlockData(LevelDataSO.CellData cellData, int index, Color color)
    {
        _blocks[index].SetData(cellData, color);
    }
}