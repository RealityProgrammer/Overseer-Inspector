using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Validation {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class EditModeOnlyAttribute : OverseerConditionalAttribute {
    }
}