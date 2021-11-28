using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Miscs {
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodButtonAttribute : BaseOverseerAttribute {
        public string DisplayName { get; set; }

        public MethodInvocationStyle InvokeStyle { get; set; }
    }

    public enum MethodInvocationStyle {
        ReflectionInvoke, Compiled,
    }
}