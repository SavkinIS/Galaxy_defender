using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class TransformStepWindow : EditorWindow
{
    [SerializeField] private List<Transform> _transforms = new List<Transform>();
    [SerializeField] private Vector3 _step = Vector3.right;

    [MenuItem("Tools/Transform Step")]
    private static void OpenWindow()
    {
        GetWindow<TransformStepWindow>("Transform Step");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Объекты будут расставлены от первого трансформа с указанным шагом в мировых координатах. Первый объект останется на месте.",
            MessageType.Info);

        _step = EditorGUILayout.Vector3Field("Шаг", _step);
        EditorGUILayout.Space();

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
            if (GUILayout.Button("Расставить с указанным шагом"))
                ArrangeTransforms();
        }

        if (_transforms.Count < 2)
            EditorGUILayout.HelpBox("Добавь минимум два объекта.", MessageType.Warning);
        else if (HasMissingTransforms())
            EditorGUILayout.HelpBox("Заполни или удали пустые слоты перед расстановкой.", MessageType.Warning);
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
            if (GUILayout.Button("Вверх", GUILayout.Width(55)))
                SwapTransforms(index, index - 1);
        }

        using (new EditorGUI.DisabledScope(index == _transforms.Count - 1))
        {
            if (GUILayout.Button("Вниз", GUILayout.Width(55)))
                SwapTransforms(index, index + 1);
        }

        if (GUILayout.Button("Удалить", GUILayout.Width(65)))
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

    private void ArrangeTransforms()
    {
        if (_transforms.Count < 2 || HasMissingTransforms())
            return;

        var startPosition = _transforms[0].position;
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Arrange Transforms with Step");
        var undoGroup = Undo.GetCurrentGroup();
        Undo.RecordObjects(_transforms.ToArray(), "Arrange Transforms with Step");

        for (var index = 1; index < _transforms.Count; index++)
        {
            var transform = _transforms[index];
            transform.position = startPosition + _step * index;
            PrefabUtility.RecordPrefabInstancePropertyModifications(transform);
        }

        Undo.CollapseUndoOperations(undoGroup);
        SceneView.RepaintAll();
    }
}
