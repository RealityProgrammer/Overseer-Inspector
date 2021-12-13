using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using RealityProgrammer.OverseerInspector.Runtime.Drawers.Group;
using RealityProgrammer.OverseerInspector.Editors.Attributes;
using RealityProgrammer.OverseerInspector.Editors.Utility;
using RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers.Group {
    [BindDrawerTo(typeof(BeginConditionalGroupAttribute))]
    public class ConditionalGroupDrawer : BaseGroupAttributeDrawer {
        BeginConditionalGroupAttribute attr;

        FieldInfo _field;
        PropertyInfo _property;
        MethodInfo _method;

        bool inverted;

        AuroraScanner scanner;
        AuroraLexer lexer;
        AuroraInterpreter interpreter;
        BaseExpression outputExpression;

        string errorMsg;

        public override void Initialize() {
            attr = (BeginConditionalGroupAttribute)AssociatedAttribute;

            try {
                AuroraUtilities.InitializeEverything(attr.Argument, AssociatedObject.targetObject, out scanner, out lexer, out outputExpression, out interpreter);
            } catch (Exception e) {
                errorMsg = e.GetType().Name + " were thrown: " + e.Message;
            }
        }

        public override void DrawLayout() {
            if (string.IsNullOrEmpty(errorMsg)) {
                try {
                    var result = interpreter.InterpretExpression(outputExpression);

                    if (result is bool b) {
                        if (b) DrawAllChildsLayout();
                    } else {
                        errorMsg = "Output result is not a boolean";
                    }
                } catch (Exception e) {
                    errorMsg = e.GetType().Name + " were thrown: " + e.Message;
                }
            } else {
                DrawAllChildsLayout();
            }
        }
    }
}