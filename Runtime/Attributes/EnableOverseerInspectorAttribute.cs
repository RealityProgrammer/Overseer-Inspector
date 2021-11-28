using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime {
    /// <summary>
    /// Allow the Behaviour to be display with Overseer.</br>
    /// 
    /// "Hell begin from here."
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EnableOverseerInspectorAttribute : Attribute {
    }
}