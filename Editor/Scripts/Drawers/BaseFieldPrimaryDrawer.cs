using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using RealityProgrammer.OverseerInspector.Runtime.Miscs;
using RealityProgrammer.OverseerInspector.Editors.Utility;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers {
    public abstract class BaseFieldPrimaryDrawer : BasePrimaryDrawer {
        public bool BeginHandleReadonly() {
            var attr = AssociatedMember.ReflectionCache.ReadonlyField;

            if (attr != null) {
                if (Application.isPlaying) {
                    EditorGUI.BeginDisabledGroup(!attr.PMEditable);

                    return attr.PMVisibility;
                } else {
                    EditorGUI.BeginDisabledGroup(!attr.EMEditable);

                    return attr.EMVisibility;
                }
            }

            return true;
        }

        public void EndHandleReadonly() {
            var attr = AssociatedMember.ReflectionCache.ReadonlyField;

            if (attr != null) {
                EditorGUI.EndDisabledGroup();
            }
        }

        public void BeginHandleFieldAssignCallback() {
            EditorGUI.BeginChangeCheck();
        }

        public void EndHandleFieldAssignCallback() {
            if (EditorGUI.EndChangeCheck()) {
                if (AssociatedMember.ReflectionCache.FieldAssignCallback == null) return;

                var target = AssociatedMember.Target;
                var targetType = target.GetType();

                foreach (var callbackAttr in AssociatedMember.ReflectionCache.FieldAssignCallback) {
                    var arg = callbackAttr.CallbackArgument;

                    var property = targetType.GetProperty(arg, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty);
                    if (property == null) {
                        var method = targetType.GetMethod(arg, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty);

                        if (method != null) {
                            if (method.GetParameters().Length != 0) {
                                Debug.LogWarning("Cannot invoke callback of field " + AssociatedMember.ReflectionCache.UnderlyingField.Name + " because callback method contains parameter");
                                continue;
                            }

                            method.Invoke(target, null);
                        }
                    } else {
                        property.GetSetMethod(true).Invoke(target, new object[] { AssociatedMember.Property.GetBoxedValue() });
                    }
                }
            }
        }
    }
}