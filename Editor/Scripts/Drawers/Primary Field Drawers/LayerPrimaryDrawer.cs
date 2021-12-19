using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Runtime.Drawers;
using RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora;
using RealityProgrammer.OverseerInspector.Editors.Utility;
using RealityProgrammer.OverseerInspector.Editors.Attributes;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers {
    [BindDrawerTo(typeof(LayerAttribute))]
    public class LayerPrimaryDrawer : BasePrimaryFieldDrawer {
        bool validate;

        LayerAttribute underlyingAttr;
        bool flag;

        public override void Initialize() {
            underlyingAttr = (LayerAttribute)AssociatedAttribute;

            switch (AssociatedMember.Property.propertyType) {
                case SerializedPropertyType.Integer:
                    validate = true;
                    flag = underlyingAttr.Flag;

                    int intV = AssociatedMember.Property.intValue;

                    if (flag) {
                        AssociatedMember.Property.intValue = EliminateNonExistLayers(intV);
                        AssociatedObject.ApplyModifiedProperties();
                    } else {
                        if (intV < 0 || intV >= 32 || string.IsNullOrEmpty(LayerMask.LayerToName(intV))) {
                            AssociatedMember.Property.intValue = 0;

                            AssociatedObject.ApplyModifiedProperties();
                        }
                    }
                    break;

                case SerializedPropertyType.String:
                    validate = true;
                    flag = false;

                    if (LayerMask.NameToLayer(AssociatedMember.Property.stringValue) == -1) {
                        AssociatedMember.Property.stringValue = "Default";
                    }

                    if (underlyingAttr.Flag) {
                        Debug.LogWarning("LayerAttribute: Flag doesn't work with field of string");
                    }
                    break;

                default:
                    validate = false;
                    Debug.LogWarning("LayerAttribute: Only works with field of type string or int. Cannot work with field of type: " + AssociatedMember.ReflectionCache.UnderlyingField.FieldType.FullName);
                    break;
            }
        }

        public override void DrawLayout() {
            if (!AssociatedMember.ConditionalCheck) {
                return;
            }

            DrawAllChildsLayout();

            if (BeginHandleReadonly()) {
                BeginHandleFieldAssignCallback();

                if (validate) {
                    var labelContent = new GUIContent(AssociatedMember.Property.displayName);

                    EditorGUILayout.LabelField(labelContent);
                    var dropdownButtonRect = EditorGUI.PrefixLabel(GUILayoutUtility.GetLastRect(), labelContent);

                    switch (AssociatedMember.Property.propertyType) {
                        case SerializedPropertyType.Integer:
                            var layerInt = AssociatedMember.Property.intValue;

                            if (!flag) {
                                if (EditorGUI.DropdownButton(dropdownButtonRect, new GUIContent(GetLayerButtonName(layerInt, false, dropdownButtonRect, out _)), FocusType.Passive)) {
                                    GenericMenu menu = new GenericMenu();

                                    for (int i = 0; i < 32; i++) {
                                        var _i = i;
                                        var name = LayerMask.LayerToName(i);

                                        if (string.IsNullOrEmpty(name)) continue;

                                        menu.AddItem(new GUIContent(name + " (" + i + ")"), layerInt == i, () => {
                                            AssociatedMember.Property.intValue = _i;
                                            AssociatedObject.ApplyModifiedProperties();
                                        });
                                    }

                                    menu.ShowAsContext();
                                }
                            } else {
                                if (EditorGUI.DropdownButton(dropdownButtonRect, new GUIContent(GetLayerButtonName(layerInt, true, dropdownButtonRect, out _)), FocusType.Passive)) {
                                    GenericMenu menu = new GenericMenu();

                                    menu.AddItem(new GUIContent("Nothing"), AssociatedMember.Property.intValue == 0, () => {
                                        AssociatedMember.Property.intValue = 0;
                                        AssociatedObject.ApplyModifiedProperties();
                                    });

                                    menu.AddItem(new GUIContent("Everything"), false, () => {
                                        AssociatedMember.Property.intValue = -1;
                                        AssociatedObject.ApplyModifiedProperties();
                                    });

                                    menu.AddSeparator(string.Empty);

                                    for (int i = 0; i < 32; i++) {
                                        var _i = i;
                                        var name = LayerMask.LayerToName(i);

                                        if (string.IsNullOrEmpty(name)) continue;

                                        menu.AddItem(new GUIContent(name + " (" + i + ")"), (layerInt & (1 << i)) != 0, () => {
                                            AssociatedMember.Property.intValue ^= 1 << _i;
                                            AssociatedObject.ApplyModifiedProperties();
                                        });
                                    }

                                    menu.ShowAsContext();

                                    AssociatedMember.Property.intValue = EliminateNonExistLayers(AssociatedMember.Property.intValue);
                                    AssociatedObject.ApplyModifiedProperties();
                                }
                            }
                            break;

                        case SerializedPropertyType.String:
                            var layerStr = AssociatedMember.Property.stringValue;

                            int layerIndex = string.IsNullOrEmpty(layerStr) ? -1 : LayerMask.NameToLayer(layerStr);

                            if (EditorGUI.DropdownButton(EditorGUI.PrefixLabel(GUILayoutUtility.GetLastRect(), labelContent), new GUIContent((layerIndex == -1 ? "Undefined Layer" : layerStr) + " (" + layerIndex + ")"), FocusType.Passive)) {
                                GenericMenu menu = new GenericMenu();

                                for (int i = 0; i < 32; i++) {
                                    var _i = i;
                                    var name = LayerMask.LayerToName(i);

                                    if (string.IsNullOrEmpty(name)) continue;

                                    menu.AddItem(new GUIContent(name + " (" + i + ")"), layerIndex == i, () => {
                                        AssociatedMember.Property.stringValue = name;
                                        AssociatedObject.ApplyModifiedProperties();
                                    });
                                }

                                menu.ShowAsContext();
                            }
                            break;
                    }
                } else {
                    EditorGUILayout.PropertyField(AssociatedMember.Property);
                }

                EndHandleFieldAssignCallback();
            }
            EndHandleReadonly();
        }

        private string GetLayerMenuName(int layer, out bool isInvalid) {
            var name = LayerMask.LayerToName(layer);

            if (string.IsNullOrEmpty(name)) {
                isInvalid = true;
                return "Undefined Layer (" + layer + ")";
            } else {
                isInvalid = false;
                return name + " (" + layer + ")";
            }
        }

        private string GetLayerButtonName(int layer, bool flag, Rect buttonRect, out bool isInvalid) {
            if (flag) {
                isInvalid = false;

                if (layer == 0) return "Nothing";

                string fullName = string.Empty;
                float currentWidth = 0;

                for (int i = 0; i < 32; i++) {
                    int bm = layer & (1 << i);

                    if (bm != 0) {
                        var layerName = LayerMask.LayerToName(i);
                        if (string.IsNullOrEmpty(layerName)) continue;

                        if (string.IsNullOrEmpty(fullName)) {
                            fullName += layerName;
                            currentWidth += EditorStyles.miniPullDown.CalcSize(new GUIContent(fullName)).x;
                        } else {
                            var n = " - " + layerName;
                            fullName += n;

                            currentWidth += EditorStyles.miniPullDown.CalcSize(new GUIContent(n)).x;

                            if (currentWidth > buttonRect.width) {
                                fullName = "<Multiple Layers>";
                                return fullName;
                            }
                        }
                    }
                }

                return fullName;
            } else {
                var name = LayerMask.LayerToName(layer);

                if (string.IsNullOrEmpty(name)) {
                    isInvalid = true;
                    return "Undefined Layer (" + layer + ")";
                } else {
                    isInvalid = false;
                    return name + " (" + layer + ")";
                }
            }
        }

        int EliminateNonExistLayers(int layer) {
            for (int i = 0; i < 32; i++) {
                if ((layer & (1 << i)) != 0) {
                    if (string.IsNullOrEmpty(LayerMask.LayerToName(i))) {
                        layer &= ~(1 << i);
                    }
                }
            }

            return layer;
        }
    }
}