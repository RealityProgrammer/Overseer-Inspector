using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnityEngine.Object), true)]
public sealed class OverseerInspector : Editor {
    static bool detectLightTheme = false;

    public static OverseerInspector CurrentInspector { get; private set; }

    private bool _requestConstantRepaint = false;
    public void RequestConstantRepaint() {
        _requestConstantRepaint = true;
    }

    private List<SerializedFieldContainer> allFields;
    public ReadOnlyCollection<SerializedFieldContainer> AllFields => new ReadOnlyCollection<SerializedFieldContainer>(allFields);

    private List<BaseDisplayable> _displayables;

    public int FieldPointer { get; set; } = 0;

    private void OnEnable() {
        if (CachingUtilities.CheckOverseerQualified(target.GetType())) {
            CheckLightTheme();
            CurrentInspector = this;

            allFields = CachingUtilities.GetAllCachedFields(serializedObject, false);
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
                displayable.DrawLayout();
            }

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
