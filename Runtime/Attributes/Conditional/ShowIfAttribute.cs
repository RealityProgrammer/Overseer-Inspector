using System;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Validation {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class ShowIfAttribute : OverseerConditionalAttribute {
        public string Argument { get; private set; }

        public ShowIfAttribute(string argument) {
            Argument = argument;
        }
    }
}