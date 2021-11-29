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
        public static readonly Type BooleanType = typeof(bool);

        [ConditionalConnect(typeof(ShowIfAttribute))]
        internal static bool ShowIfValidationMethod(OverseerConditionalAttribute attr, object target) {
            var argument = ((ShowIfAttribute)attr).Argument;

            if (string.IsNullOrEmpty(argument))
                return true;

            var type = target.GetType();
            bool inverted = argument[0] == '!';

            // Testing around
#if !OVERSEER_INSPECTOR_ENABLE_EMIT
            if (argument.EndsWith("()")) {
                var method = type.GetMethod(argument.Substring(inverted ? 1 : 0, argument.Length - 2), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (method == null) {
                    return true;
                } else {
                    if (method.ReturnType == BooleanType && method.GetParameters().Length == 0) {
                        return (bool)method.Invoke(target, null) ^ inverted;
                    }
                }
            } else {
                if (argument.EndsWith(">k__BackingField")) {
                    var field = type.GetField(inverted ? argument.Substring(1, argument.Length - 16) : argument, BindingFlags.NonPublic | BindingFlags.Instance);

                    if (field == null) {
                        return true;
                    } else if (field.FieldType == BooleanType) {
                        return (bool)field.GetValue(target) ^ inverted;
                    }
                } else {
                    var property = type.GetProperty(inverted ? argument.Substring(1, argument.Length - 1) : argument, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);

                    if (property != null && property.PropertyType == BooleanType) {
                        return (bool)property.GetValue(target) ^ inverted;
                    } else {
                        var field = type.GetField(inverted ? argument.Substring(1, argument.Length - 1) : argument, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                        if (field == null) {
                            return true;
                        } else if (field.FieldType == BooleanType) {
                            bool invoke = (bool)field.GetValue(target);

                            return (bool)field.GetValue(target) ^ inverted;
                        }
                    }
                }
            }
#else
            if (_showIfAttrEmitStorage.TryGetValue(attr, out var @delegate)) {
                return @delegate.Invoke(target);
            }

            if (argument.EndsWith("()")) {
                string methodName = inverted ? argument.Substring(1, argument.Length - 3) : argument.Substring(0, argument.Length - 2);

                var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (method != null) {
                    if (method.ReturnType == BooleanType && method.GetParameters().Length == 0) {
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
                }
            } else {
                if (argument.EndsWith(">k__BackingField")) {
                    string name = inverted ? argument.Substring(1, argument.Length - 1) : argument;
                    var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);

                    if (field != null && field.FieldType == BooleanType) {
                        ReflectionUtilities.InitializeDynamicMethod("ShowIfValidation_BackingField", BooleanType, DefaultDynamicMethodParamTypes, typeof(ShowIfAttribute), out var dm, out var il);

                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, field);
                        if (inverted) {
                            il.EmitInverseBoolean();
                        }

                        il.Emit(OpCodes.Ret);

                        @delegate = (Func<object, bool>)dm.CreateDelegate(DynamicMethodDelegateType);
                        _showIfAttrEmitStorage.Add(attr, @delegate);

                        return @delegate.Invoke(target);
                    }
                } else {
                    string name = inverted ? argument.Substring(1, argument.Length - 1) : argument;
                    var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);

                    if (property != null && property.PropertyType == typeof(bool)) {
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
                    } else {
                        var field = type.GetField(inverted ? argument.Substring(1, argument.Length - 1) : argument, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

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
                        }
                    }
                }
            }
            _showIfAttrEmitStorage.Add(attr, (_) => true);
#endif

            return true;
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