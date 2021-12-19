using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Drawers {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class TagAttribute : BasePrimaryDrawerAttribute {
    }
}