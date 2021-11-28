using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RealityProgrammer.OverseerInspector.Editors.Utility {
    public static class SerializationUtilities {
        internal const BindingFlags DefaultFieldFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

        internal static List<SerializedFieldContainer> GetAllSerializedProperties(this SerializedObject serializedObject, bool enterChildren = false) {
            using (SerializedProperty iterator = serializedObject.GetIterator()) {
                List<SerializedFieldContainer> output = new List<SerializedFieldContainer>();

                if (iterator.NextVisible(true)) {                       // This one is required by unity for a very understandable reason
                    while (iterator.NextVisible(enterChildren)) {       // use while instead of do..while to skip over the m_Script property
                        if (iterator.type == "ArraySize")
                            continue;

                        output.Add(SerializedFieldContainer.Create(iterator.Copy()));
                    }
                }

                return output;
            }
        }

        public static FieldInfo GetFieldInfo(this SerializedProperty property) {
            var so = property.serializedObject;
            string[] path = property.propertyPath.Split('.');
            object target = so.targetObject;

            var targetType = target.GetType();

            FieldInfo ret = targetType.GetField(path[0], DefaultFieldFlags);

            while (ret == null && targetType.BaseType != null) {
                targetType = targetType.BaseType;
                ret = targetType.GetField(path[0], DefaultFieldFlags);
            }

            for (int i = 1; i < path.Length; i++) {
                var getValue = ret.GetValue(target);
                ret = getValue.GetType().GetField(path[i], DefaultFieldFlags);

                while (ret == null && targetType.BaseType != null) {
                    targetType = targetType.BaseType;
                    ret = targetType.GetField(path[0], DefaultFieldFlags);
                }

                target = getValue;
            }

            return ret;
        }
    }
}