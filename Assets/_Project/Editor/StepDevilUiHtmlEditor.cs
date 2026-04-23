#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using StepDevil;

namespace StepDevil.Editor
{
    /// <summary>
    /// Inspector + menu commands to rebuild runtime UI from <see cref="StepDevilGame.UiBuild"/> to match
    /// <c>STEP_DEVIL_game.html</c> (title, world, game, result, truth, game over, complete). The level map
    /// screen is still built by <see cref="StepDevilLevelMapView"/> (road-style path) — unchanged.
    /// </summary>
    [CustomEditor(typeof(StepDevilGame))]
    public sealed class StepDevilUiHtmlEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("STEP_DEVIL_game.html layout", EditorStyles.boldLabel);
            if (GUILayout.Button("Rebuild entire UI hierarchy (destructive)", GUILayout.Height(28)))
            {
                var g = (StepDevilGame)target;
                if (EditorUtility.DisplayDialog("Step Devil",
                        "This destroys all children under StepDevilGame and rebuilds UI from code. " +
                        "The level map keeps the current road / StepDevilLevelMapView. Continue?",
                        "Rebuild", "Cancel"))
                {
                    Undo.RegisterFullObjectHierarchyUndo(g.gameObject, "Rebuild Step Devil UI");
                    g.EditorOnlyRebuildUi();
                    EditorSceneManager.MarkSceneDirty(g.gameObject.scene);
                }
            }

            EditorGUILayout.HelpBox(
                "Use menu: Step Devil → Rebuild UI when no StepDevilGame is selected. " +
                "Reference layout: STEP_DEVIL_game.html (same colors, copy, and flow).",
                MessageType.Info);
        }
    }

    /// <summary>Menu shortcuts for HTML-based UI rebuild.</summary>
    public static class StepDevilHtmlMenu
    {
        const string MenuRoot = "Step Devil/";

        [MenuItem(MenuRoot + "Rebuild UI (STEP_DEVIL_game.html layout)")]
        public static void RebuildUiFromMenu()
        {
            var g = Object.FindFirstObjectByType<StepDevilGame>(FindObjectsInactive.Include);
            if (g == null)
            {
                EditorUtility.DisplayDialog("Step Devil", "No StepDevilGame in the open scene(s).", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Step Devil",
                    "Rebuild UI from code? This removes existing canvas children under StepDevilGame.",
                    "Rebuild", "Cancel"))
                return;

            Undo.RegisterFullObjectHierarchyUndo(g.gameObject, "Rebuild Step Devil UI");
            g.EditorOnlyRebuildUi();
            EditorSceneManager.MarkSceneDirty(g.gameObject.scene);
        }

        [MenuItem(MenuRoot + "Select StepDevilGame in hierarchy")]
        public static void SelectGame()
        {
            var g = Object.FindFirstObjectByType<StepDevilGame>(FindObjectsInactive.Include);
            if (g == null)
            {
                EditorUtility.DisplayDialog("Step Devil", "No StepDevilGame found.", "OK");
                return;
            }

            Selection.activeGameObject = g.gameObject;
            EditorGUIUtility.PingObject(g.gameObject);
        }
    }

    /// <summary>
    /// <b>GameObject → Step Devil</b> — creates <see cref="StepDevilGame"/> plus the full canvas hierarchy (same as runtime <c>BuildUi()</c>).
    /// </summary>
    public static class StepDevilGameObjectMenu
    {
        const string GoMenu = "GameObject/Step Devil/";

        [MenuItem(GoMenu + "Step Devil Game (full UI)", false, 10)]
        static void CreateStepDevilGame(MenuCommand menuCommand)
        {
            var go = new GameObject("StepDevilGame");
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Step Devil Game");

            var game = Undo.AddComponent<StepDevilGame>(go);

            var so = new SerializedObject(game);
            var p = so.FindProperty("_useHierarchyFromScene");
            if (p != null)
                p.boolValue = false;
            so.ApplyModifiedProperties();

            Undo.RegisterFullObjectHierarchyUndo(go, "Build Step Devil UI");
            game.EditorOnlyRebuildUi();

            EnsureEventSystemForScene();
            EditorSceneManager.MarkSceneDirty(go.scene);
            Selection.activeGameObject = go;
        }

        [MenuItem(GoMenu + "Level path authoring (fork gizmos)", false, 11)]
        static void CreateLevelPathAuthoring(MenuCommand menuCommand)
        {
            var go = new GameObject("LevelPathAuthoring");
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Level Path Authoring");
            Undo.AddComponent<StepDevilLevelPathAuthoring>(go);
            EditorSceneManager.MarkSceneDirty(go.scene);
            Selection.activeGameObject = go;
        }

        static void EnsureEventSystemForScene()
        {
            if (Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) != null)
                return;

            var es = new GameObject("EventSystem");
            Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
            Undo.AddComponent<EventSystem>(es);
#if ENABLE_INPUT_SYSTEM
            Undo.AddComponent<InputSystemUIInputModule>(es);
#else
            Undo.AddComponent<StandaloneInputModule>(es);
#endif
            EditorSceneManager.MarkSceneDirty(es.scene);
        }
    }
}
#endif
