using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using RealityProgrammer.OverseerInspector.Editors.Attributes;
using RealityProgrammer.OverseerInspector.Editors.Utility;
using RealityProgrammer.OverseerInspector.Editors.Miscs;
using RealityProgrammer.OverseerInspector.Runtime.Drawers;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers {
    [BindDrawerTo(typeof(MethodButtonAttribute))]
    public class MethodButtonDrawer : BaseMethodPrimaryDrawer {
        private ParameterInfo[] parameterInfos;
        public object[] Parameters { get; private set; }

        #region Validation
        private static readonly HashSet<Type> _defaultInitialization = new HashSet<Type>() {
            typeof(AnimationCurve), typeof(Gradient), typeof(RectOffset), typeof(GUIContent),
        };
        private static readonly HashSet<Type> serializableTypes = new HashSet<Type>() {
                typeof(Vector2), typeof(Vector3), typeof(Vector4), typeof(Quaternion),
                typeof(Vector2Int), typeof(Vector3Int),
                typeof(Rect), typeof(RectInt), typeof(Bounds), typeof(BoundsInt),
                typeof(Matrix4x4),
                typeof(Color), typeof(Color32),
                typeof(LayerMask),

                typeof(string),

                typeof(AnimationCurve), typeof(Gradient), typeof(RectOffset), typeof(GUIStyle)
            };
        private static readonly Type unityObjectType = typeof(UnityEngine.Object);
        bool validation;

        public bool Validate() {
            var um = AssociatedMember.ReflectionCache.UnderlyingMethod;

            if (um.IsGenericMethod || um.IsGenericMethodDefinition) {
                Debug.LogWarning("MethodButtonAttribute cannot be used for methods with generic definition or generic parameter(s).");

                return false;
            }

            foreach (var parameter in parameterInfos) {
                bool supportedType = IsTypeSupported(parameter.ParameterType);

                if (!supportedType) {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("MethodButtonAttribute cannot be used for methods with invalid parameter. Informations:");
                    sb.Append("Method name: ").AppendLine(um.Name);
                    sb.Append("Method declaring type: ").AppendLine(um.DeclaringType.AssemblyQualifiedName);
                    sb.Append("Invalid parameter: ").AppendLine(parameter.Name);
                    sb.Append("Invalid parameter type: ").AppendLine(parameter.ParameterType.AssemblyQualifiedName);

                    Debug.LogWarning(sb.ToString());

                    return false;
                }
            }

            return true;
        }
        #endregion

        MethodButtonAttribute attribute;

        private AnimBool foldoutAnim;

        public MethodButtonDrawer() {
            foldoutAnim = new AnimBool(false);
            //foldoutAnim.speed = 0.4f;

            foldoutAnim.valueChanged.AddListener(() => {
                //Debug.Log(foldoutAnim.faded + " -> " + foldoutAnim.target);
                OverseerInspector.CurrentInspector.RequestConstantRepaint();
            });
        }

        private static bool IsTypeSupported(Type type) {
            if (type.IsPrimitive)
                return true;

            if (type.IsAbstract)
                return false;

            if (type.IsAbstract && type.IsSealed)
                return false;

            if (serializableTypes.Contains(type))
                return true;

            if (type.IsSubclassOf(unityObjectType) || type == unityObjectType)
                return true;

            return false;
        }

        public override void Initialize() {
            attribute = AssociatedAttribute as MethodButtonAttribute;

            parameterInfos = AssociatedMember.ReflectionCache.UnderlyingMethod.GetParameters();
            Parameters = new object[parameterInfos.Length];

            validation = Validate();

            if (!validation) return;

            if (parameterInfos.Length > 0) {
                for (int i = 0; i < parameterInfos.Length; i++) {
                    var pt = parameterInfos[i].ParameterType;

                    if (pt.IsValueType) {
                        Parameters[i] = Activator.CreateInstance(pt);
                    } else {
                        if (_defaultInitialization.Contains(pt)) {
                            Parameters[i] = Activator.CreateInstance(pt);
                        } else {
                            Parameters[i] = null;
                        }
                    }
                }
            }
        }

        public override void DrawLayout() {
            if (!validation) return;

            DrawAllChildsLayout();

            foldoutAnim.target = EditorGUILayout.Foldout(foldoutAnim.target, GetMethodDisplayName());


            if (foldoutAnim.faded > 0.001f) {
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.BeginFadeGroup(foldoutAnim.faded);
                {
                    if (GUI.Button(EditorGUILayout.GetControlRect(GUILayout.Height(Math.Abs(attribute.ButtonSize))), "Invoke")) {
                        AssociatedMember.ReflectionCache.UnderlyingMethod.Invoke(AssociatedMember.Target, Parameters);
                    }

                    if (Parameters.Length != 0) {
                        EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);

                        for (int i = 0; i < Parameters.Length; i++) {
                            Parameters[i] = OverseerEditorUtilities.DrawLayoutBasedOnType(Parameters[i], parameterInfos[i].ParameterType, parameterInfos[i].Name);
                        }
                    }
                }
                EditorGUILayout.EndFadeGroup();
                EditorGUILayout.EndVertical();
            }
        }

        private string GetMethodDisplayName() {
            string dn = attribute.DisplayName;
            var umn = AssociatedMember.ReflectionCache.UnderlyingMethod.Name;

            if (string.IsNullOrWhiteSpace(dn) || string.IsNullOrEmpty(dn))
                return umn;

            if (dn == "*") {
                return ObjectNames.NicifyVariableName(umn);
            }

            return dn;
        }
    }
}