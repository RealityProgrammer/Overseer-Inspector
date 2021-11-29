using System.Reflection.Emit;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Utility {
    public static class ReflectionUtilities {
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
    }
}