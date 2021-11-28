using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class BindDrawerToAttribute : Attribute {
        public Type AttributeType { get; private set; }

        public BindDrawerToAttribute(Type attr) {
            AttributeType = attr;
        }
    }
}