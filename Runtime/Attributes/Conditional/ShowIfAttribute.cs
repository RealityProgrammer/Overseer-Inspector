using System;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Validation {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class ShowIfAttribute : OverseerConditionalAttribute {
        public string Program { get; private set; }

        public ShowIfAttribute(string argument) {
            Program = argument;
        }
    }
}