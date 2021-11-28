using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Runtime.Drawers;
using RealityProgrammer.OverseerInspector.Editors.Drawers;

namespace RealityProgrammer.OverseerInspector.Editors.Utility {
    public static class AttributeDrawerCollector {
        // Dictionary<Runtime Attribute Group Type, Editor Drawer Type>
        private static Dictionary<Type, Type> _drawerTypes;

        private static readonly Type baseDrawerType = typeof(BaseOverseerDrawerAttribute);

        // For the sake of safe code
        private static readonly Action<BaseAttributeDrawer, SerializedObject> associatedObjectAssigner;
        private static readonly Action<BaseAttributeDrawer, BaseOverseerDrawerAttribute> associatedAttributeAssigner;
        private static readonly Action<BaseAttributeDrawer, SerializedFieldContainer> associatedFieldAssigner;

        static AttributeDrawerCollector() {
            var allDrawers = TypeCache.GetTypesWithAttribute<BindDrawerToAttribute>();
            _drawerTypes = new Dictionary<Type, Type>(allDrawers.Count);

            foreach (var drawer in allDrawers) {
                var attrs = drawer.GetCustomAttributes<BindDrawerToAttribute>();

                foreach (var attr in attrs) {
                    if (attr.AttributeType.IsSubclassOf(baseDrawerType)) {
                        _drawerTypes.Add(attr.AttributeType, drawer);
                    }
                }
            }

            var type = typeof(BaseAttributeDrawer);
            associatedObjectAssigner = (Action<BaseAttributeDrawer, SerializedObject>)type.GetProperty(nameof(BaseAttributeDrawer.AssociatedObject), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetSetMethod(true).CreateDelegate(typeof(Action<BaseAttributeDrawer, SerializedObject>));
            associatedAttributeAssigner = (Action<BaseAttributeDrawer, BaseOverseerDrawerAttribute>)type.GetProperty(nameof(BaseAttributeDrawer.AssociatedAttribute), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetSetMethod(true).CreateDelegate(typeof(Action<BaseAttributeDrawer, BaseOverseerDrawerAttribute>));
            associatedFieldAssigner = (Action<BaseAttributeDrawer, SerializedFieldContainer>)type.GetProperty(nameof(BaseAttributeDrawer.AssociatedField), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetSetMethod(true).CreateDelegate(typeof(Action<BaseAttributeDrawer, SerializedFieldContainer>));
        }

        public static bool TryRetrieveDrawer<T>(out Type drawer) where T : BaseOverseerDrawerAttribute {
            return TryRetrieveDrawer(typeof(T), out drawer);
        }

        public static bool TryRetrieveDrawer(Type type, out Type drawer) {
            return _drawerTypes.TryGetValue(type, out drawer);
        }

        /// <summary>
        /// Create a drawer instance of attribute type T with Associated Attribute and Field assigned
        /// </summary>
        /// <param name="attrType">Type of Runtime Drawer Attribute</typeparam>
        /// <param name="attrInstance">Associated Attribute binded to created drawer instance</param>
        /// <param name="field">Associated Field binded to created drawer instance</param>
        /// <param name="drawer">Drawer instance output</param>
        /// <returns>Whether the creation success</returns>
        public static bool TryCreateDrawerInstance(Type attrType, BaseOverseerDrawerAttribute attrInstance, SerializedFieldContainer field, out BaseAttributeDrawer drawer) {
            if (attrType == null || attrInstance == null) {
                FieldDisplayer fd = new FieldDisplayer();

                associatedFieldAssigner.Invoke(fd, field);
                associatedObjectAssigner.Invoke(fd, field.Property.serializedObject);

                drawer = fd;
                return true;
            }

            var retrieve = TryRetrieveDrawer(attrType, out Type drawerType);

            if (retrieve) {
                BaseAttributeDrawer drawerInstance = Activator.CreateInstance(drawerType) as BaseAttributeDrawer;

                associatedAttributeAssigner.Invoke(drawerInstance, attrInstance);
                associatedFieldAssigner.Invoke(drawerInstance, field);
                associatedObjectAssigner.Invoke(drawerInstance, field.Property.serializedObject);

                drawer = drawerInstance;
                return true;
            }

            drawer = null;
            return false;
        }
    }
}