using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Level Generator/Level Data")]
public class LevelDataSO : ScriptableObject
{
    [Serializable]
    public struct ColorPoint
    {
        [SerializeField] private Color _color;
        
        public Color Color => _color;

        public void Set(Color color)
        {
            _color = color;
        }
    }

    [Serializable]
    public struct CellData
    {
        public const int Empty = -1;
        
        [SerializeField] private int _x;
        [FormerlySerializedAs("Y")] [SerializeField] public int _y;
        [FormerlySerializedAs("ColorIndex")] [SerializeField] public int _colorIndex; 
        
        public int X => _x;
        
        public int Y => _y;
        public int ColorIndex => _colorIndex;
        public bool HasColor => _colorIndex != Empty;

        public void Set(int x, int y, int colorIndex = Empty)
        {
            if (x < 0 || y < 0 )
                throw new ArgumentOutOfRangeException();
            
            _x = x;
            _y = y;
            
            if (colorIndex <=  Empty)
                _colorIndex = Empty;
            else
                _colorIndex = colorIndex;
        } 
    }

    [SerializeField] private Texture2D _levelMap;
    [SerializeField] private ColorPoint[] _colorMappings;
    [SerializeField] private float _stepSize = 1.0f;

    [Header("Запеченная сетка карты")]
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private CellData[] _grid;

    public Texture2D LevelMap => _levelMap;
    public ColorPoint[] ColorMappings => _colorMappings;
    public float StepSize => _stepSize;
    public int Width => _width;
    public int Height => _height;
    public CellData[] Grid => _grid;

    public CellData GetCell(int x, int y)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height) 
            return default;

        return _grid[y * _width + x];
    }
    
    public void SetMappings(ColorPoint[] mappings)
    {
        _colorMappings = mappings;
    }

    public void SetGridData(int width, int height, CellData[] gridData)
    {
        _width = width;
        _height = height;
        _grid = gridData;
    }
    
    public void SetTexture(Texture2D texture)
    {
        _levelMap = texture;
    }

    public Color GetColor(int colorIndex)
    {
        if (_colorMappings.Length <= colorIndex || colorIndex <= LevelDataSO.CellData.Empty)
        {
            var color =  new Color();
            color.a = 0;
            return color;
        }
        
        return _colorMappings[colorIndex].Color;
        
    }
}