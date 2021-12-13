using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public class UndefinedUnaryOperatorException : Exception {
        public UndefinedUnaryOperatorException(string msg) : base(msg) {
        }
    }
}