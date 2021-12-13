using UnityEngine;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Editors.Attributes;
using RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora;
using RealityProgrammer.OverseerInspector.Runtime.Validation;

namespace RealityProgrammer.OverseerInspector.Editors.Validators {
    [ConditionalConnect(typeof(PlayModeOnlyAttribute))]
    public class PlayModeOnlyConditionalValidator : BaseConditionalValidator {
        public override bool Validate(ValidateContext context) {
            return Application.isPlaying;
        }
    }
}