using System.Collections.Generic;
using System.IO;
using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Editors.Utility;
using RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora;

namespace RealityProgrammer.OverseerInspector.Editors {
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public sealed class OverseerInspector : Editor {
        static bool detectLightTheme = false;

        public static OverseerInspector CurrentInspector { get; private set; }

        private bool _requestConstantRepaint = false;
        public void RequestConstantRepaint() {
            _requestConstantRepaint = true;
        }

        private ReadOnlyCollection<OverseerInspectingMember> allMembers;

        private List<BaseDisplayable> _displayables;
        public int FieldPointer { get; set; } = 0;

        private void OnEnable() {
            if (CachingUtilities.CheckOverseerQualified(target.GetType())) {
                CheckLightTheme();
                CurrentInspector = this;

                allMembers = CachingUtilities.RetrieveInspectingMembers(serializedObject);
                _displayables = OverseerEditorUtilities.AutoInitializeInspector(serializedObject);
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

                foreach (var field in allMembers) {
                    field.ForceCheckValidation();
                }

                foreach (var displayable in _displayables) {
                    if (displayable == null) {
                        Debug.LogWarning("Something went wrong. Overseer Inspector detected a null displayable element.");

                        continue;
                    }

                    displayable.DrawLayout();
                }

                if (EditorGUI.EndChangeCheck()) {
                    serializedObject.ApplyModifiedProperties();
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