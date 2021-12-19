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
                if (iterator.NextVisible(true)) {                       // This one is required by unity for a very understandable reason
                    while (iterator.NextVisible(false)) {                // use while instead of do..while to skip over the m_Script property
                        if (iterator.type == "ArraySize") continue;

                        all.Add(OverseerInspectingMember.Create(iterator.Copy(), reflectionUnits[iterator.name], serializedObject.targetObject));

                        #region WIP
                        //Debug.Log("Iterate through " + iterator.name);

                        //string[] pathTokens = iterator.propertyPath.Split('.');

                        //var containerObject = serializedObject.targetObject;
                        //var reflectionUnits = RetrieveReflectionUnits(containerObject.GetType());
                        //var fieldInfo = reflectionUnits[pathTokens[0]].UnderlyingField;

                        //HandleNestingProperty(iterator, containerObject, all);

                        //Debug.Log("Iterate through " + iterator.name);

                        //if (SerializedProperty.EqualContents(iterator, endProperty)) {
                        //    break;
                        //}

                        //if (iterator.type == "ArraySize") {             // Don't need this for now
                        //    continue;
                        //}

                        //var nextSibling = iterator.Copy();
                        //nextSibling.NextVisible(false);

                        //object containerObject = serializedObject.targetObject;
                        //string[] pathTokens = iterator.propertyPath.Split('.');

                        //var reflectionUnits = RetrieveReflectionUnits(containerObject.GetType());
                        //var fieldInfo = reflectionUnits[pathTokens[0]].UnderlyingField;

                        //if (iterator.propertyType == SerializedPropertyType.Generic) {
                        //    if (CachingUtilities.CheckOverseerQualified(fieldInfo.FieldType)) {

                        //    }

                        //    do {
                        //        Debug.Log(iterator.name + ", " + SerializedProperty.EqualContents(iterator, nextSibling));

                        //        if (SerializedProperty.EqualContents(iterator, nextSibling)) break;
                        //        //iterator.NextVisible(true);
                        //    } while (iterator.NextVisible(false));
                        //} else {
                        //    all.Add(OverseerInspectingMember.Create(iterator.Copy(), reflectionUnits[pathTokens[0]], serializedObject.targetObject));
                        //}

                        //all.AddRange(BuildInspectingMembersFromProperty(iterator, serializedObject.targetObject));

                        //Type parentType = serializedObject.targetObject.GetType();

                        //object containerObject = serializedObject.targetObject;
                        //var reflectionUnit = RetrieveReflectionUnits(containerObject.GetType());

                        //var fieldInfo = reflectionUnit[pathTokens[0]].UnderlyingField;

                        ////Debug.Log(0 + ": FieldInfo name: " + fieldInfo.Name + ". Container object: " + containerObject);

                        //if (iterator.propertyType == SerializedPropertyType.Generic && fieldInfo.FieldType.GetCustomAttribute<EnableOverseerInspectorAttribute>() != null) {
                        //    for (int i = 1; i < pathTokens.Length; i++) {
                        //        var fieldName = pathTokens[i];

                        //        containerObject = fieldInfo.GetValue(containerObject);
                        //        reflectionUnit = RetrieveReflectionUnits(containerObject.GetType());
                        //        fieldInfo = reflectionUnit[pathTokens[i]].UnderlyingField;

                        //        all.Add(OverseerInspectingMember.Create(iterator.Copy(), reflectionUnit[pathTokens[0]], containerObject));

                        //        //Debug.Log(i + ": FieldInfo name: " + fieldInfo.Name + ". Container object: " + containerObject);
                        //    }
                        //    //Debug.LogWarning("Nesting is not supported yet");
                        //    //all.Add(OverseerInspectingMember.Create(iterator.Copy(), reflectionUnit[pathTokens[0]], serializedObject.targetObject));
                        //} else {
                        //    all.Add(OverseerInspectingMember.Create(iterator.Copy(), reflectionUnit[pathTokens[0]], serializedObject.targetObject));
                        //}

                        //Debug.Log("Iterator's name: " + iterator.name + ". Path: " + iterator.propertyPath + ". Type: " + iterator.propertyType);
                        //all.Add(OverseerInspectingMember.Create(iterator.Copy(), reflectionUnits[iterator.name], serializedObject.targetObject));
                        #endregion
                    }
                }
            }

            var _reflectionUnits = RetrieveReflectionUnits(serializedObject.targetObject.GetType());
            foreach (var pair in _reflectionUnits) {
                if (pair.Value.Type == ReflectionTargetType.Field) continue;

                all.Add(OverseerInspectingMember.Create(null, pair.Value, serializedObject.targetObject));
            }

            _allMembers[serializedObject] = all;
            return new ReadOnlyCollection<OverseerInspectingMember>(all);
        }

        //private static int nestingLevel = 0;
        //internal static void HandleNestingProperty(SerializedProperty iterator, object container, List<OverseerInspectingMember> output) {
        //    string[] pathTokens = iterator.propertyPath.Split('.');
        //    var reflectionUnits = RetrieveReflectionUnits(container.GetType());

        //    var fieldInfo = reflectionUnits[pathTokens[nestingLevel]].UnderlyingField;

        //    if (iterator.propertyType == SerializedPropertyType.Generic && CheckOverseerQualified(fieldInfo.FieldType)) {
        //        container = fieldInfo.GetValue(container);

        //        output.Add(OverseerInspectingMember.Create(iterator.Copy(), reflectionUnits[pathTokens[nestingLevel]], container));

        //        nestingLevel++;
        //        foreach (var child in GetCopiesVisibleChildren(iterator)) {
        //            HandleNestingProperty(child, container, output);
        //        }
        //        nestingLevel--;
        //    } else {
        //        output.Add(OverseerInspectingMember.Create(iterator.Copy(), reflectionUnits[pathTokens[nestingLevel]], container));
        //    }
        //}

        public static ReadOnlyCollection<OverseerInspectingMember> RetrieveInspectingMembers(SerializedProperty property) {
            var all = new List<OverseerInspectingMember>();
            var fieldInfo = property.GetFieldInfo(property.serializedObject.targetObject.GetType());

            var reflectionUnits = RetrieveReflectionUnits(fieldInfo.FieldType);

            using (SerializedProperty iterator = property.Copy()) {
                do {
                    if (iterator.type == "ArraySize")
                        continue;

                    Debug.Log("Iterator: " + iterator.name);
                } while (iterator.Next(false));
            }

            foreach (var pair in reflectionUnits) {
                if (pair.Value.Type == ReflectionTargetType.Field) continue;

                Debug.Log("Field: " + pair.Key);
            }

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
                Debug.Log(unit.Name);
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

            foreach (var methodGroup in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(x => x.GetCustomAttribute<ObsoleteAttribute>() == null).GroupBy(x => x.Name)) {
                yield return ReflectionCacheUnit.Create(methodGroup.ToArray());
            }

            var baseType = type.BaseType;
            while (baseType != null) {
                foreach (var methodGroup in baseType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(x => x.GetCustomAttribute<ObsoleteAttribute>() == null && x.GetCustomAttributes<BaseOverseerAttribute>().Any()).GroupBy(x => x.Name)) {
                    yield return ReflectionCacheUnit.Create(methodGroup.ToArray());
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

        public static FieldInfo GetFieldInfo(this SerializedProperty property, Type type) {
            var path = property.propertyPath;

            Type parentType = type;
            FieldInfo fi = type.GetField(path);

            string[] perDot = path.Split('.');

            foreach (string fieldName in perDot) {
                fi = parentType.GetField(fieldName);

                if (fi != null)
                    parentType = fi.FieldType;
                else
                    return null;
            }

            return fi;
        }

        public static IEnumerable<SerializedProperty> GetCopiesVisibleChildren(this SerializedProperty serializedProperty) {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            nextSiblingProperty.NextVisible(false);

            if (currentProperty.NextVisible(true)) {
                do {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    yield return currentProperty.Copy();
                }
                while (currentProperty.NextVisible(false));
            }
        }
    }
}