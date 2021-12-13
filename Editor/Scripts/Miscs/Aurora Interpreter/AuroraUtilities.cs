using System.Collections;
using System.Runtime.CompilerServices;
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public static class AuroraUtilities {
        public const string TreeVerticalBranch = "\u2551  ";
        public const string TreeLastBranch = "\u255A\u2550\u2550";
        public const string TreeNonLastBranch = "\u2560\u2550\u2550";

        public static string DebugExpressionTree(BaseExpression expr) {
            StringBuilder sb = new StringBuilder();

            DebugExpressionTree(expr, sb);

            return sb.ToString();
        }

        private static void DebugExpressionTree(BaseExpression expr, StringBuilder output, string indent = "", bool isLast = true) {
            var b = isLast ? TreeLastBranch : TreeNonLastBranch;

            output.Append(indent).Append(b);

            switch (expr) {
                case LiteralExpression literal:
                    output.Append("Literal: ").Append(literal.Literal).AppendLine();
                    //output.AppendLine(literal.GetType().Name);
                    //indent += isLast ? "   " : TreeVerticalBranch;
                    //output.Append(indent).Append(TreeLastBranch).Append("Value: ").Append(literal.Literal.ToString()).Append(" (").Append(literal.Literal.GetType().FullName).AppendLine(")");
                    //output.Append(indent).Append(TreeLastBranch).Append("Token: ").AppendLine(literal.Token.ToString());
                    break;

                case UnaryExpression unary:
                    output.AppendLine(unary.GetType().Name);
                    indent += isLast ? "   " : TreeVerticalBranch;

                    output.Append(indent).Append(TreeNonLastBranch).AppendLine("Operator:" + unary.Operator.Type);
                    DebugExpressionTree(unary.Expression, output, indent, true);
                    break;

                case BinaryExpression binary:
                    output.AppendLine(binary.GetType().Name);
                    indent += isLast ? "   " : TreeVerticalBranch;
                    DebugExpressionTree(binary.Left, output, indent, false);
                    output.Append(indent).Append(TreeNonLastBranch).AppendLine("Operator:" + binary.Operator.Type);

                    DebugExpressionTree(binary.Right, output, indent, true);
                    break;

                case GroupingExpression group:
                    output.AppendLine(group.GetType().Name);
                    indent += isLast ? "   " : TreeVerticalBranch;

                    DebugExpressionTree(group.Expression, output, indent, true);
                    break;

                case MemberAccessExpression memberAccess:
                    output.AppendLine(memberAccess.GetType().Name);
                    indent += isLast ? "   " : TreeVerticalBranch;

                    DebugExpressionTree(memberAccess.Expression, output, indent, false);
                    output.Append(indent).Append(TreeLastBranch).Append("Name: ").AppendLine(memberAccess.Name.ToString());
                    break;

                case MethodCallExpression methodCall:
                    output.AppendLine(methodCall.GetType().Name);
                    indent += isLast ? "   " : TreeVerticalBranch;

                    if (methodCall.Parameters.Count == 0) {
                        DebugExpressionTree(methodCall.Expression, output, indent, true);
                    } else {
                        DebugExpressionTree(methodCall.Expression, output, indent, false);
                        output.Append(indent).Append(TreeLastBranch).Append("Parameter (").Append(methodCall.Parameters.Count).AppendLine("): ");

                        indent += "   ";

                        for (int i = 0; i < methodCall.Parameters.Count; i++) {
                            DebugExpressionTree(methodCall.Parameters[i], output, indent, i == methodCall.Parameters.Count - 1);
                        }
                    }
                    break;

                case IndexerExpression indexer:
                    output.AppendLine(indexer.GetType().Name);
                    indent += isLast ? "   " : TreeVerticalBranch;

                    if (indexer.Parameters.Count == 0) {
                        DebugExpressionTree(indexer.Expression, output, indent, true);
                    } else {
                        DebugExpressionTree(indexer.Expression, output, indent, false);
                        output.Append(indent).Append(TreeLastBranch).Append("Parameter (").Append(indexer.Parameters.Count).AppendLine("): ");

                        indent += "   ";

                        for (int i = 0; i < indexer.Parameters.Count; i++) {
                            DebugExpressionTree(indexer.Parameters[i], output, indent, i == indexer.Parameters.Count - 1);
                        }
                    }
                    break;
            }
        }

        public static Type GrabCorrespondNumericalType(NumericType nt) {
            bool unsigned = nt.IsUnsigned();

            switch (nt) {
                case NumericType.Byte: return unsigned ? typeof(byte) : typeof(sbyte);
                case NumericType.Short: return unsigned ? typeof(ushort) : typeof(short);
                case NumericType.Int: return unsigned ? typeof(uint) : typeof(int);
                case NumericType.Long: return unsigned ? typeof(ulong) : typeof(long);
                case NumericType.Float: return typeof(float);
                case NumericType.Double: return typeof(double);
                case NumericType.Decimal: return typeof(decimal);
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUnsigned(this NumericType nt) {
            return (int)(nt & NumericType.Unsigned) != 0;
        }

        public static void InitializeEverything(string program, object bindTarget, out AuroraScanner scanner, out AuroraLexer lexer, out BaseExpression lexed, out AuroraInterpreter interpreter) {
            scanner = new AuroraScanner();
            var tokens = scanner.Scan(program);

            lexer = new AuroraLexer(tokens);
            lexer.BindTarget(bindTarget);
            lexed = lexer.BeginLexing();

            interpreter = new AuroraInterpreter();
            interpreter.BindInterpretingTarget(bindTarget);
        }
    }
}