using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Runtime.Drawers;
using RealityProgrammer.OverseerInspector.Editors.Utility;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers {
    [BindDrawerTo(typeof(SeperatorAttribute))]
    public class SeperatorDrawer : BaseAttributeDrawer {
        public static readonly Color FallbackColor = new Color32(0xCD, 0xCD, 0xCD, 0xCD);

        private Color? colorValue;

        private SeperatorAttribute underlying;

        public override void DrawLayout() {
            DrawAllChildsLayout();

            if (underlying == null) {
                underlying = (SeperatorAttribute)AssociatedAttribute;
            }

            if (!colorValue.HasValue) {
                if (OverseerEditorUtilities.TryHandleColorString(underlying.ColorParameter, out Color output)) {
                    colorValue = output;
                } else {
                    colorValue = FallbackColor;
                }
            }

            EditorGUILayout.Space(0);

            var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, underlying.Height));
            float decrement = rect.width / 2 * (1 - underlying.Normalize);

            EditorGUI.DrawRect(new Rect(rect.x + decrement, rect.y, rect.width - decrement, rect.height), colorValue.Value);
            EditorGUILayout.Space(0);
        }
    }
}