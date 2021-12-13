using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public class BinaryExpression : BaseExpression {
        public BaseExpression Left { get; private set; }
        public BaseExpression Right { get; private set; }
        public LexerToken Operator { get; private set; }

        public override ExpressionType ExprType => ExpressionType.Binary;

        public BinaryExpression(BaseExpression lhs, LexerToken op, BaseExpression rhs) {
            Left = lhs;
            Operator = op;
            Right = rhs;
        }

        public override object Interpret(AuroraInterpreter interpreter) {
            return interpreter.Interpret(this);
        }
    }
}