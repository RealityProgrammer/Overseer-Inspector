using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using RealityProgrammer.OverseerInspector.Runtime.Drawers.Group;
using RealityProgrammer.OverseerInspector.Editors.Attributes;
using RealityProgrammer.OverseerInspector.Editors.Miscs;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers.Group {
    [BindDrawerTo(typeof(BeginFoldoutGroupAttribute))]
    public class FoldoutGroupDrawer : BaseGroupAttributeDrawer {
        private AnimBool foldoutAnim;

        private readonly static GUIStyle boxStyle;
        private readonly static GUIStyle foldoutStyle;

        static FoldoutGroupDrawer() {
            boxStyle = new GUIStyle() {
                padding = new RectOffset(4, 4, 4, 4),
                border = new RectOffset(3, 3, 3, 3),
            };

            boxStyle.normal.background = Resources.Load<Texture2D>("Dark/group-foldout-box");

            foldoutStyle = new GUIStyle();

            foldoutStyle.normal.background = Resources.Load<Texture2D>("Dark/foldout-0");
            foldoutStyle.fixedWidth = 8;
            foldoutStyle.fixedHeight = 8;
        }

        public FoldoutGroupDrawer() {
            foldoutAnim = new AnimBool(false);

            foldoutAnim.valueChanged.AddListener(() => {
                OverseerInspector.CurrentInspector.RequestConstantRepaint();
            });
        }

        public const float HeaderSpace = 2;

        BeginFoldoutGroupAttribute underlyingAttr;

        public override void Initialize() {
            underlyingAttr = AssociatedAttribute as BeginFoldoutGroupAttribute;
        }

        public override void DrawLayout() {
            var rect = EditorGUILayout.BeginVertical();

            if (Event.current.type == EventType.Repaint) {
                var indentRect = EditorGUI.IndentedRect(rect);

                boxStyle.Draw(indentRect, false, false, false, false);
            }

            using (new EditorGUI.IndentLevelScope()) {
                EditorGUILayout.Space(HeaderSpace);

                DoFoldoutHeader();

                EditorGUILayout.Space(HeaderSpace);

                if (foldoutAnim.faded > 0.001f) {
                    EditorGUILayout.BeginFadeGroup(foldoutAnim.faded);
                    {
                        DrawAllChildsLayout();
                        EditorGUILayout.Space(2);
                    }
                    EditorGUILayout.EndFadeGroup();
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }

        private void DoFoldoutHeader() {
            int id = GUIUtility.GetControlID(FocusType.Passive);

            var old = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = underlyingAttr.FontStyle;

            EditorGUILayout.LabelField(underlyingAttr.Name);

            EditorStyles.label.fontStyle = old;

            var lastRect = GUILayoutUtility.GetLastRect();
            lastRect = EditorGUI.IndentedRect(lastRect);
            lastRect.x -= 14;

            var foldoutRect = new Rect(lastRect.x + 2, lastRect.y, lastRect.width - 2, lastRect.height);

            switch (Event.current.GetTypeForControl(id)) {
                case EventType.MouseDown:
                    if (Event.current.button == 0 && foldoutRect.Contains(Event.current.mousePosition)) {
                        GUIUtility.hotControl = id;
                        Event.current.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id) {
                        GUIUtility.hotControl = 0;

                        if (foldoutRect.Contains(Event.current.mousePosition)) {
                            foldoutAnim.target = !foldoutAnim.target;
                            Event.current.Use();
                        }
                    }
                    break;

                case EventType.Repaint:
                    var drawRect = new Rect(foldoutRect.x, lastRect.y + 5, 8, 8);

                    GUIUtility.RotateAroundPivot(foldoutAnim.faded * 90, drawRect.center);
                    foldoutStyle.Draw(drawRect, false, false, false, false);
                    GUIUtility.RotateAroundPivot(-foldoutAnim.faded * 90, drawRect.center);
                    break;
            }
        }
    }
}