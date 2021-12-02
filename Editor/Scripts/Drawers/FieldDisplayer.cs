using UnityEditor;
using UnityEngine;
using RealityProgrammer.OverseerInspector.Editors.Utility;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers {
    public class FieldDisplayer : BaseFieldPrimaryDrawer {
        public override void DrawLayout() {
            if (!AssociatedMember.LastValidation)
                return;

            DrawAllChildsLayout();

            BeginHandleReadonly();
            BeginHandleFieldAssignCallback();
            EditorGUILayout.PropertyField(AssociatedMember.Property);
            EndHandleFieldAssignCallback();
            EndHandleReadonly();
        }
    }
}