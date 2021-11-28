using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Validation {
    public abstract class ConditionalValidationAttribute : BaseOverseerAttribute {
        public abstract bool Validation(object target);
    }
}