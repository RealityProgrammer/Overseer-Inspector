using System;

namespace RealityProgrammer.OverseerInspector.Editors.Attributes {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ConditionalConnectAttribute : Attribute {
        public Type ConditionalType { get; private set; }

        public ConditionalConnectAttribute(Type type) {
            ConditionalType = type;
        }
    }
}