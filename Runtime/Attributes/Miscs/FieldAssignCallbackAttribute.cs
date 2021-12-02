using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Miscs {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public sealed class FieldAssignCallbackAttribute : BaseOverseerAttribute {
        public string CallbackArgument { get; private set; }
        
        public FieldAssignCallbackAttribute(string argument) {
            CallbackArgument = argument;
        }
    }
}