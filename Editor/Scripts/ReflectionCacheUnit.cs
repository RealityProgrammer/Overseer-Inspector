using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Editors.Utility;
using RealityProgrammer.OverseerInspector.Runtime.Drawers;
using RealityProgrammer.OverseerInspector.Runtime.Drawers.Group;
using RealityProgrammer.OverseerInspector.Runtime;
using RealityProgrammer.OverseerInspector.Runtime.Validation;
using RealityProgrammer.OverseerInspector.Runtime.Miscs;

namespace RealityProgrammer.OverseerInspector.Editors {
    public sealed class ReflectionCacheUnit {
        public ReflectionTargetType Type { get; private set; }

        public FieldInfo UnderlyingField { get; private set; }
        public MethodInfo[] UnderlyingMethods { get; private set; }
        public PropertyInfo UnderlyingProperty { get; private set; }

        public MethodInfo UnderlyingMethod => UnderlyingMethods[0];

        public BasePrimaryDrawerAttribute PrimaryDrawerAttribute { get; private set; }
        public List<AdditionDrawerAttribute> Additionals { get; private set; }
        public List<OverseerBeginGroupAttribute> BeginGroups { get; private set; }
        public int EndGroupCount { get; private set; }

        public List<OverseerConditionalAttribute> Conditions { get; private set; }

        public List<FieldAssignCallbackAttribute> FieldAssignCallback { get; private set; }
        public ReadonlyFieldAttribute ReadonlyField { get; private set; }

        private void InitializeAttributes(MemberInfo member) {
            var allAttributes = member.GetCustomAttributes<BaseOverseerAttribute>();
            foreach (var attr in allAttributes) {
                switch (attr) {
                    case AdditionDrawerAttribute addition:
                        if (Additionals == null)
                            Additionals = new List<AdditionDrawerAttribute>();

                        Additionals.Add(addition);
                        break;

                    case BasePrimaryDrawerAttribute primary:
                        PrimaryDrawerAttribute = primary;
                        break;

                    case OverseerConditionalAttribute conditional:
                        if (Conditions == null)
                            Conditions = new List<OverseerConditionalAttribute>();

                        Conditions.Add(conditional);
                        break;

                    case OverseerBeginGroupAttribute beginGroup:
                        if (BeginGroups == null)
                            BeginGroups = new List<OverseerBeginGroupAttribute>();

                        BeginGroups.Add(beginGroup);
                        break;

                    case EndGroupAttribute _:
                        EndGroupCount++;
                        break;

                    case ReadonlyFieldAttribute @readonly:
                        ReadonlyField = @readonly;
                        break;

                    case FieldAssignCallbackAttribute fac:
                        if (FieldAssignCallback == null)
                            FieldAssignCallback = new List<FieldAssignCallbackAttribute>();

                        FieldAssignCallback.Add(fac);
                        break;
                }
            }
        }
        internal static ReflectionCacheUnit Create(FieldInfo field) {
            if (field == null) {
                Debug.LogError("Overseer Inspector Internal: Trying to create an ReflectionCacheUnit with null underlying field");
                return null;
            }

            ReflectionCacheUnit unit = new ReflectionCacheUnit {
                Type = ReflectionTargetType.Field,
                UnderlyingField = field
            };
            unit.InitializeAttributes(unit.UnderlyingField);

            return unit;
        }
        internal static ReflectionCacheUnit Create(MethodInfo[] methods) {
            if (methods == null) {
                Debug.LogError("Overseer Inspector Internal: Trying to create an ReflectionCacheUnit with null underlying methods");
                return null;
            }

            ReflectionCacheUnit unit = new ReflectionCacheUnit {
                Type = ReflectionTargetType.Method,
                UnderlyingMethods = methods
            };
            unit.InitializeAttributes(unit.UnderlyingMethod);

            return unit;
        }
        internal static ReflectionCacheUnit Create(PropertyInfo property) {
            if (property == null) {
                Debug.LogError("Overseer Inspector Internal: Trying to create an ReflectionCacheUnit with null underlying property");
                return null;
            }

            ReflectionCacheUnit unit = new ReflectionCacheUnit {
                Type = ReflectionTargetType.Property,
                UnderlyingProperty = property
            };
            unit.InitializeAttributes(unit.UnderlyingProperty);

            return unit;
        }

        public bool CheckCondition(object target) {
            if (target == null || Conditions == null) {
                return true;
            }

            foreach (var condition in Conditions) {
                var ret = OverseerEditorUtilities.ValidateAttribute(condition, target);

                if (!ret) {
                    return false;
                }
            }

            return true;
        }

        public string Name {
            get {
                switch (Type) {
                    case ReflectionTargetType.Field: return UnderlyingField.Name;
                    case ReflectionTargetType.Property: return UnderlyingProperty.Name;
                    case ReflectionTargetType.Method: return UnderlyingMethod.Name;
                }

                return "???";
            }
        }

        public bool IsEndGroup => EndGroupCount > 0;
        public bool IsBackingField => Type == ReflectionTargetType.Field && UnderlyingField.Name.EndsWith(">k__BackingField");
    }

    public enum ReflectionTargetType {
        Field, Method, Property,
    }
}