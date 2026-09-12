using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MapParserWindow : EditorWindow
{
    private List<ColorMapping> _colorMappings = new List<ColorMapping>();
    private Texture2D _sourceTexture;
    private float _colorTolerance = 0.08f;
    private Texture2D _previewTexture;
    private Vector2 _scrollPosition;

    [MenuItem("Tools/Map Parser")]
    public static void ShowWindow()
    {
        GetWindow<MapParserWindow>("Map Parser");
    }

    private void OnGUI()
    {
        GUILayout.Label("Parser map from sprite", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        _sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Map sprite", _sourceTexture, typeof(Texture2D), false);

        EditorGUI.BeginChangeCheck();
        _colorTolerance = EditorGUILayout.Slider("Color tolerance", _colorTolerance, 0.001f, 0.3f);
        bool toleranceChanged = EditorGUI.EndChangeCheck();

        if ((EditorGUI.EndChangeCheck() || toleranceChanged) && _sourceTexture != null)
        {
            ParsePalette();
        }

        if (_sourceTexture == null)
        {
            EditorGUILayout.HelpBox("Set sprite before parse", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(10);

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        DrawPaletteControls();

        EditorGUILayout.Space(15);

        DrawPreviewGrid();

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Save LevelDataSO", GUILayout.Height(35)))
        {
            SaveLevelDataAsset();
        }
    }

    private void ParsePalette()
    {
        _colorMappings = SpriteMapParser.ParsePalette(_sourceTexture, _colorTolerance);
        UpdatePreview();
    }

    private void ResetColors()
    {
        foreach (var mapping in _colorMappings)
        {
            mapping.TargetColor = mapping.OriginalColor;
        }

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        _previewTexture =
            SpriteMapParser.GeneratePreviewTexture(_sourceTexture, _colorMappings, _colorTolerance, _previewTexture);
    }

    private void DrawPaletteControls()
    {
        EditorGUILayout.LabelField($"Pallete Groups: {_colorMappings.Count}", EditorStyles.boldLabel);

        if (GUILayout.Button("Reset", GUILayout.Width(200)))
        {
            ResetColors();
        }

        EditorGUILayout.Space(5);

        for (int i = 0; i < _colorMappings.Count; i++)
        {
            var mapping = _colorMappings[i];

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"Color_{mapping.PixelCount}", GUILayout.Width(80));

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ColorField(GUIContent.none, mapping.OriginalColor, false, false, false,
                GUILayout.Width(50));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.LabelField("➔", GUILayout.Width(20));

            EditorGUI.BeginChangeCheck();
            mapping.TargetColor = EditorGUILayout.ColorField(GUIContent.none, mapping.TargetColor, false, false, false,
                GUILayout.Width(50));
            if (EditorGUI.EndChangeCheck())
            {
                UpdatePreview();
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawPreviewGrid()
    {
        if (_previewTexture == null) return;

        EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);

        float previewSize = Mathf.Min(EditorGUIUtility.currentViewWidth - 40, 300f);
        Rect rect = GUILayoutUtility.GetRect(previewSize, previewSize, GUILayout.ExpandWidth(false));

        GUI.DrawTexture(rect, _previewTexture, ScaleMode.ScaleToFit);
    }

    private void SaveLevelDataAsset()
    {
        if (_sourceTexture == null)
        {
            Debug.LogError("[MapParser] Отсутствует исходная текстура!");
            return;
        }

        string folderPath = "Assets/MapData";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        LevelDataSO levelData = ScriptableObject.CreateInstance<LevelDataSO>();

        LevelDataSO.ColorPoint[] colorPoints = new LevelDataSO.ColorPoint[_colorMappings.Count];
        for (int i = 0; i < _colorMappings.Count; i++)
        {
            var point = new LevelDataSO.ColorPoint();
            point.Set(_colorMappings[i].TargetColor);
            colorPoints[i] = point;
        }

        levelData.SetMappings(colorPoints);

        int width = _sourceTexture.width;
        int height = _sourceTexture.height;
        LevelDataSO.CellData[] gridCells =
            SpriteMapParser.GenerateGridCells(_sourceTexture, _colorMappings, _colorTolerance);

        levelData.SetGridData(width, height, gridCells);
        levelData.SetTexture(_sourceTexture);

        string assetPath = $"{folderPath}/{_sourceTexture.name}_LevelData.asset";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        AssetDatabase.CreateAsset(levelData, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = levelData;
    }
}