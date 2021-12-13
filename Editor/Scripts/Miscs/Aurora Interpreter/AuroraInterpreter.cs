using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.CSharp.RuntimeBinder;
using RealityProgrammer.OverseerInspector.Editors.Utility;
using System;
using System.Linq;
using System.Reflection;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public sealed class AuroraInterpreter {
        // Dealing with ambiguous is pain
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> _cachedFields;
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _cachedProperties;
        private static readonly Dictionary<Type, Dictionary<string, MethodInfo[]>> _cachedMethods;

        static AuroraInterpreter() {
            _cachedFields = new Dictionary<Type, Dictionary<string, FieldInfo>>();
            _cachedProperties = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
            _cachedMethods = new Dictionary<Type, Dictionary<string, MethodInfo[]>>();
        }

        private static void EnsureMemberCache(Type type, out Dictionary<string, FieldInfo> fields, out Dictionary<string, PropertyInfo> properties, out Dictionary<string, MethodInfo[]> methods) {
            if (!_cachedFields.TryGetValue(type, out fields)) {
                fields = new Dictionary<string, FieldInfo>();
                _cachedFields.Add(type, fields);
            }

            if (!_cachedProperties.TryGetValue(type, out properties)) {
                properties = new Dictionary<string, PropertyInfo>();
                _cachedProperties.Add(type, properties);
            }

            if (!_cachedMethods.TryGetValue(type, out methods)) {
                methods = new Dictionary<string, MethodInfo[]>();
                _cachedMethods.Add(type, methods);
            }
        }

        public object Target { get; private set; }

        public void BindInterpretingTarget(object target) {
            Target = target;
        }

        public object InterpretExpression(BaseExpression expression) {
            return Evaluate(expression);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object Evaluate(BaseExpression expr) {
            return expr.Interpret(this);
        }

        public object Interpret(BinaryExpression binary) {
            dynamic lhs = Evaluate(binary.Left);
            dynamic rhs = Evaluate(binary.Right);

            if (lhs.GetType().IsEnum) {
                lhs = (long)lhs;
            }

            try {
                switch (binary.Operator.Type) {
                    case TokenType.Plus:
                        return lhs + rhs;
                    case TokenType.Minus:
                        return lhs - rhs;
                    case TokenType.Star:
                        return lhs * rhs;
                    case TokenType.Slash:
                        return lhs / rhs;
                    case TokenType.Percentage:
                        return lhs % rhs;
                    case TokenType.Greater:
                        return lhs > rhs;
                    case TokenType.GreaterEqual:
                        return lhs >= rhs;
                    case TokenType.Less:
                        return lhs < rhs;
                    case TokenType.LessEqual:
                        return lhs <= rhs;
                    case TokenType.EqualEqual:
                        return lhs == rhs;
                    case TokenType.BangEqual:
                        return lhs != rhs;
                    case TokenType.ConditionalAnd:
                        return lhs && rhs;
                    case TokenType.ConditionalOr:
                        return lhs || rhs;
                    case TokenType.BitwiseAnd:
                        return lhs & rhs;
                    case TokenType.BitwiseOr:
                        return lhs | rhs;
                    case TokenType.BitwiseXor:
                        return lhs ^ rhs;
                    case TokenType.BitwiseLeftShift:
                        return lhs << rhs;
                    case TokenType.BitwiseRightShift:
                        return lhs >> rhs;
                    default:
                        throw new UndefinedBinaryOperatorException("Operator '" + binary.Operator.Type + "' cannot be applied to operands of type '" + GetObjectTypeName(lhs) + "' and '" + GetObjectTypeName(rhs) + "'");
                }
            } catch (RuntimeBinderException) {
                throw new UndefinedBinaryOperatorException("Operator '" + binary.Operator.Type + "' cannot be applied to operands of type '" + GetObjectTypeName(lhs) + "' and '" + GetObjectTypeName(rhs) + "'");
            }
        }

        public object Interpret(UnaryExpression unary) {
            dynamic value = Evaluate(unary.Expression);

            try {
                switch (unary.Operator.Type) {
                    case TokenType.Minus: return -value;
                    case TokenType.Bang: return !value;
                    case TokenType.BitwiseComplement: return ~value;
                    default:
                        throw new UndefinedUnaryOperatorException("Unary operator '" + unary.Operator.Type + "' cannot be applied to operand of type '" + GetObjectTypeName(value) + "'");
                }
            } catch (RuntimeBinderException) {
                throw new UndefinedUnaryOperatorException("Unary operator '" + unary.Operator.Type + "' cannot be applied to operand of type '" + GetObjectTypeName(value) + "'");
            }
        }

        public object Interpret(LiteralExpression literal) {
            return literal.Literal;
        }

        public object Interpret(MethodCallExpression call) {
            object value = Evaluate(call.Expression);

            if (value is MethodInfo[] methods) {
                if (methods.Length != 0) {
                    object[] args = new object[call.Parameters.Count];
                    for (int i = 0; i < args.Length; i++) {
                        args[i] = Evaluate(call.Parameters[i]);
                    }

                    var compatibles = ReflectionUtilities.PickCompatibleMethods(args, methods);
                    var mostCompatible = ReflectionUtilities.PickTheMostCompatibleMethod(args, compatibles);

                    if (mostCompatible != null) {
                        return mostCompatible.Invoke(Target, args);
                    } else {
                        Debug.LogError("Trying to interpret MethodCallExpression, but cannot find most compatible overload. This might be caused by Ambiguous, or an internal bug.");
                        return null;
                    }
                }
            }

            Debug.LogWarning("Trying to interpret MethodCallExpression, but seems like the carrying Expression is not a MemberAccessExpression (probably internal error)");
            return null;
        }

        public object Interpret(MemberAccessExpression expression) {
            var obj = Evaluate(expression.Expression);

            if (obj != null) {
                Type type = obj.GetType();

                string name = expression.Name.Lexeme;

                EnsureMemberCache(type, out var fd, out var pd, out var md);

                FieldInfo field;
                PropertyInfo property;
                MethodInfo[] methods;

                // Fields, Properties will be treated as value, but method will be treated as MethodInfo, so that we can invoke it via MethodCallExpression

                if (fd.TryGetValue(name, out field)) {
                    return field.GetValue(obj);
                } else if (pd.TryGetValue(name, out property)) {
                    return property.GetValue(obj);
                } else if (md.TryGetValue(name, out methods)) {
                    return methods;
                }

                field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

                if (field != null) {
                    fd.Add(name, field);
                    return field.GetValue(obj);
                } else {
                    property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic);

                    if (property != null) {
                        pd.Add(name, property);
                        return property.GetValue(obj);
                    } else {
                        methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.Name == name).ToArray();

                        if (methods.Length != 0) {
                            md.Add(name, methods);
                            return methods;
                        }
                    }
                }

                throw new UndefinedMemberException(name, type.FullName);
            }

            throw new NullReferenceException("Trying to access instance member '" + expression.Name.Lexeme + "' of null instance");
        }

        public object Interpret(IndexerExpression indexer) {
            object value = Evaluate(indexer.Expression);

            if (value != null) {
                object[] args = new object[indexer.Parameters.Count];
                for (int i = 0; i < args.Length; i++) {
                    args[i] = Evaluate(indexer.Parameters[i]);
                }

                var allIndexerMethods = value.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty).Where(x => x.Name == "get_Item");

                if (allIndexerMethods.Any()) {
                    var compatibles = ReflectionUtilities.PickCompatibleMethods(args, allIndexerMethods.ToArray());
                    var mostCompatible = ReflectionUtilities.PickTheMostCompatibleMethod(args, compatibles);

                    if (mostCompatible != null) {
                        return mostCompatible.Invoke(value, args);
                    } else {
                        Debug.LogError("Trying to interpret IndexerExpression, but cannot find most compatible overload. This might be caused by Ambiguous, or an internal bug.");
                        return null;
                    }
                } else {
                    throw new UndefinedMemberException("Indexer", value.GetType().FullName);
                }
            }

            throw new NullReferenceException("Trying to interpret IndexerExpression on a null instance");
        }

        private string GetObjectTypeName(object obj) {
            return obj == null ? "<null>" : obj.GetType().FullName;
        }

        public object Interpret(GroupingExpression group) {
            return group.Expression.Interpret(this);
        }
    }
}