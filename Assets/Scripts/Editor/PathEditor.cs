using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Path))]
public class PathEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Path path = (Path)target;
        EditorGUILayout.Space();
        if (path.PointPrefab == null)
        {
            EditorGUILayout.HelpBox("Сначала назначьте префаб точки в поле Point Prefab.", MessageType.Warning);
        }

        EditorGUI.BeginDisabledGroup(path.PointPrefab == null);
        if (GUILayout.Button("Создать точку маршрута на месте предыдущей"))
        {
            CreatePoint(path);
        }
        EditorGUI.EndDisabledGroup();
    }

    private static void CreatePoint(Path path)
    {
        SerializedObject serializedPath = new SerializedObject(path);
        serializedPath.Update();
        SerializedProperty points = serializedPath.FindProperty("_points");
        
        if (points == null || !points.isArray)
        {
            Debug.LogError("Не удалось найти список точек маршрута.", path);
            return;
        }

        PathPoint previousPoint = points.arraySize > 0
            ? points.GetArrayElementAtIndex(points.arraySize - 1).objectReferenceValue as PathPoint
            : null;
        Vector3 position = previousPoint != null
            ? previousPoint.transform.position
            : path.transform.position;

        PathPoint point = PrefabUtility.InstantiatePrefab(path.PointPrefab, path.transform) as PathPoint;
        
        GameObject pointObject = point.gameObject;
        
        if (pointObject == null)
        {
            Debug.LogError("Не удалось создать точку из назначенного префаба. Убедитесь, что назначен prefab asset.", path);
            return;
        }

        PathPoint newPoint = pointObject.GetComponentInChildren<PathPoint>();
        
        if (newPoint == null)
        {
            Undo.DestroyObjectImmediate(pointObject);
            Debug.LogError("В назначенном префабе не найден компонент PathPoint.", path);
            return;
        }

        pointObject.name = $"Path Point {points.arraySize + 1}";
        Undo.RegisterCreatedObjectUndo(pointObject, "Create Path Point");
        Undo.SetTransformParent(pointObject.transform, path.transform, "Parent Path Point");
        pointObject.transform.position = position;

        serializedPath.Update();
        points = serializedPath.FindProperty("_points");
        points.arraySize++;
        points.GetArrayElementAtIndex(points.arraySize - 1).objectReferenceValue = newPoint;
        serializedPath.ApplyModifiedProperties();

        if (previousPoint != null)
        {
            SerializedObject serializedPreviousPoint = new SerializedObject(previousPoint);
            serializedPreviousPoint.Update();
            SerializedProperty targetPoint = serializedPreviousPoint.FindProperty("_targetPont");
            if (targetPoint != null)
            {
                targetPoint.objectReferenceValue = newPoint;
                serializedPreviousPoint.ApplyModifiedProperties();
            }
        }

        Selection.activeGameObject = pointObject;
    }

    [MenuItem("GameObject/Path/Create Path", false, 10)]
    private static void CreatePath()
    {
        GameObject pathObject = new GameObject("Path");
        Undo.RegisterCreatedObjectUndo(pathObject, "Create Path");
        Undo.AddComponent<Path>(pathObject);
        Selection.activeGameObject = pathObject;
    }

    private void OnSceneGUI()
    {
        Path path = (Path)target;
        for (int index = 0; index < path.PointCount; index++)
        {
            PathPoint point = path.GetPoint(index);
            if (point == null)
            {
                continue;
            }

            EditorGUI.BeginChangeCheck();
            Vector3 movedPosition = Handles.PositionHandle(point.transform.position, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(point.transform, "Move Path Point");
                point.transform.position = movedPosition;
                EditorUtility.SetDirty(point.transform);
            }

            Handles.Label(point.transform.position, $"Point {index + 1}");
        }

        for (int index = 0; index < path.PointCount; index++)
        {
            PathPoint startPoint = path.GetPoint(index);
            if (startPoint == null)
            {
                continue;
            }

            PathPoint endPoint = startPoint.TargetPont;
            if (endPoint == null)
            {
                continue;
            }

            Vector3 startPosition = startPoint.transform.position;
            Vector3 endPosition = endPoint.transform.position;
            Vector3 outgoing = startPoint.GetOutgoingControlPoint(endPoint);
            Vector3 incoming = endPoint.GetIncomingControlPoint(startPosition);

            Handles.DrawLine(startPosition, outgoing);
            Handles.DrawLine(endPosition, incoming);
            Handles.DrawBezier(startPosition, endPosition, outgoing, incoming, Color.cyan, null, 2f);

            EditorGUI.BeginChangeCheck();
            Vector3 movedOutgoing = Handles.PositionHandle(outgoing, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(startPoint, "Move Path Control Point");
                startPoint.SetOutgoingControlPoint(movedOutgoing);
                EditorUtility.SetDirty(startPoint);
            }

            EditorGUI.BeginChangeCheck();
            Vector3 movedIncoming = Handles.PositionHandle(incoming, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(endPoint, "Move Path Control Point");
                endPoint.SetIncomingControlPoint(movedIncoming);
                EditorUtility.SetDirty(endPoint);
            }
        }
    }
}
