using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public class MethodCallExpression : BaseExpression {
        /// <summary>
        /// Expected a MemberAccessExpression
        /// </summary>
        public BaseExpression Expression { get; private set; }
        public List<BaseExpression> Parameters { get; private set; }

        public override ExpressionType ExprType => ExpressionType.MethodCall;

        public MethodCallExpression(BaseExpression expr, List<BaseExpression> parameters) {
            Expression = expr;
            Parameters = parameters;
        }

        public override object Interpret(AuroraInterpreter interpreter) {
            return interpreter.Interpret(this);
        }
    }
}