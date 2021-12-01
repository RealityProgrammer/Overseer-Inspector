using UnityEditor;

namespace RealityProgrammer.OverseerInspector.Editors {
    public sealed class OverseerInspectingMember {
        // The property are tied to Instance, not tied to the Type, so it will be different for each inspecting object
        public SerializedProperty Property { get; internal set; }
        public object Target { get; private set; }
        public bool LastValidation { get; private set; }

        public ReflectionCacheUnit ReflectionCache { get; private set; }

        public void ForceCheckValidation() {
            LastValidation = ReflectionCache.CheckCondition(Target);
        }

        internal static OverseerInspectingMember Create(SerializedProperty property, ReflectionCacheUnit cache, object target) {
            return new OverseerInspectingMember() {
                Property = property,
                Target = target,
                ReflectionCache = cache,
            };
        }

        public bool IsBackingField => ReflectionCache.IsBackingField;
        public bool IsEndGroup => ReflectionCache.IsEndGroup;
    }
}