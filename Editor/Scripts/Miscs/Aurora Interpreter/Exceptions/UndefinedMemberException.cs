using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public class UndefinedMemberException : Exception {
        public UndefinedMemberException(string name, string type) : base("Member '" + name + "' doesn't exists in type '" + type + "'") { }
    }
}