using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Runtime.Drawers;
using RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora;
using RealityProgrammer.OverseerInspector.Editors.Utility;
using RealityProgrammer.OverseerInspector.Editors.Attributes;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers {
    [BindDrawerTo(typeof(VectorRangeAttribute))]
    public class VectorRangeAttributeDrawer : BasePrimaryFieldDrawer {
        internal struct Cache {
            public BaseExpression[] Limits { get; private set; }
            public string[] ErrorMsg { get; private set; }

            public Cache(int size) {
                Limits = new BaseExpression[size];
                ErrorMsg = new string[size];
            }

            public void HandleExpression(string program, object target, int storeLoc, float fallback = 0) {
                try {
                    var tokens = s_Scanner.Scan(program);

                    // Only EOF
                    if (tokens.Count == 1) {
                        tokens.Insert(0, new LexerToken(TokenType.Number, fallback, fallback.ToString()));
                    }

                    s_Lexer.BindTarget(target);
                    s_Lexer.FeedTokens(tokens);
                    Limits[storeLoc] = s_Lexer.BeginLexing();
                } catch (Exception e) {
                    Debug.Log(e.ToString());

                    ErrorMsg[storeLoc] = e.ToString();
                }
            }
        }

        bool errorFoldout = false;
        bool validate = true;

        VectorRangeAttribute underlyingAttr;

        SerializedProperty[] axisProperties;

        private static readonly AuroraScanner s_Scanner;
        private static readonly AuroraLexer s_Lexer;
        private static readonly AuroraInterpreter s_Interpreter;

        private static readonly Dictionary<VectorRangeAttribute, Cache> _caches;

        static VectorRangeAttributeDrawer() {
            s_Scanner = new AuroraScanner();
            s_Lexer = new AuroraLexer();
            s_Interpreter = new AuroraInterpreter();

            _caches = new Dictionary<VectorRangeAttribute, Cache>();
        }

        public BaseExpression[] limitExpressions;

        public override void Initialize() {
            var ut = AssociatedMember.ReflectionCache.UnderlyingField.FieldType;

            validate = ValidateUnderlyingType(ut);
            if (!validate) {
                Debug.LogWarning("Invalid attribute detected. " + nameof(VectorRangeAttribute) + " only works with fields with the type of Vector2, Vector3 and Vector4, and it doesn't work with " + ut.FullName);
                return;
            }

            underlyingAttr = (VectorRangeAttribute)AssociatedAttribute;
            var bindingTarget = AssociatedObject.targetObject;

            if (ut == typeof(Vector2)) {
                axisProperties = new SerializedProperty[2];

                var cache = new Cache(4);

                cache.HandleExpression(underlyingAttr.XMinRange, bindingTarget, 0, 0);
                cache.HandleExpression(underlyingAttr.XMaxRange, bindingTarget, 1, 1);
                cache.HandleExpression(underlyingAttr.YMinRange, bindingTarget, 2, 0);
                cache.HandleExpression(underlyingAttr.YMaxRange, bindingTarget, 3, 1);

                _caches[underlyingAttr] = cache;
            } else if (ut == typeof(Vector3)) {
                axisProperties = new SerializedProperty[3];
                axisProperties[2] = AssociatedMember.Property.FindPropertyRelative("z");

                var cache = new Cache(6);

                cache.HandleExpression(underlyingAttr.XMinRange, bindingTarget, 0, 0);
                cache.HandleExpression(underlyingAttr.XMaxRange, bindingTarget, 1, 1);
                cache.HandleExpression(underlyingAttr.YMinRange, bindingTarget, 2, 0);
                cache.HandleExpression(underlyingAttr.YMaxRange, bindingTarget, 3, 1);
                cache.HandleExpression(underlyingAttr.ZMinRange, bindingTarget, 4, 0);
                cache.HandleExpression(underlyingAttr.ZMaxRange, bindingTarget, 5, 1);

                _caches[underlyingAttr] = cache;
            } else if (ut == typeof(Vector4)) {
                axisProperties = new SerializedProperty[4];
                axisProperties[2] = AssociatedMember.Property.FindPropertyRelative("z");
                axisProperties[3] = AssociatedMember.Property.FindPropertyRelative("w");

                var cache = new Cache(8);

                cache.HandleExpression(underlyingAttr.XMinRange, bindingTarget, 0, 0);
                cache.HandleExpression(underlyingAttr.XMaxRange, bindingTarget, 1, 1);
                cache.HandleExpression(underlyingAttr.YMinRange, bindingTarget, 2, 0);
                cache.HandleExpression(underlyingAttr.YMaxRange, bindingTarget, 3, 1);
                cache.HandleExpression(underlyingAttr.ZMinRange, bindingTarget, 4, 0);
                cache.HandleExpression(underlyingAttr.ZMaxRange, bindingTarget, 5, 1);
                cache.HandleExpression(underlyingAttr.WMinRange, bindingTarget, 6, 0);
                cache.HandleExpression(underlyingAttr.WMaxRange, bindingTarget, 7, 1);

                _caches[underlyingAttr] = cache;
            }

            axisProperties[0] = AssociatedMember.Property.FindPropertyRelative("x");
            axisProperties[1] = AssociatedMember.Property.FindPropertyRelative("y");
        }

        public static bool ValidateUnderlyingType(Type t) {
            return t == typeof(Vector2) || t == typeof(Vector3) || t == typeof(Vector4);
        }

        public override void DrawLayout() {
            if (!AssociatedMember.ConditionalCheck) {
                return;
            }

            DrawAllChildsLayout();

            if (BeginHandleReadonly()) {
                BeginHandleFieldAssignCallback();

                if (!validate) {
                    EditorGUILayout.PropertyField(AssociatedMember.Property);
                } else {
                    var cache = _caches[underlyingAttr];

                    float xMin = Evaluate(cache.Limits[0]);
                    float xMax = Evaluate(cache.Limits[1]);

                    float yMin = Evaluate(cache.Limits[2]);
                    float yMax = Evaluate(cache.Limits[3]);

                    switch (axisProperties.Length) {
                        case 2: {
                            if (AssociatedMember.Property.isExpanded = EditorGUILayout.Foldout(AssociatedMember.Property.isExpanded, AssociatedMember.Property.displayName)) {
                                using (new EditorGUI.IndentLevelScope()) {
                                    bool changed = false;

                                    DoRange(underlyingAttr.XDisplay, "X", axisProperties[0].floatValue, xMin, xMax, out float newX, ref changed);
                                    DoRange(underlyingAttr.YDisplay, "Y", axisProperties[1].floatValue, yMin, yMax, out float newY, ref changed);

                                    if (changed) {
                                        var target = AssociatedObject.targetObject;

                                        Undo.RecordObject(target, "Changed VectorRange");

                                        if (EditorUtility.IsPersistent(target)) {
                                            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                                        }

                                        axisProperties[0].floatValue = newX;
                                        axisProperties[1].floatValue = newY;
                                    }
                                }
                            }
                            break;
                        }

                        case 3: {
                            if (AssociatedMember.Property.isExpanded = EditorGUILayout.Foldout(AssociatedMember.Property.isExpanded, AssociatedMember.Property.displayName)) {
                                using (new EditorGUI.IndentLevelScope()) {
                                    bool changed = false;

                                    float zMin = Evaluate(cache.Limits[4]);
                                    float zMax = Evaluate(cache.Limits[5]);

                                    DoRange(underlyingAttr.XDisplay, "X", axisProperties[0].floatValue, xMin, xMax, out float newX, ref changed);
                                    DoRange(underlyingAttr.YDisplay, "Y", axisProperties[1].floatValue, yMin, yMax, out float newY, ref changed);
                                    DoRange(underlyingAttr.ZDisplay, "Z", axisProperties[2].floatValue, zMin, zMax, out float newZ, ref changed);

                                    if (changed) {
                                        var target = AssociatedObject.targetObject;

                                        Undo.RecordObject(target, "Changed VectorRange");

                                        if (EditorUtility.IsPersistent(target)) {
                                            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                                        }

                                        axisProperties[0].floatValue = newX;
                                        axisProperties[1].floatValue = newY;
                                        axisProperties[2].floatValue = newZ;
                                    }
                                }
                            }
                            break;
                        }

                        case 4:
                            if (AssociatedMember.Property.isExpanded = EditorGUILayout.Foldout(AssociatedMember.Property.isExpanded, AssociatedMember.Property.displayName)) {
                                using (new EditorGUI.IndentLevelScope()) {
                                    bool changed = false;

                                    float zMin = Evaluate(cache.Limits[4]);
                                    float zMax = Evaluate(cache.Limits[5]);

                                    float wMin = Evaluate(cache.Limits[6]);
                                    float wMax = Evaluate(cache.Limits[7]);

                                    DoRange(underlyingAttr.XDisplay, "X", axisProperties[0].floatValue, xMin, xMax, out float newX, ref changed);
                                    DoRange(underlyingAttr.YDisplay, "Y", axisProperties[1].floatValue, yMin, yMax, out float newY, ref changed);
                                    DoRange(underlyingAttr.ZDisplay, "Z", axisProperties[2].floatValue, zMin, zMax, out float newZ, ref changed);
                                    DoRange(underlyingAttr.WDisplay, "W", axisProperties[3].floatValue, wMin, wMax, out float newW, ref changed);

                                    if (changed) {
                                        var target = AssociatedObject.targetObject;

                                        Undo.RecordObject(target, "Changed VectorRange");

                                        if (EditorUtility.IsPersistent(target)) {
                                            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                                        }

                                        axisProperties[0].floatValue = newX;
                                        axisProperties[1].floatValue = newY;
                                        axisProperties[2].floatValue = newZ;
                                        axisProperties[3].floatValue = newW;
                                    }
                                }
                            }
                            break;
                    }
                }

                EndHandleFieldAssignCallback();
            }
            EndHandleReadonly();
        }

        private static void DoRange(AxisDisplayMode mode, string label, float input, float min, float max, out float newValue, ref bool changed) {
            newValue = input;

            if (newValue < min) {
                newValue = min;
                changed = true;
            } else if (newValue > max) {
                newValue = max;
                changed = true;
            }

            EditorGUI.BeginChangeCheck();

            switch (mode) {
                case AxisDisplayMode.Field:
                    EditorGUI.BeginChangeCheck();

                    newValue = EditorGUILayout.FloatField(label, newValue);

                    if (EditorGUI.EndChangeCheck()) {
                        newValue = Mathf.Clamp(newValue, min, max);
                        changed = true;
                    }
                    return;

                case AxisDisplayMode.Slider:
                    EditorGUI.BeginChangeCheck();

                    newValue = EditorGUILayout.Slider(label, newValue, min, max);

                    if (EditorGUI.EndChangeCheck()) {
                        newValue = Mathf.Clamp(newValue, min, max);
                        changed = true;
                    }
                    return;
            }

            Debug.LogError("Undefined range display mode of " + mode + " with the value of " + (int)mode);
        }

        private float Evaluate(BaseExpression expr) {
            int id = GUIUtility.GetControlID(FocusType.Passive);

            try {
                var result = s_Interpreter.InterpretExpression(expr);
                if (result is IConvertible convertible) {
                    return convertible.ToSingle(null);
                } else {
                    throw new Exception("Unexpected return type of " + result.GetType().FullName);
                }
            } catch (Exception e) {
                errorFoldout = OverseerEditorUtilities.DoStacktraceBoxLayout(e, errorFoldout, id);
            }

            return float.NaN;
        }
    }
}