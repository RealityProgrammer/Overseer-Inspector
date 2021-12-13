using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public class GroupingExpression : BaseExpression {
        public BaseExpression Expression { get; private set; }

        public override ExpressionType ExprType => ExpressionType.Grouping;

        public GroupingExpression(BaseExpression input) {
            Expression = input;
        }

        public override object Interpret(AuroraInterpreter interpreter) {
            return interpreter.Interpret(this);
        }
    }
}