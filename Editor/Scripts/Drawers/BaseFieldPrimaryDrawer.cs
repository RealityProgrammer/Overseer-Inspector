using UnityEditor;
using UnityEngine;
using RealityProgrammer.OverseerInspector.Runtime.Miscs;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers {
    public abstract class BaseFieldPrimaryDrawer : BasePrimaryDrawer {
        public void BeginHandleReadonly() {
            var attr = AssociatedMember.ReflectionCache.ReadonlyField;

            if (attr != null) {
                switch (attr.ExecutionMode) {
                    case ExecutionMode.Always:
                        EditorGUI.BeginDisabledGroup(true);
                        break;

                    case ExecutionMode.EditMode:
                        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
                        break;

                    case ExecutionMode.PlayMode:
                        EditorGUI.BeginDisabledGroup(Application.isPlaying);
                        break;
                }
            }
        }

        public void EndHandleReadonly() {
            var attr = AssociatedMember.ReflectionCache.ReadonlyField;

            if (attr != null) {
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}