using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Miscs {
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class OverseerIncludeAttribute : BaseOverseerAttribute {
        public string Key { get; set; }

        public OverseerIncludeAttribute() {
        }
    }
}