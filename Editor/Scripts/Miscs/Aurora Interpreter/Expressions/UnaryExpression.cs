using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public class UnaryExpression : BaseExpression {
        public BaseExpression Expression { get; private set; }
        public LexerToken Operator { get; private set; }

        public override ExpressionType ExprType => ExpressionType.Unary;

        public UnaryExpression(LexerToken op, BaseExpression expr) {
            Operator = op;
            Expression = expr;
        }

        public override object Interpret(AuroraInterpreter interpreter) {
            return interpreter.Interpret(this);
        }
    }
}