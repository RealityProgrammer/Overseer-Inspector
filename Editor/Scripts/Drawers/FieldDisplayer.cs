using UnityEditor;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers {
    public class FieldDisplayer : BasePrimaryAttributeDrawer {
        public override void DrawLayout() {
            if (!AssociatedField.LastValidation)
                return;

            DrawAllChildsLayout();

            EditorGUILayout.PropertyField(AssociatedField.Property);
        }
    }
}