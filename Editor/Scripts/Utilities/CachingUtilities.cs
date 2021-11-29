using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Runtime;
using RealityProgrammer.OverseerInspector.Editors.Attributes;
using RealityProgrammer.OverseerInspector.Runtime.Miscs;
using RealityProgrammer.OverseerInspector.Runtime.Validation;

namespace RealityProgrammer.OverseerInspector.Editors.Utility {
    public static class CachingUtilities {
        public static Type UnityEngineObjectType = typeof(UnityEngine.Object);

        // Collection of conditional validate type with it's correspond validate method
        private static readonly Dictionary<Type, Func<OverseerConditionalAttribute, object, bool>> _conditionalValidateMethods;
        static CachingUtilities() {
            _conditionalValidateMethods = new Dictionary<Type, Func<OverseerConditionalAttribute, object, bool>>();

            var validationMethods = TypeCache.GetMethodsWithAttribute<ConditionalConnectAttribute>();

            foreach (var method in validationMethods) {
                if (method.IsStatic && method.ReturnType == typeof(bool)) {
                    var parameters = method.GetParameters();

                    if (parameters.Length == 2) {
                        if (parameters[0].ParameterType == typeof(OverseerConditionalAttribute) && parameters[1].ParameterType == typeof(object)) {
                            var attr = method.GetCustomAttribute<ConditionalConnectAttribute>();

                            _conditionalValidateMethods[attr.ConditionalType] = (Func<OverseerConditionalAttribute, object, bool>)method.CreateDelegate(typeof(Func<OverseerConditionalAttribute, object, bool>));
                        }
                    }
                }
            }
        }
        

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

        #region Field Caches
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

        public static IEnumerable<OverseerConditionalAttribute> GetAllValidationAttribute(FieldInfo field) {
            var attrs = field.GetCustomAttributes();

            foreach (var attr in attrs) {
                if (attr is OverseerConditionalAttribute validation) {
                    yield return validation;
                }
            }
        }
        #endregion

        #region Method Data Caches
        public sealed class MethodButtonCache {
            public MethodInfo Method { get; private set; }
            public MethodButtonAttribute MethodButton { get; private set; }
            public ParameterInfo[] Parameters { get; private set; }
            public bool UseParameter => Parameters.Length > 0;
            public bool IsParameterFoldout { get; set; }

            public MethodButtonHandler Handler { get; private set; }

            internal MethodButtonCache(MethodInfo mtd, MethodButtonAttribute attr) {
                Method = mtd;
                MethodButton = attr;

                Parameters = mtd.GetParameters();
            }

            private static readonly HashSet<Type> serializableTypes = new HashSet<Type>() {
                typeof(Vector2), typeof(Vector3), typeof(Vector4), typeof(Quaternion),
                typeof(Vector2Int), typeof(Vector3Int),
                typeof(Rect), typeof(RectInt), typeof(Bounds), typeof(BoundsInt),
                typeof(Matrix4x4),
                typeof(Color), typeof(Color32),
                typeof(LayerMask),

                typeof(string),
                
                typeof(AnimationCurve), typeof(Gradient), typeof(RectOffset), typeof(GUIStyle)
            };
            private static readonly Type unityObjectType = typeof(UnityEngine.Object);
            public bool Validate() {
                if (Method.IsGenericMethod || Method.IsGenericMethodDefinition) {
                    Debug.LogWarning("MethodButtonAttribute cannot be used for methods with generic definition or generic parameter(s).");

                    return false;
                }

                foreach (var parameter in Parameters) {
                    if (!IsTypeSupported(parameter.ParameterType)) {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("MethodButtonAttribute cannot be used for methods with invalid parameter. Informations:");
                        sb.Append("Method name: ").AppendLine(Method.Name);
                        sb.Append("Method declaring type: ").AppendLine(Method.DeclaringType.AssemblyQualifiedName);
                        sb.Append("Invalid parameter: ").AppendLine(parameter.Name);

                        return false;
                    }
                }

                return true;
            }

            public void Initialize() {
                switch (MethodButton.InvokeStyle) {
                    case MethodInvocationStyle.Compiled:
                        Debug.LogWarning("MethodButtonAttribute with invocation type of Compiled is not supported yet");
                        break;

                    case MethodInvocationStyle.ReflectionInvoke:
                    default:
                        Handler = new MethodButtonReflectionInvokeHandler(this);
                        Handler.Initialize();
                        break;
                }
            }

            private static bool IsTypeSupported(Type type) {
                if (serializableTypes.Contains(type))
                    return true;

                if (type.IsPrimitive)
                    return true;

                if (!type.IsAbstract && !type.IsSealed && (type.IsSubclassOf(unityObjectType) || type == unityObjectType))
                    return true;

                return false;
            }
        }

        private static Dictionary<Type, List<MethodButtonCache>> _methodButtonCaches = new Dictionary<Type, List<MethodButtonCache>>();

        public static List<MethodButtonCache> GetAllMethodButtonCache(Type type) {
            if (type == null)
                return null;

            if (_methodButtonCaches.TryGetValue(type, out var caches)) {
                return caches;
            }

            caches = new List<MethodButtonCache>();
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)) {
                var attr = method.GetCustomAttribute<MethodButtonAttribute>();

                if (attr != null) {
                    var cache = new MethodButtonCache(method, attr);

                    if (cache.Validate()) {
                        cache.Initialize();
                        caches.Add(cache);
                    }
                }
            }

            _methodButtonCaches.Add(type, caches);
            return caches;
        }
        #endregion

        #region Conditional Caches
        public static bool ValidateAttribute(OverseerConditionalAttribute conditional, object target) {
            if (conditional == null)
                return true;

            if (_conditionalValidateMethods.TryGetValue(conditional.GetType(), out var method)) {
                return method.Invoke(conditional, target);
            }

            return true;
        }
        #endregion
    }
}