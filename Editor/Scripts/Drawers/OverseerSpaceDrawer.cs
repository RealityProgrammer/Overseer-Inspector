using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
