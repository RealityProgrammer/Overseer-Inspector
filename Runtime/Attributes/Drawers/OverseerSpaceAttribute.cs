using System;

namespace RealityProgrammer.OverseerInspector.Runtime.Drawers {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class OverseerSpaceAttribute : AdditionDrawerAttribute {
        public float Amount { get; private set; }
        public OverseerSpaceAttribute(float space) {
            Amount = space;
        }

        public override string ToString() {
            return "OverseerSpaceAttribute(" + Amount + ")";
        }
    }
}