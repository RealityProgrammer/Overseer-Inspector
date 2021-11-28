using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Runtime.Drawers.Group;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers.Group {
    public abstract class BaseGroupAttributeDrawer : BaseAttributeDrawer {
        public int NestingLevel { get; private set; }

        public virtual void EditorInitialize() { }

        public string GroupName => ((OverseerBeginGroupAttribute)AssociatedAttribute).Name;
    }
}