using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Drawers.Group {
    public class BeginConditionalGroupAttribute : OverseerBeginGroupAttribute {
        public string Argument { get; private set; }
        public bool Show { get; set; }

        public BeginConditionalGroupAttribute(string arg) {
            Argument = arg;
        }
    }
}