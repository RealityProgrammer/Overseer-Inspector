using UnityEditor;
using UnityEngine;
using RealityProgrammer.OverseerInspector.Editors.Utility;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers {
    public class FieldDisplayer : BasePrimaryFieldDrawer {
        public override void DrawLayout() {
            if (!AssociatedMember.ConditionalCheck) {
                return;
            }

            DrawAllChildsLayout();

            if (BeginHandleReadonly()) {
                BeginHandleFieldAssignCallback();
                EditorGUILayout.PropertyField(AssociatedMember.Property);
                EndHandleFieldAssignCallback();
            }
            EndHandleReadonly();
        }
    }
}