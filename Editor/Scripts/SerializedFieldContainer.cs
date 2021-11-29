using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Editors.Utility;
using RealityProgrammer.OverseerInspector.Runtime.Drawers;
using RealityProgrammer.OverseerInspector.Runtime.Drawers.Group;
using RealityProgrammer.OverseerInspector.Runtime;
using RealityProgrammer.OverseerInspector.Runtime.Validation;

namespace RealityProgrammer.OverseerInspector.Editors {
    public sealed class SerializedFieldContainer {
        public SerializedProperty Property { get; private set; }
        public FieldInfo UnderlyingField { get; private set; }

        public bool LastValidation { get; private set; }

        public BasePrimaryDrawerAttribute PrimaryDrawerAttribute { get; private set; }
        public List<AdditionDrawerAttribute> Additionals { get; private set; }
        public List<OverseerBeginGroupAttribute> BeginGroups { get; private set; }
        public int EndGroupCount { get; private set; }

        private SerializedFieldContainer(SerializedProperty property, FieldInfo underlying) {
            Property = property;
            UnderlyingField = underlying;

            RetrieveAllOverseerAttributes();
        }

        private void RetrieveAllOverseerAttributes() {
            var allAttributes = UnderlyingField.GetCustomAttributes<Attribute>();
            foreach (var attr in allAttributes) {
                if (attr is BaseOverseerAttribute overseerAttr) {
                    switch (overseerAttr) {
                        case AdditionDrawerAttribute addition:
                            if (Additionals == null)
                                Additionals = new List<AdditionDrawerAttribute>();

                            Additionals.Add(addition);
                            break;

                        case BasePrimaryDrawerAttribute primary:
                            PrimaryDrawerAttribute = primary;
                            break;

                        case OverseerConditionalAttribute _:
                            break;

                        case OverseerBeginGroupAttribute beginGroup:
                            if (BeginGroups == null)
                                BeginGroups = new List<OverseerBeginGroupAttribute>();

                            BeginGroups.Add(beginGroup);
                            break;

                        case EndGroupAttribute end:
                            EndGroupCount++;
                            break;

                        default:
                            Debug.LogWarning("Unhandled attribute of type " + overseerAttr.GetType().AssemblyQualifiedName + ".");
                            break;
                    }
                }
            }
        }

        public void ForceCheckValidation() {
            var allValidates = CachingUtilities.GetAllValidationAttribute(UnderlyingField);

            LastValidation = true;

            foreach (var validation in allValidates) {
                bool res = CachingUtilities.ValidateAttribute(validation, Property.serializedObject.targetObject);

                if (!res) {
                    LastValidation = false;
                    break;
                }
            }
        }

        internal static SerializedFieldContainer Create(SerializedProperty property) {
            return new SerializedFieldContainer(property, property.GetFieldInfo());
        }

        public bool IsBackingField => UnderlyingField != null && UnderlyingField.Name.EndsWith(">k__BackingField");
        public bool IsEndGroup => EndGroupCount > 0;
    }
}