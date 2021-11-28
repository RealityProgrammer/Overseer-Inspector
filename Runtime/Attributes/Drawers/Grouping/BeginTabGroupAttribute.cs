using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Drawers.Group {
    [Obsolete("WIP")]
    public sealed class BeginTabGroupAttribute : OverseerBeginGroupAttribute {
        public BeginTabGroupAttribute(string path) {
            Name = path;
        }
    }
}