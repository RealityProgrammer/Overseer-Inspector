using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Miscs {
    /// <summary>
    /// Determine whether the field could be edited during executing mode, or handling
    /// field's disability will be shown.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class ReadonlyFieldAttribute : BaseOverseerAttribute {
        public bool PMVisibility { get; set; } = true;
        public bool PMEditable { get; set; } = true;

        public bool EMVisibility { get; set; }
        public bool EMEditable { get; set; }
    }
}