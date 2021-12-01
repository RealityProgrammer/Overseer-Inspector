using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Editors.Utility;

namespace RealityProgrammer.OverseerInspector.Editors {
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public sealed class OverseerInspector : Editor {
        static bool detectLightTheme = false;

        public static OverseerInspector CurrentInspector { get; private set; }

        private bool _requestConstantRepaint = false;
        public void RequestConstantRepaint() {
            _requestConstantRepaint = true;
        }

        private ReadOnlyCollection<OverseerInspectingMember> allFields;

        private List<BaseDisplayable> _displayables;
        public int FieldPointer { get; set; } = 0;

        private void OnEnable() {
            if (CachingUtilities.CheckOverseerQualified(target.GetType())) {
                CheckLightTheme();
                CurrentInspector = this;

                allFields = CachingUtilities.RetrieveInspectingMembers(serializedObject);
                _displayables = OverseerEditorUtilities.AutoInitializeInspector(serializedObject);

                foreach (var field in allFields) {
                    field.ForceCheckValidation();
                }
            }
        }

        static void CheckLightTheme() {
            if (!detectLightTheme) {
                detectLightTheme = true;

                if (!EditorGUIUtility.isProSkin) {
                    Debug.LogWarning("Overseer Inspector: Light mode detected, Overseer Inspector Skin/Style are not compatible with Light Mode yet (and might never will, at least for the time being)");
                }
            }
        }

        public override void OnInspectorGUI() {
            if (CachingUtilities.CheckOverseerQualified(target.GetType())) {
                _requestConstantRepaint = false;

                serializedObject.Update();
                EditorGUI.BeginChangeCheck();

                foreach (var displayable in _displayables) {
                    if (displayable == null) {
                        Debug.LogWarning("Something went wrong. Overseer Inspector detected a null displayable element.");

                        continue;
                    }

                    displayable.DrawLayout();
                }

                //var allMethodBtns = CachingUtilities.GetAllMethodButtonCache(serializedObject.targetObject.GetType());
                //if (allMethodBtns.Count != 0) {
                //    foreach (var methodBtn in allMethodBtns) {
                //        var name = methodBtn.MethodButton.DisplayName ?? ObjectNames.NicifyVariableName(methodBtn.Method.Name);

                //        if (methodBtn.UseParameter) {
                //            var rect = EditorGUILayout.BeginVertical();

                //            EditorGUILayout.Space(2);
                //            if (Event.current.type == EventType.Repaint) {
                //                ((GUIStyle)"HelpBox").Draw(new Rect(rect.x - 15, rect.y, rect.width + 15, rect.height), false, false, false, false);
                //            }

                //            if (methodBtn.IsParameterFoldout = EditorGUILayout.Foldout(methodBtn.IsParameterFoldout, name)) {
                //                if (GUILayout.Button("Invoke")) {
                //                    methodBtn.Handler?.Invoke(serializedObject.targetObject);
                //                }

                //                methodBtn.Handler?.DrawLayoutParameters();
                //            }

                //            EditorGUILayout.Space(2);
                //            EditorGUILayout.EndVertical();
                //        } else {
                //            if (GUILayout.Button(name)) {
                //                methodBtn.Method.Invoke(serializedObject.targetObject, null);
                //            }
                //        }
                //    }
                //}

                if (EditorGUI.EndChangeCheck()) {
                    serializedObject.ApplyModifiedProperties();

                    foreach (var field in allFields) {
                        field.ForceCheckValidation();
                    }
                }
            } else {
                base.OnInspectorGUI();
            }
        }

        public override bool RequiresConstantRepaint() {
            return _requestConstantRepaint;
        }
    }
}