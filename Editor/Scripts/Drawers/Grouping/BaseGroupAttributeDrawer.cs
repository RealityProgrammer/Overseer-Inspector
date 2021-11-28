using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class BaseGroupAttributeDrawer : BaseAttributeDrawer {
    public int NestingLevel { get; private set; }

    public virtual void EditorInitialize() { }

    public virtual bool ShouldCreateNew(OverseerBeginGroupAttribute attribute) {
        return true;
    }

    public string GroupName => ((OverseerBeginGroupAttribute)AssociatedAttribute).Name;
}
