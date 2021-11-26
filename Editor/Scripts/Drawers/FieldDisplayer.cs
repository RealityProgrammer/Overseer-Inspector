using System;
using UnityEditor;
using UnityEngine;

public class FieldDisplayer : BasePrimaryAttributeDrawer {
    public override void DrawLayout() {
        if (!AssociatedField.LastValidation)
            return;

        DrawAllChildsLayout();

        EditorGUILayout.PropertyField(AssociatedField.Property);
    }
}