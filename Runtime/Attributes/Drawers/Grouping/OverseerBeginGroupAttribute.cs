using System;

namespace RealityProgrammer.OverseerInspector.Runtime.Drawers.Group {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public abstract class OverseerBeginGroupAttribute : BaseOverseerDrawerAttribute {
        public string Name { get; set; }
        public int ID { get; set; }
    }
}