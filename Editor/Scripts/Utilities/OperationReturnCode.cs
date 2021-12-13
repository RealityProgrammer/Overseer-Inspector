using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Utility {
    // I've use WinAPI for too long
    public enum OperationReturnCode {
        Success, NotSuccess_Safe, UnexpectedReturnValue,

        Custom = int.MaxValue,
    }
}