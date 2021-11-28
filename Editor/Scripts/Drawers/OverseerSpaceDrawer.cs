using UnityEditor;
using RealityProgrammer.OverseerInspector.Runtime.Drawers;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers {
    [BindDrawerTo(typeof(OverseerSpaceAttribute))]
    public class OverseerSpaceDrawer : BaseAttributeDrawer {
        private OverseerSpaceAttribute underlying;

        public override void DrawLayout() {
            if (underlying == null) {
                underlying = (OverseerSpaceAttribute)AssociatedAttribute;
            }

            EditorGUILayout.Space(underlying.Amount - 2);
        }
    }
}