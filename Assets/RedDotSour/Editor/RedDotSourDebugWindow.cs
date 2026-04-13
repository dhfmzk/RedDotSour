using System.Collections.Generic;
using RedDotSour.Core;
using UnityEditor;
using UnityEngine;

namespace RedDotSour.Editor
{
    public class RedDotSourDebugWindow : EditorWindow
    {
        private Vector2 _scrollPos;
        private readonly Dictionary<string, bool> _foldouts = new();
        private double _lastRepaintTime;
        private const double RepaintInterval = 0.25; // 4Hz

        [MenuItem("Tools/RedDotSour/Debug Window")]
        public static void Open()
        {
            GetWindow<RedDotSourDebugWindow>("RedDotSour Debug");
        }

        private void OnEnable()
        {
            EditorApplication.update += this.OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= this.OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (!Application.isPlaying) return;

            if (EditorApplication.timeSinceStartup - this._lastRepaintTime > RepaintInterval)
            {
                this._lastRepaintTime = EditorApplication.timeSinceStartup;
                this.Repaint();
            }
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Play 모드에서만 사용할 수 있습니다.", MessageType.Info);
                return;
            }

            var instances = RedDotSourRegistry.Instances;
            if (instances.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "등록된 RedDotSour 인스턴스가 없습니다.\nnew RedDotSour<T>() 호출 시 자동 등록됩니다.",
                    MessageType.Warning);
                return;
            }

            this._scrollPos = EditorGUILayout.BeginScrollView(this._scrollPos);

            for (var i = 0; i < instances.Count; i++)
            {
                this.DrawInstance(i, instances[i]);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawInstance(int index, IRedDotSourInstance instance)
        {
            EditorGUILayout.LabelField($"Instance #{index}", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            foreach (var container in instance.Containers)
            {
                this.DrawContainer(container.CategoryName, container);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(8);
        }

        private void DrawContainer(string categoryName, IRedDotContainer container)
        {
            if (!this._foldouts.ContainsKey(categoryName))
            {
                this._foldouts[categoryName] = false;
            }

            var countOn = container.CountOn();
            var persistence = container as IRedDotContainerPersistence;
            var dirtyCount = persistence?.DirtyCount ?? 0;
            var header = $"{categoryName}  [On: {countOn} | Dirty: {dirtyCount}]";

            this._foldouts[categoryName] = EditorGUILayout.Foldout(
                this._foldouts[categoryName], header, true);

            if (!this._foldouts[categoryName]) return;

            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("IsOnAny", container.IsOnAny().ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CountOn", countOn.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("DirtyCount", dirtyCount.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Clear All", GUILayout.Width(100)))
            {
                container.ClearAll();
            }

            if (GUILayout.Button("Clear Dirty", GUILayout.Width(100)))
            {
                persistence?.ClearDirty();
            }

            EditorGUI.indentLevel--;
        }
    }
}
