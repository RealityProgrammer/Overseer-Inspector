using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Miscs {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class ReadonlyFieldAttribute : BaseOverseerAttribute {
        public ExecutionMode ExecutionMode { get; set; } = ExecutionMode.Always;
    }

    public enum ExecutionMode {
        PlayMode = 1,
        EditMode = 2,

        Always = -1,
    }
}