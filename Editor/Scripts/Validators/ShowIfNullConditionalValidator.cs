using UnityEngine;
using System.Reflection;
using System;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Editors.Attributes;
using RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora;
using RealityProgrammer.OverseerInspector.Runtime.Validation;

namespace RealityProgrammer.OverseerInspector.Editors.Validators {
    [ConditionalConnect(typeof(ShowIfNullAttribute))]
    public class ShowIfNullConditionalValidator : BaseConditionalValidator {
        public override bool Validate(ValidateContext context) {
            var type = context.ValidateTarget.GetType();
            var fieldName = ((HideIfNullAttribute)context.Attribute).FieldName;

            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) {
                var property = type.GetProperty(fieldName, BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null) {
                    object pvalue = property.GetValue(context.ValidateTarget);

                    if (pvalue is UnityEngine.Object puobj) {
                        return puobj == null;
                    }

                    return pvalue == null;
                }

                return true;
            }

            object fvalue = field.GetValue(context.ValidateTarget);
            if (fvalue is UnityEngine.Object fuobj) {
                return fuobj == null;
            }

            return fvalue == null;
        }
    }
}