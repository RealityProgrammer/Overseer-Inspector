using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Validation {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class ShowIfNullAttribute : OverseerConditionalAttribute {
        public string FieldName { get; private set; }

        public ShowIfNullAttribute(string field) {
            FieldName = field;
        }
    }
}