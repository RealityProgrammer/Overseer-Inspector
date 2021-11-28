using UnityEditor;
using RealityProgrammer.OverseerInspector.Runtime.Drawers;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers {
    public abstract class BaseAttributeDrawer : BaseDisplayable {
        public SerializedObject AssociatedObject { get; private set; }
        public BaseOverseerDrawerAttribute AssociatedAttribute { get; private set; }
        public SerializedFieldContainer AssociatedField { get; private set; }
    }
}