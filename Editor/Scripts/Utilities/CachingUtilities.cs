using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Runtime;
using RealityProgrammer.OverseerInspector.Editors.Attributes;
using RealityProgrammer.OverseerInspector.Editors.Validators;
using RealityProgrammer.OverseerInspector.Runtime.Drawers;
using RealityProgrammer.OverseerInspector.Runtime.Validation;

namespace RealityProgrammer.OverseerInspector.Editors.Utility {
    public static class CachingUtilities {
        public static Type UnityEngineObjectType = typeof(UnityEngine.Object);
        public static Type ConditionalValidateRetType = typeof(OperationReturnContext);

        // Collection of conditional validate type with it's correspond validate method
        private static readonly Dictionary<Type, Type> _conditionalValidators;
        static CachingUtilities() {
            _conditionalValidators = new Dictionary<Type, Type>();

            var validators = TypeCache.GetTypesWithAttribute<ConditionalConnectAttribute>();

            var baseCondVT = typeof(BaseConditionalValidator);
            foreach (var validator in validators) {
                if (validator.IsSubclassOf(baseCondVT)) {
                    var target = validator.GetCustomAttribute<ConditionalConnectAttribute>().ConditionalType;

                    if (!_conditionalValidators.ContainsKey(target)) {
                        _conditionalValidators[target] = validator;
                    } else {
                        Debug.LogWarning("Multiple validator type for conditional attribute of type '" + target.FullName + "'");
                    }
                } else {
                    Debug.LogWarning("Conditional Validator of type '" + validator.FullName + "' must inherit '" + baseCondVT.FullName + "'");
                }
            }
        }

        public static BaseConditionalValidator GetConditionalValidatorInstance(Type conditionalType) {
            if (_conditionalValidators.TryGetValue(conditionalType, out var output)) {
                var instance = (BaseConditionalValidator)Activator.CreateInstance(output);

                return instance;
            }

            return null;
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

        private static Dictionary<SerializedObject, List<OverseerInspectingMember>> _allMembers = new Dictionary<SerializedObject, List<OverseerInspectingMember>>();

        public static ReadOnlyCollection<OverseerInspectingMember> RetrieveInspectingMembers(SerializedObject serializedObject) {
            if (_allMembers.TryGetValue(serializedObject, out var cache)) {
                return new ReadOnlyCollection<OverseerInspectingMember>(cache);
            }

            var all = new List<OverseerInspectingMember>();
            var reflectionUnits = RetrieveReflectionUnits(serializedObject.targetObject.GetType());

            using (SerializedProperty iterator = serializedObject.GetIterator()) {
                List<OverseerInspectingMember> output = new List<OverseerInspectingMember>();

                if (iterator.NextVisible(true)) {                       // This one is required by unity for a very understandable reason
                    while (iterator.NextVisible(false)) {       // use while instead of do..while to skip over the m_Script property
                        if (iterator.type == "ArraySize")
                            continue;

                        all.Add(OverseerInspectingMember.Create(iterator.Copy(), reflectionUnits[iterator.name], serializedObject.targetObject));
                    }
                }
            }

            foreach (var pair in reflectionUnits) {
                if (pair.Value.Type == ReflectionTargetType.Field) continue;

                all.Add(OverseerInspectingMember.Create(null, pair.Value, serializedObject.targetObject));
            }

            _allMembers[serializedObject] = all;
            return new ReadOnlyCollection<OverseerInspectingMember>(all);
        }

        #region Reflection Caches
        private static readonly Dictionary<Type, Dictionary<string, ReflectionCacheUnit>> _reflectionUnitStorage = new Dictionary<Type, Dictionary<string, ReflectionCacheUnit>>();

        public static Dictionary<string, ReflectionCacheUnit> RetrieveReflectionUnits(Type type) {
            if (_reflectionUnitStorage.TryGetValue(type, out var dict)) {
                return dict;
            }

            // Imagine if iterator method can have ref keyword, we won't have to resize this Dictionary when it got full
            dict = new Dictionary<string, ReflectionCacheUnit>();

            foreach (var unit in RetrieveAllFieldUnits(type)) {
                dict.Add(unit.Name, unit);
            }

            foreach (var unit in RetrieveAllMethodUnits(type)) {
                dict.Add(unit.Name, unit);
            }

            foreach (var unit in RetrieveAllPropertyUnits(type)) {
                dict.Add(unit.Name, unit);
            }

            _reflectionUnitStorage.Add(type, dict);
            return dict;
        }

        internal static IEnumerable<ReflectionCacheUnit> RetrieveAllFieldUnits(Type type) {
            if (type == null)
                yield break;

            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)) {
                if (field.GetCustomAttribute<ObsoleteAttribute>() != null) continue;

				yield return ReflectionCacheUnit.Create(field);
            }

            var baseType = type.BaseType;
            while (baseType != null) {
                foreach (FieldInfo field in baseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
                    if (field.GetCustomAttribute<ObsoleteAttribute>() != null) continue;

                    if (field.IsPrivate) {
                        if (field.GetCustomAttribute<SerializeField>() == null) {
                            continue;
                        }
                    }

                    yield return ReflectionCacheUnit.Create(field);
                }

                baseType = baseType.BaseType;
            }
        }

        internal static IEnumerable<ReflectionCacheUnit> RetrieveAllMethodUnits(Type type) {
            if (type == null) yield break;

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)) {
                if (method.GetCustomAttribute<ObsoleteAttribute>() != null) continue;
                if (method.GetParameters().Length != 0) continue;

                if (method.GetCustomAttributes<BaseOverseerAttribute>().Any()) {
                    yield return ReflectionCacheUnit.Create(method);
                }
            }

            var baseType = type.BaseType;
            while (baseType != null) {
                foreach (MethodInfo method in baseType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)) {
                    if (method.GetCustomAttribute<ObsoleteAttribute>() != null) continue;
                    if (method.GetParameters().Length != 0) continue;

                    if (method.GetCustomAttributes<BaseOverseerAttribute>().Any()) {
                        yield return ReflectionCacheUnit.Create(method);
                    }
                }

                baseType = baseType.BaseType;
            }
        }

        internal static IEnumerable<ReflectionCacheUnit> RetrieveAllPropertyUnits(Type type) {
            if (type == null) yield break;

            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)) {
                if (property.GetCustomAttribute<ObsoleteAttribute>() != null) continue;

                if (property.GetCustomAttributes<BaseOverseerAttribute>().Any()) {
                    yield return ReflectionCacheUnit.Create(property);
                }
            }

            var baseType = type.BaseType;
            while (baseType != null) {
                foreach (PropertyInfo property in baseType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)) {
                    if (property.GetCustomAttribute<ObsoleteAttribute>() != null) continue;

                    if (property.GetCustomAttributes<BaseOverseerAttribute>().Any()) {
                        yield return ReflectionCacheUnit.Create(property);
                    }
                }

                baseType = baseType.BaseType;
            }
        }
        #endregion
    }
}