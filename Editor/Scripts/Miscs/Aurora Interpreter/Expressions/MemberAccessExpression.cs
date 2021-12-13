using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public class MemberAccessExpression : BaseExpression {
        public BaseExpression Expression { get; private set; }
        public LexerToken Name { get; private set; }

        public override ExpressionType ExprType => ExpressionType.MemberAccess;

        public MemberAccessExpression(BaseExpression expression, LexerToken name) {
            Expression = expression;
            Name = name;
        }


        public override object Interpret(AuroraInterpreter interpreter) {
            return interpreter.Interpret(this);
        }
    }
}