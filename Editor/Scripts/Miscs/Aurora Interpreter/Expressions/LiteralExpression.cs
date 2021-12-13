using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public class LiteralExpression : BaseExpression {
        public LexerToken Token { get; private set; }
        public object Literal { get; private set; }

        public override ExpressionType ExprType => ExpressionType.Literal;

        public LiteralExpression(object literal, LexerToken token) {
            Literal = literal;
            Token = token;
        }

        public override object Interpret(AuroraInterpreter interpreter) {
            return interpreter.Interpret(this);
        }
    }
}