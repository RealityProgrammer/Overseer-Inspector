using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public class ExpectAnExpressionException : Exception {
        public ExpectAnExpressionException(string msg) : base(msg) { }
    }
}