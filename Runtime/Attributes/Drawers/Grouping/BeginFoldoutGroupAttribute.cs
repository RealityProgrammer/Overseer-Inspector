using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Drawers.Group {
    public sealed class BeginFoldoutGroupAttribute : OverseerBeginGroupAttribute {
        public FontStyle FontStyle { get; set; } = FontStyle.Normal;

        public BeginFoldoutGroupAttribute(string name) {
            Name = name;
        }
    }
}