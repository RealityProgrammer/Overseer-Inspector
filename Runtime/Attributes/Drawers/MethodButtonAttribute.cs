using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Drawers {
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodButtonAttribute : BaseMethodDrawerAttribute {
        public string DisplayName { get; set; }
        public float ButtonSize { get; set; } = 21;
    }
}