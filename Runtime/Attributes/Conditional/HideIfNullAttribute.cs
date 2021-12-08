using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Validation {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class HideIfNullAttribute : OverseerConditionalAttribute {
        public string FieldName { get; private set; }

        public HideIfNullAttribute(string field) {
            FieldName = field;
        }
    }
}