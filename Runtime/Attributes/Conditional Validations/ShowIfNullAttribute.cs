using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Validation {
    public class ShowIfNullAttribute : ConditionalValidationAttribute {
        public string FieldName { get; private set; }

        public ShowIfNullAttribute(string field) {
            FieldName = field;
        }

        public override bool Validation(object target) {
            var type = target.GetType();

            var field = type.GetField(FieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) {
                var property = type.GetProperty(FieldName, BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null) {
                    object pvalue = property.GetValue(target);

                    if (pvalue is UnityEngine.Object puobj) {
                        return puobj == null;
                    }

                    return pvalue == null;
                }

                return true;
            }

            object fvalue = field.GetValue(target);
            if (fvalue is UnityEngine.Object fuobj) {
                return fuobj == null;
            }

            return fvalue == null;
        }
    }
}