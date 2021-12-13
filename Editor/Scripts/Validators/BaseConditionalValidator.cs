using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RealityProgrammer.OverseerInspector.Runtime.Validation;

namespace RealityProgrammer.OverseerInspector.Editors.Validators {
    public abstract class BaseConditionalValidator {
        public abstract bool Validate(ValidateContext context);
    }

    public class ValidateContext {
        public OverseerConditionalAttribute Attribute { get; private set; }
        public object ValidateTarget { get; private set; }

        internal ValidateContext(OverseerConditionalAttribute attr, object target) {
            Attribute = attr;
            ValidateTarget = target;
        }
    }
}