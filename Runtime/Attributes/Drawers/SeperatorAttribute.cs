using System;

namespace RealityProgrammer.OverseerInspector.Runtime.Drawers {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class SeperatorAttribute : AdditionDrawerAttribute {
        public float Normalize { get; private set; } = 1;

        public int Height { get; set; } = 1;
        public string ColorParameter { get; set; } = "#CDCDCD";
    }
}