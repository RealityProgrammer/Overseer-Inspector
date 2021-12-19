using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Runtime.Drawers;
using RealityProgrammer.OverseerInspector.Editors.Utility;
using RealityProgrammer.OverseerInspector.Editors.Attributes;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers {
    [BindDrawerTo(typeof(ShaderPropertyAttribute))]
    public class ShaderPropertyAttributeDrawer : BasePrimaryFieldDrawer {
        ShaderPropertyAttribute underlyingAttr;

        FieldInfo field;
        PropertyInfo property;

        bool validate = true;

        public override void Initialize() {
            underlyingAttr = (ShaderPropertyAttribute)AssociatedAttribute;

            if (AssociatedMember.ReflectionCache.UnderlyingField.FieldType != typeof(string)) {
                Debug.LogWarning("ShaderPropertyAttribute can only be applied to field type of string");

                validate = false;
                return;
            }

            ReflectionUtilities.ObtainMemberInfoFromArgument(AssociatedObject.targetObject.GetType(), underlyingAttr.Argument, out field, out property, out _);

            if (field != null) {
                if (!field.FieldType.IsOrSubclassOf(typeof(Renderer))) {
                    field = null;
                }
            } else if (property != null) {
                if (!property.PropertyType.IsOrSubclassOf(typeof(Renderer))) {
                    property = null;
                }
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
                    if (field != null) {
                        var rendererInstance = field.GetValue(AssociatedObject.targetObject) as Renderer;

                        DoShaderPropertyDropdownButton(AssociatedMember.Property, rendererInstance);
                    } else if (property != null) {
                        var rendererInstance = property.GetValue(AssociatedObject.targetObject) as Renderer;

                        DoShaderPropertyDropdownButton(AssociatedMember.Property, rendererInstance);
                    } else {
                        EditorGUILayout.PropertyField(AssociatedMember.Property);
                    }
                } else {
                    EditorGUILayout.PropertyField(AssociatedMember.Property);
                }

                EndHandleFieldAssignCallback();
            }
            EndHandleReadonly();
        }

        void DoShaderPropertyDropdownButton(SerializedProperty property, Renderer renderer) {
            var label = new GUIContent(property.displayName);

            if (renderer == null) {
                EditorGUI.BeginDisabledGroup(true);

                EditorGUILayout.LabelField(label);
                EditorGUI.DropdownButton(EditorGUI.PrefixLabel(GUILayoutUtility.GetLastRect(), label), new GUIContent("E"), FocusType.Passive);

                EditorGUI.EndDisabledGroup();
            } else {
                EditorGUI.BeginDisabledGroup(renderer.sharedMaterial == null);

                EditorGUILayout.LabelField(label);

                var oldValue = property.stringValue;

                if (EditorGUI.DropdownButton(EditorGUI.PrefixLabel(GUILayoutUtility.GetLastRect(), label), new GUIContent(property.stringValue), FocusType.Passive)) {
                    GenericMenu menu = new GenericMenu();

                    var shader = renderer.sharedMaterial.shader;
                    
                    for (int i = 0; i < shader.GetPropertyCount(); i++) {
                        string name = shader.GetPropertyName(i);

                        menu.AddItem(new GUIContent(name), oldValue == name, () => {
                            property.stringValue = name;
                            property.serializedObject.ApplyModifiedProperties();
                        });
                    }

                    menu.ShowAsContext();
                }

                EditorGUI.EndDisabledGroup();
            }
        }
    }
}