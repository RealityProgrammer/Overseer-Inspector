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
            var field = target.GetType().GetField(FieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) {
                return true;
            }

            object value = field.GetValue(target);
            if (value is UnityEngine.Object uobj) {
                return uobj == null;
            }

            return value == null;
        }
    }
}