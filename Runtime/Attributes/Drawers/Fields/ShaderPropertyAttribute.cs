using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Drawers {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ShaderPropertyAttribute : BasePrimaryDrawerAttribute {
        public string Argument { get; private set; }

        public ShaderPropertyAttribute(string argument) {
            Argument = argument;
        }
    }
}