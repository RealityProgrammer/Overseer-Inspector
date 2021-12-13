using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public class UndefinedBinaryOperatorException : Exception {
        public UndefinedBinaryOperatorException(string msg) : base(msg) {
        }
    }
}