using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Runtime;
using RealityProgrammer.OverseerInspector.Runtime.Validation;

namespace RealityProgrammer.OverseerInspector.Editors.Utility {
    public static class CachingUtilities {
        public static Type UnityEngineObjectType = typeof(UnityEngine.Object);

        #region Qualification
        private static HashSet<Type> _inspectorEnabledTypes = new HashSet<Type>();
        public static bool CheckOverseerQualified<T>() where T : UnityEngine.Object {
            var type = typeof(T);

            if (_inspectorEnabledTypes.Contains(type)) {
                return true;
            } else {
                if (type.GetCustomAttribute<EnableOverseerInspectorAttribute>() == null) {
                    return false;
                } else {
                    _inspectorEnabledTypes.Add(type);
                    return true;
                }
            }
        }
        public static bool CheckOverseerQualified(Type type) {
            if (!type.IsSubclassOf(UnityEngineObjectType))
                return false;

            if (_inspectorEnabledTypes.Contains(type)) {
                return true;
            } else {
                if (type.GetCustomAttribute<EnableOverseerInspectorAttribute>() == null) {
                    return false;
                } else {
                    _inspectorEnabledTypes.Add(type);
                    return true;
                }
            }
        }
        #endregion

        #region Data Caches
        private static Dictionary<SerializedObject, List<SerializedFieldContainer>> _allFieldsWithChildren = new Dictionary<SerializedObject, List<SerializedFieldContainer>>();
        private static Dictionary<SerializedObject, List<SerializedFieldContainer>> _allFields = new Dictionary<SerializedObject, List<SerializedFieldContainer>>();

        public static List<SerializedFieldContainer> GetAllCachedFields(SerializedObject serializedObject, bool enterChildren) {
            if (enterChildren) {
                if (_allFieldsWithChildren.TryGetValue(serializedObject, out var cache)) {
                    return cache;
                }

                var all = serializedObject.GetAllSerializedProperties(true);
                _allFieldsWithChildren[serializedObject] = all;
                return all;
            } else {
                if (_allFields.TryGetValue(serializedObject, out var cache)) {
                    return cache;
                }

                var all = serializedObject.GetAllSerializedProperties(false);
                _allFields[serializedObject] = all;
                return all;
            }
        }

        public static IEnumerable<ConditionalValidationAttribute> GetAllValidationAttribute(FieldInfo field) {
            var attrs = field.GetCustomAttributes();

            foreach (var attr in attrs) {
                if (attr is ConditionalValidationAttribute validation) {
                    yield return validation;
                }
            }
        }
        #endregion
    }
}