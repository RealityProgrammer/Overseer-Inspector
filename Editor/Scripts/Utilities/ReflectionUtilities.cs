using System.Reflection;
using System.Reflection.Emit;
using System;
using System.Runtime.CompilerServices;

namespace RealityProgrammer.OverseerInspector.Editors.Utility {
    public static class ReflectionUtilities {
        public static readonly Type BooleanType = typeof(bool);

#if OVERSEER_INSPECTOR_ENABLE_EMIT
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InitializeDynamicMethod(string name, Type ret, Type[] args, Type owner, out DynamicMethod mtd, out ILGenerator il) {
            mtd = new DynamicMethod(name, ret, args, owner, true);
            il = mtd.GetILGenerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EmitInverseBoolean(this ILGenerator il) {
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
        }
#endif

        public static bool ObtainMemberInfoFromArgument(Type type, string argument, out FieldInfo field, out PropertyInfo property, out MethodInfo method) {
            if (string.IsNullOrEmpty(argument)) {
                field = null;
                method = null;
                property = null;

                return false;
            }

            if (argument.EndsWith("()")) {
                method = type.GetMethod(argument.Substring(0, argument.Length - 2), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                field = null;
                property = null;

                if (method == null) {
                    return false;
                } else {
                    if (method.GetParameters().Length == 0) {
                        return true;
                    }
                }
            } else {
                field = type.GetField(argument, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                method = null;
                property = null;

                if (field == null) {
                    property = type.GetProperty(argument, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    field = null;
                    method = null;

                    if (property != null) {
                        return true;
                    }

                    return false;
                }

                return true;
            }

            field = null;
            method = null;
            property = null;

            return false;
        }
    }
}