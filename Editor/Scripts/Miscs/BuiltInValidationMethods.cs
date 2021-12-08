using UnityEngine;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using RealityProgrammer.OverseerInspector.Editors.Utility;
using RealityProgrammer.OverseerInspector.Editors.Attributes;
using RealityProgrammer.OverseerInspector.Runtime.Validation;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs {
    /// <summary>
    /// <br>Because Unity Runtime removed references to almost everything in <c>System.Reflection.Emit</c>, so
    /// we cannot put attribute validation code there, so a way to communicate with Editor from Runtime is
    /// needed.</br>
    /// 
    /// <br>Why? Some feature need emit code for better performance.</br>
    /// </summary>
    #pragma warning disable IDE0060
    internal static class BuiltInValidationMethods {
#if OVERSEER_INSPECTOR_ENABLE_EMIT
        public static readonly Dictionary<OverseerConditionalAttribute, Func<object, bool>> _showIfAttrEmitStorage = new Dictionary<OverseerConditionalAttribute, Func<object, bool>>();
        public static readonly Type[] DefaultDynamicMethodParamTypes = new Type[] { typeof(object) };
        public static readonly Type DynamicMethodDelegateType = typeof(Func<object, bool>);
#endif
        [ConditionalConnect(typeof(ShowIfAttribute))]
        internal static bool ShowIfValidationMethod(OverseerConditionalAttribute attr, object target) {
            var argument = ((ShowIfAttribute)attr).Argument;

            if (string.IsNullOrEmpty(argument))
                return true;

            var type = target.GetType();
            bool inverted = argument[0] == '!';

            ReflectionUtilities.ObtainMemberInfoFromArgument(type, inverted ? argument.Substring(1, argument.Length - 1) : argument, out var field, out var property, out var method);

            // Testing around
#if !OVERSEER_INSPECTOR_ENABLE_EMIT
            if (field != null && field.FieldType == ReflectionUtilities.BooleanType) {
                return (bool)field.GetValue(target) ^ inverted;
            } else if (property != null && property.CanRead && property.PropertyType == ReflectionUtilities.BooleanType) {
                return (bool)property.GetValue(target) ^ inverted;
            } else if (method != null && method.ReturnType == ReflectionUtilities.BooleanType) {
                return (bool)method.Invoke(target, null) ^ inverted;
            }

            return true;
#else
            if (_showIfAttrEmitStorage.TryGetValue(attr, out var @delegate)) {
                return @delegate.Invoke(target);
            }

            if (field != null && field.FieldType == BooleanType) {
                ReflectionUtilities.InitializeDynamicMethod("ShowIfValidation_Field", BooleanType, DefaultDynamicMethodParamTypes, typeof(ShowIfAttribute), out var dm, out var il);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, field);
                if (inverted) {
                    il.EmitInverseBoolean();
                }

                il.Emit(OpCodes.Ret);

                @delegate = (Func<object, bool>)dm.CreateDelegate(DynamicMethodDelegateType);
                _showIfAttrEmitStorage.Add(attr, @delegate);

                return @delegate.Invoke(target);
            } else if (property != null && property.CanRead && property.PropertyType == BooleanType) {
                ReflectionUtilities.InitializeDynamicMethod("ShowIfValidation_Property", BooleanType, DefaultDynamicMethodParamTypes, typeof(ShowIfAttribute), out var dm, out var il);

                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Call, property.GetGetMethod(true), null);
                if (inverted) {
                    il.EmitInverseBoolean();
                }

                il.Emit(OpCodes.Ret);

                @delegate = (Func<object, bool>)dm.CreateDelegate(DynamicMethodDelegateType);
                _showIfAttrEmitStorage.Add(attr, @delegate);

                return @delegate.Invoke(target);
            } else if (method != null && method.ReturnType == BooleanType) {
                ReflectionUtilities.InitializeDynamicMethod("ShowIfValidation_Method", BooleanType, DefaultDynamicMethodParamTypes, typeof(ShowIfAttribute), out var dm, out var il);

                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Callvirt, method, null);
                if (inverted) {
                    il.EmitInverseBoolean();
                }

                il.Emit(OpCodes.Ret);

                @delegate = (Func<object, bool>)dm.CreateDelegate(DynamicMethodDelegateType);
                _showIfAttrEmitStorage.Add(attr, @delegate);

                return @delegate.Invoke(target);
            }

            _showIfAttrEmitStorage.Add(attr, (_) => true);

            return true;
#endif
        }

        [ConditionalConnect(typeof(PlayModeOnlyAttribute))]
        internal static bool PlayModeOnlyValidationMethod(OverseerConditionalAttribute attr, object target) {
            return Application.isPlaying;
        }

        [ConditionalConnect(typeof(EditModeOnlyAttribute))]
        internal static bool EditModeOnlyValidationMethod(OverseerConditionalAttribute attr, object target) {
            return !Application.isPlaying;
        }

        [ConditionalConnect(typeof(ShowIfNullAttribute))]
        internal static bool ShowIfNullValidationMethod(OverseerConditionalAttribute attr, object target) {
            var type = target.GetType();
            var fieldName = ((ShowIfNullAttribute)attr).FieldName;

            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) {
                var property = type.GetProperty(fieldName, BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null) {
                    object pvalue = property.GetValue(target);

                    if (pvalue is UnityEngine.Object puobj) {
                        return puobj == null;
                    }

                    return pvalue == null;
                }

                return true;
            }

            object fvalue = field.GetValue(target);
            if (fvalue is UnityEngine.Object fuobj) {
                return fuobj == null;
            }

            return fvalue == null;
        }

        [ConditionalConnect(typeof(HideIfNullAttribute))]
        internal static bool HideIfNullValidationMethod(OverseerConditionalAttribute attr, object target) {
            var type = target.GetType();
            var fieldName = ((HideIfNullAttribute)attr).FieldName;

            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) {
                var property = type.GetProperty(fieldName, BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null) {
                    object pvalue = property.GetValue(target);

                    if (pvalue is UnityEngine.Object puobj) {
                        return puobj != null;
                    }

                    return pvalue != null;
                }

                return true;
            }

            object fvalue = field.GetValue(target);
            if (fvalue is UnityEngine.Object fuobj) {
                return fuobj != null;
            }

            return fvalue != null;
        }
    }
#pragma warning restore IDE0060
}