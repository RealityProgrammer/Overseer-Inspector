using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public class UnexpectedCharacterException : Exception {
        public UnexpectedCharacterException(int position, char c) : base($"Unexpected character at position {position - 1}: {c}") { }
    }
}