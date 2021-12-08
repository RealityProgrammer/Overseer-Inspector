using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using RealityProgrammer.OverseerInspector.Runtime.Drawers.Group;
using RealityProgrammer.OverseerInspector.Editors.Attributes;
using RealityProgrammer.OverseerInspector.Editors.Utility;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers.Group {
    [BindDrawerTo(typeof(BeginConditionalGroupAttribute))]
    public class ConditionalGroupDrawer : BaseGroupAttributeDrawer {
        BeginConditionalGroupAttribute attr;

        FieldInfo _field;
        PropertyInfo _property;
        MethodInfo _method;

        bool inverted;

        public override void Initialize() {
            attr = (BeginConditionalGroupAttribute)AssociatedAttribute;

            inverted = attr.Argument.StartsWith("!");

            ReflectionUtilities.ObtainMemberInfoFromArgument(AssociatedObject.targetObject.GetType(), inverted ? attr.Argument.Substring(1, attr.Argument.Length - 1) : attr.Argument, out _field, out _property, out _method);

            if (_field != null && _field.FieldType != ReflectionUtilities.BooleanType) {
                _field = null;
            } else if (_property != null && (!_property.CanRead || _property.PropertyType != ReflectionUtilities.BooleanType)) {
                _property = null;
            } else if (_method != null && _method.ReturnType != ReflectionUtilities.BooleanType) {
                _method = null;
            }
        }

        public override void DrawLayout() {
            bool validate = true;

            if (_field != null) {
                validate = (bool)_field.GetValue(AssociatedObject.targetObject) ^ inverted;
            } else if (_property != null) {
                validate = (bool)_property.GetValue(AssociatedObject.targetObject) ^ inverted;
            } else if (_method != null) {
                validate = (bool)_method.Invoke(AssociatedObject.targetObject, null) ^ inverted;
            }

            if (validate) {
                DrawAllChildsLayout();
            }
        }
    }
}