using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using RealityProgrammer.OverseerInspector.Runtime.Drawers;
using RealityProgrammer.OverseerInspector.Editors.Utility;
using RealityProgrammer.OverseerInspector.Editors.Attributes;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers {
    [BindDrawerTo(typeof(TagAttribute))]
    public class TagAttributeDrawer : BasePrimaryFieldDrawer {
        bool validate = true;

        public override void Initialize() {
            validate = AssociatedMember.Property.propertyType == SerializedPropertyType.String || (AssociatedMember.Property.isArray && AssociatedMember.ReflectionCache.UnderlyingField.FieldType.GetElementType() == typeof(string));

            if (!validate) {
                Debug.LogWarning("TagAttribute only valid on field with the type of string or string collection.");
                return;
            }

            if (AssociatedMember.Property.isArray) {
                EliminateNonExistTags(AssociatedMember.Property);
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
                    var label = new GUIContent(AssociatedMember.Property.displayName);

                    EditorGUILayout.LabelField(label);

                    var dropdownButtonRect = EditorGUI.PrefixLabel(GUILayoutUtility.GetLastRect(), label);
                    // EditorStyles.miniPullDown
                    string buttonName = ButtonName(AssociatedMember.Property, dropdownButtonRect);

                    if (EditorGUI.DropdownButton(EditorGUI.PrefixLabel(GUILayoutUtility.GetLastRect(), label), new GUIContent(buttonName), FocusType.Passive)) {
                        GenericMenu menu = new GenericMenu();

                        if (AssociatedMember.Property.isArray) {
                            foreach (var tag in InternalEditorUtility.tags) {
                                bool containTag = false;
                                int containIndex = -1;

                                for (int i = 0; i < AssociatedMember.Property.arraySize; i++) {
                                    if (AssociatedMember.Property.GetArrayElementAtIndex(i).stringValue == tag) {
                                        containTag = true;
                                        containIndex = i;
                                        break;
                                    }
                                }

                                var _tag = tag;
                                menu.AddItem(new GUIContent(tag), containTag, () => {
                                    if (containTag) {
                                        AssociatedMember.Property.DeleteArrayElementAtIndex(containIndex);
                                    } else {
                                        AssociatedMember.Property.GetArrayElementAtIndex(AssociatedMember.Property.arraySize++).stringValue = _tag;
                                    }

                                    //AssociatedMember.Property.stringValue = tag;
                                    AssociatedObject.ApplyModifiedProperties();
                                });
                            }

                            EliminateNonExistTags(AssociatedMember.Property);
                        } else {
                            var oldTag = AssociatedMember.Property.stringValue;

                            foreach (var tag in InternalEditorUtility.tags) {
                                menu.AddItem(new GUIContent(tag), tag == oldTag, () => {
                                    AssociatedMember.Property.stringValue = tag;
                                    AssociatedObject.ApplyModifiedProperties();
                                });
                            }
                        }

                        menu.ShowAsContext();
                    }
                }

                EndHandleFieldAssignCallback();
            }

            EndHandleReadonly();
        }

        void EliminateNonExistTags(SerializedProperty property) {
            for (int i = 0; i < property.arraySize; i++) {
                if (!InternalEditorUtility.tags.Contains(property.GetArrayElementAtIndex(i).stringValue)) {
                    property.DeleteArrayElementAtIndex(i);
                }
            }

            AssociatedObject.ApplyModifiedProperties();
        }

        string ButtonName(SerializedProperty property, Rect buttonRect) {
            if (property.isArray) {
                switch (property.arraySize) {
                    case 0: return "<Empty>";
                    case 1: return property.GetArrayElementAtIndex(0).stringValue;
                    default:
                        string fullname = property.GetArrayElementAtIndex(0).stringValue;

                        for (int i = 1; i < property.arraySize; i++) {
                            fullname += " - " + property.GetArrayElementAtIndex(i).stringValue;
                        }

                        float w = EditorStyles.miniPullDown.CalcSize(new GUIContent(fullname)).x;

                        if (buttonRect.width > w) {
                            return fullname;
                        }

                        return "<Multiple Values>";
                }
            } else {
                return property.stringValue;
            }
        }
    }
}