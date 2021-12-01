using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Runtime.Drawers;
using RealityProgrammer.OverseerInspector.Editors.Drawers;
using RealityProgrammer.OverseerInspector.Editors.Attributes;

namespace RealityProgrammer.OverseerInspector.Editors.Utility {
    public static class AttributeDrawerCollector {
        // Dictionary<Runtime Attribute Group Type, Editor Drawer Type>
        private static Dictionary<Type, Type> _drawerTypes;

        private static readonly Type baseDrawerType = typeof(BaseOverseerDrawerAttribute);

        // For the sake of safe code
        private static readonly Action<BaseAttributeDrawer, BaseOverseerDrawerAttribute> associatedAttributeAssigner;
        private static readonly Action<BaseAttributeDrawer, OverseerInspectingMember> associatedFieldAssigner;

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
            associatedAttributeAssigner = (Action<BaseAttributeDrawer, BaseOverseerDrawerAttribute>)type.GetProperty(nameof(BaseAttributeDrawer.AssociatedAttribute), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetSetMethod(true).CreateDelegate(typeof(Action<BaseAttributeDrawer, BaseOverseerDrawerAttribute>));
            associatedFieldAssigner = (Action<BaseAttributeDrawer, OverseerInspectingMember>)type.GetProperty(nameof(BaseAttributeDrawer.AssociatedMember), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetSetMethod(true).CreateDelegate(typeof(Action<BaseAttributeDrawer, OverseerInspectingMember>));
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
        /// <param name="member">Associated Field binded to created drawer instance</param>
        /// <param name="drawer">Drawer instance output</param>
        /// <returns>Whether the creation success</returns>
        public static bool TryCreateDrawerInstance(Type attrType, BaseOverseerDrawerAttribute attrInstance, OverseerInspectingMember member, out BaseAttributeDrawer drawer) {
            if ((attrType == null || attrInstance == null) && member.ReflectionCache.Type == ReflectionTargetType.Field) {
                FieldDisplayer fd = new FieldDisplayer();

                associatedFieldAssigner.Invoke(fd, member);

                fd.Initialize();

                drawer = fd;
                return true;
            }

            var retrieve = TryRetrieveDrawer(attrType, out Type drawerType);

            if (retrieve) {
                BaseAttributeDrawer drawerInstance = Activator.CreateInstance(drawerType) as BaseAttributeDrawer;

                associatedAttributeAssigner.Invoke(drawerInstance, attrInstance);
                associatedFieldAssigner.Invoke(drawerInstance, member);

                drawerInstance.Initialize();

                drawer = drawerInstance;
                return true;
            }

            drawer = null;
            return false;
        }
    }
}