using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using RealityProgrammer.OverseerInspector.Runtime.Miscs;
using RealityProgrammer.OverseerInspector.Editors.Utility;

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

        public void BeginHandleFieldAssignCallback() {
            EditorGUI.BeginChangeCheck();
        }

        public void EndHandleFieldAssignCallback() {
            if (EditorGUI.EndChangeCheck()) {
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