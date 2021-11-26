using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class BaseAttributeDrawer : BaseDisplayable {
    public SerializedObject AssociatedObject { get; private set; }
    public BaseOverseerDrawerAttribute AssociatedAttribute { get; private set; }
    public SerializedFieldContainer AssociatedField { get; private set; }
}
