using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class TransformLineupWindow : EditorWindow
{
    [SerializeField] private List<Transform> _transforms = new List<Transform>();

    [MenuItem("Tools/Transform Lineup")]
    private static void OpenWindow()
    {
        GetWindow<TransformLineupWindow>("Transform Lineup");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Объекты будут равномерно расставлены по прямой от первого трансформа до последнего. Крайние объекты останутся на месте.",
            MessageType.Info);

        for (var index = 0; index < _transforms.Count; index++)
            DrawTransformRow(index);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Добавить выделенные"))
            AddSelectedTransforms();
        if (GUILayout.Button("Добавить слот"))
            _transforms.Add(null);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(_transforms.Count < 2 || HasMissingTransforms()))
        {
            if (GUILayout.Button("Выровнять между первым и последним"))
                AlignTransforms();
        }

        if (_transforms.Count < 2)
            EditorGUILayout.HelpBox("Добавь минимум два объекта.", MessageType.Warning);
        else if (HasMissingTransforms())
            EditorGUILayout.HelpBox("Заполни или удали пустые слоты перед выравниванием.", MessageType.Warning);
    }

    private void DrawTransformRow(int index)
    {
        EditorGUILayout.BeginHorizontal();
        _transforms[index] = (Transform)EditorGUILayout.ObjectField(
            $"Объект {index + 1}",
            _transforms[index],
            typeof(Transform),
            true);

        using (new EditorGUI.DisabledScope(index == 0))
        {
            if (GUILayout.Button("↑", GUILayout.Width(26)))
                SwapTransforms(index, index - 1);
        }

        using (new EditorGUI.DisabledScope(index == _transforms.Count - 1))
        {
            if (GUILayout.Button("↓", GUILayout.Width(26)))
                SwapTransforms(index, index + 1);
        }

        if (GUILayout.Button("−", GUILayout.Width(26)))
            _transforms.RemoveAt(index);
        EditorGUILayout.EndHorizontal();
    }

    private void AddSelectedTransforms()
    {
        foreach (var selectedTransform in Selection.transforms)
        {
            if (!_transforms.Contains(selectedTransform))
                _transforms.Add(selectedTransform);
        }
    }

    private bool HasMissingTransforms()
    {
        return _transforms.Exists(transform => transform == null);
    }

    private void SwapTransforms(int firstIndex, int secondIndex)
    {
        var transform = _transforms[firstIndex];
        _transforms[firstIndex] = _transforms[secondIndex];
        _transforms[secondIndex] = transform;
    }

    private void AlignTransforms()
    {
        if (_transforms.Count < 2 || HasMissingTransforms())
            return;

        var startPosition = _transforms[0].position;
        var endPosition = _transforms[_transforms.Count - 1].position;
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Align Transforms in Line");
        var undoGroup = Undo.GetCurrentGroup();

        for (var index = 1; index < _transforms.Count - 1; index++)
        {
            var transform = _transforms[index];
            Undo.RecordObject(transform, "Align Transforms in Line");
            transform.position = Vector3.Lerp(startPosition, endPosition, (float)index / (_transforms.Count - 1));
            PrefabUtility.RecordPrefabInstancePropertyModifications(transform);
        }

        Undo.CollapseUndoOperations(undoGroup);
        SceneView.RepaintAll();
    }
}

