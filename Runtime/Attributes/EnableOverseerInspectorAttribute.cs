using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class EnableOverseerInspectorAttribute : Attribute {
    }
}