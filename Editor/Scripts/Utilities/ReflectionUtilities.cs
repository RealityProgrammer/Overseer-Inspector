using System.Reflection;
using System.Reflection.Emit;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Utility {
    public static class ReflectionUtilities {
        public static readonly Type BooleanType = typeof(bool);

        private static readonly Dictionary<Type, HashSet<Type>> _primitiveImplicitCasts;
        private static readonly Dictionary<Type, HashSet<MethodInfo>> _implicitCastCheckCache;

        static ReflectionUtilities() {
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/numeric-conversions#implicit-numeric-conversions

            _primitiveImplicitCasts = new Dictionary<Type, HashSet<Type>>() {
                [typeof(sbyte)] = new HashSet<Type> { typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) },
                [typeof(byte)] = new HashSet<Type> { typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) },
                [typeof(short)] = new HashSet<Type> { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) },
                [typeof(ushort)] = new HashSet<Type> { typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) },
                [typeof(int)] = new HashSet<Type> { typeof(long), typeof(float), typeof(double), typeof(decimal) },
                [typeof(uint)] = new HashSet<Type> { typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) },
                [typeof(long)] = new HashSet<Type> { typeof(float), typeof(double), typeof(decimal) },
                [typeof(ulong)] = new HashSet<Type> { typeof(float), typeof(double), typeof(decimal) },
                [typeof(float)] = new HashSet<Type> { typeof(double) },
            };

            _implicitCastCheckCache = new Dictionary<Type, HashSet<MethodInfo>>();
        }

        public const string ImplicitOperatorName = "op_Implicit";

        private static HashSet<MethodInfo> GetImplicitCastHashSet(Type type) {
            if (_implicitCastCheckCache.TryGetValue(type, out var hs)) {
                return hs;
            } else {
                hs = new HashSet<MethodInfo>();
                _implicitCastCheckCache.Add(type, hs);

                return hs;
            }
        }
        private static void EnsureImplicitCastCache(Type type) {
            if (!_implicitCastCheckCache.ContainsKey(type)) {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

                foreach (var method in methods) {
                    if (method.Name == ImplicitOperatorName) {
                        // public static implicit operator <return type>(<from type> from);

                        if (method.GetParameters()[0].ParameterType == type) {
                            GetImplicitCastHashSet(type).Add(method);
                        }
                    }
                }
            }
        }

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

        public static Type[] GetParameterTypes(this MethodInfo method) {
            var parameters = method.GetParameters();
            Type[] types = new Type[parameters.Length];

            for (int i = 0; i < types.Length; i++) {
                types[i] = parameters[i].ParameterType;
            }

            return types;
        }

        private static IEnumerable<MethodInfo> PickCompatibleMethods_Internal(object[] parameters, MethodInfo[] methods) {
            foreach (var method in methods) {
                var parameterInfos = method.GetParameters();

                if (parameterInfos.Length != parameters.Length) continue;

                for (int i = 0; i < parameters.Length; i++) {
                    var correspondParamType = parameterInfos[i].ParameterType;

                    if (parameters[i] == null) {
                        if (correspondParamType.IsPrimitive) {
                            yield break;
                        }
                    } else {
                        var parameterType = parameters[i].GetType();

                        if (correspondParamType == parameterType) continue;

                        if (correspondParamType.IsPrimitive) {
                            var implicitCastTable = _primitiveImplicitCasts[correspondParamType];

                            if (!_primitiveImplicitCasts.ContainsKey(parameterType)) {
                                yield break;
                            }
                        } else {
                            if (!parameterType.IsSubclassOf(correspondParamType)) {
                                yield break;
                            }
                        }
                    }
                }

                yield return method;
            }
        }

        public static MethodInfo[] PickCompatibleMethods(object[] parameters, MethodInfo[] methods) {
            return PickCompatibleMethods_Internal(parameters, methods).ToArray();
        }

        /// <summary>
        /// Pick the most compatible method with as less casting as possible. See <seealso cref="PickCompatibleMethods"/>
        /// </summary>
        /// <param name="parameters">Parameters used to invoke method</param>
        /// <param name="candidates">Candidated method after filtered</param>
        /// <returns>The most compatible method that can be invoked. Return null if cannot be found or ambiguous.</returns>
        public static MethodInfo PickTheMostCompatibleMethod(object[] parameters, MethodInfo[] candidates) {
            if (candidates.Length == 1) return candidates[0];
            if (parameters.Length == 0) {
                return candidates.FirstOrDefault(x => x.GetParameters().Length == 0);
            }

            MethodInfo mostCompatible = null;
            int compatibleScore = 0;

            bool isAmbiguous = false;

            foreach (var candidate in candidates) {
                var paramInfos = candidate.GetParameters();

                int candidateScore = 0;

                for (int i = 0; i < parameters.Length; i++) {
                    if (parameters[i] == null) continue;

                    var parameterType = parameters[i].GetType();

                    if (parameterType == paramInfos[i].ParameterType) {
                        candidateScore += 3; // Testing purpose
                        continue;
                    }

                    if (parameterType.IsSubclassOf(paramInfos[i].ParameterType)) {
                        candidateScore += 1; // Testing purpose
                        continue;
                    }

                    if (parameterType.IsPrimitive) {
                        if (_primitiveImplicitCasts[parameterType].Contains(paramInfos[i].ParameterType)) {
                            candidateScore += 2;
                            continue;
                        }
                    }
                }

                if (candidateScore == compatibleScore) {
                    isAmbiguous = true;
                } else if (candidateScore > compatibleScore) {
                    isAmbiguous = false;
                    compatibleScore = candidateScore;
                    mostCompatible = candidate;
                }
            }

            return isAmbiguous ? null : mostCompatible;
        }

        public static string DebugMethodInfo(MethodInfo method) {
            return method.Name + "(" + string.Join(", ", method.GetParameters().Select(x => x.ParameterType.FullName)) + ")";
        }
        public static bool IsOrSubclassOf(this Type type, Type parent) {
            return type == parent || type.IsSubclassOf(parent);
        }

        public static Type FindNearestAncestor(Type type, params Type[] others) {
            Type nearest = null;
            int score = int.MaxValue;

            List<Type> flattenedHierarchy = new List<Type>();

            var parent = type;
            while (parent != null) {
                flattenedHierarchy.Add(parent);
                parent = parent.BaseType;
            }

            foreach (var other in others.Where(x => x == type || type.IsSubclassOf(x))) {
                if (type == other) return type;

                int search = flattenedHierarchy.IndexOf(other);

                if (search != -1 && search < score) {
                    nearest = flattenedHierarchy[search];

                    score = search;
                }
            }

            return nearest;
        }

        public static bool IsImplicitCastable(Type from, Type to) {
            if (from.IsPrimitive) {
                return _primitiveImplicitCasts[from].Contains(to);
            } else {
                EnsureImplicitCastCache(from);

                return _implicitCastCheckCache[from].Any(x => x.ReturnType == to);
            }
        }
    }
}