using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Drawers {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class MethodButtonAttribute : BaseMethodDrawerAttribute {
        public string DisplayName { get; set; }
        public float ButtonSize { get; set; } = 21;
    }
}