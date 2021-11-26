using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConditionalValidationAttribute : BaseOverseerAttribute {
    public abstract bool Validation(object target);
}
