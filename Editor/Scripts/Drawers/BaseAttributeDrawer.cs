using UnityEditor;
using RealityProgrammer.OverseerInspector.Runtime.Drawers;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers {
    public abstract class BaseAttributeDrawer : BaseDisplayable {
        public SerializedObject AssociatedObject => AssociatedMember.Property.serializedObject;
        public BaseOverseerDrawerAttribute AssociatedAttribute { get; private set; }
        public OverseerInspectingMember AssociatedMember { get; private set; }
    }
}