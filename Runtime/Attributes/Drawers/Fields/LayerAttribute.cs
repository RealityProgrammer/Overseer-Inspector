using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Drawers {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class LayerAttribute : BasePrimaryDrawerAttribute {
        public bool Flag { get; set; }
    }
}