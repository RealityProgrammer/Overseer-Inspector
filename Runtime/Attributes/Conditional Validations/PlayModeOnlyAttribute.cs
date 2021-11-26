using System;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class PlayModeOnlyAttribute : ConditionalValidationAttribute {
    public override bool Validation(object target) {
        return Application.isPlaying;
    }
}
