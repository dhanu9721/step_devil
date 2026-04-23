#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace StepDevil.Editor
{
    [CustomEditor(typeof(StepDevilLevelPathAuthoring))]
    public sealed class StepDevilLevelPathAuthoringEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var auth = (StepDevilLevelPathAuthoring)target;
            if (GUILayout.Button("Resize offsets to fork count (default line)"))
            {
                var idx = Mathf.Clamp(auth.LevelId - 1, 0, StepDevilDatabase.LevelCount - 1);
                var n = StepDevilDatabase.GetLevel(idx).ForkCount;
                var so = serializedObject;
                var prop = so.FindProperty("_forkOffsets");
                prop.arraySize = n;
                for (var i = 0; i < n; i++)
                    prop.GetArrayElementAtIndex(i).vector3Value = Vector3.right * (i * 1.5f);
                so.ApplyModifiedProperties();
            }
        }

        void OnSceneGUI()
        {
            var auth = (StepDevilLevelPathAuthoring)target;
            var idx = Mathf.Clamp(auth.LevelId - 1, 0, StepDevilDatabase.LevelCount - 1);
            var lv = StepDevilDatabase.GetLevel(idx);
            Handles.color = auth.GizmoColor;
            Vector3 prev = auth.transform.position;
            var first = true;
            for (var i = 0; i < lv.ForkCount; i++)
            {
                var p = auth.GetForkPosition(i);
                if (!first)
                    Handles.DrawDottedLine(prev, p, 4f);
                first = false;
                prev = p;
                Handles.SphereHandleCap(0, p, Quaternion.identity, 0.32f, EventType.Repaint);
                Handles.Label(p + Vector3.up * 0.35f, $"Lv{auth.LevelId}  Step {i + 1}/{lv.ForkCount}");
            }
        }
    }
}
#endif
