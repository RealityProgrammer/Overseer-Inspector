using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class ShowIfAttribute : ConditionalValidationAttribute {
    public string Argument { get; private set; }

    public ShowIfAttribute(string argument) {
        Argument = argument;
    }

    private static readonly Type boolType = typeof(bool);

    public override bool Validation(object target) {
        if (string.IsNullOrEmpty(Argument)) return true;

        var type = target.GetType();
        bool inverted = Argument[0] == '!'; 

        if (Argument.EndsWith("()")) {
            var method = type.GetMethod(Argument.Substring(inverted ? 1 : 0, Argument.Length - 2), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (method == null) {
                return true;
            } else {
                if (method.ReturnType == boolType && method.GetParameters().Length == 0) {
                    return (bool)method.Invoke(target, null) ^ inverted;
                }
            }
        } else {
            if (Argument.EndsWith(">k__BackingField")) {
                var field = type.GetField(inverted ? Argument.Substring(1, Argument.Length - 1) : Argument, BindingFlags.NonPublic | BindingFlags.Instance);

                if (field == null) {
                    return true;
                } else if (field.FieldType == boolType) {
                    return (bool)field.GetValue(target) ^ inverted;
                }
            } else {
                var property = type.GetProperty(inverted ? Argument.Substring(1, Argument.Length - 1) : Argument, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);

                if (property != null && property.PropertyType == boolType) {
                    return (bool)property.GetValue(target) ^ inverted;
                } else {
                    var field = type.GetField(inverted ? Argument.Substring(1, Argument.Length - 1) : Argument, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (field == null) {
                        return true;
                    } else if (field.FieldType == boolType) {
                        bool invoke = (bool)field.GetValue(target);

                        return (bool)field.GetValue(target) ^ inverted;
                    }
                }
            }
        }

        return true;
    }
}
