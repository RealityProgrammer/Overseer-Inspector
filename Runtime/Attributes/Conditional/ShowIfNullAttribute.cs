using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Validation {
    public sealed class ShowIfNullAttribute : OverseerConditionalAttribute {
        public string FieldName { get; private set; }

        public ShowIfNullAttribute(string field) {
            FieldName = field;
        }
    }
}