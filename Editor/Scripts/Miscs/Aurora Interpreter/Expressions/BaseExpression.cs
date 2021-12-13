using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public abstract class BaseExpression {
        public abstract ExpressionType ExprType { get; }

        public abstract object Interpret(AuroraInterpreter interpreter);
    }
}