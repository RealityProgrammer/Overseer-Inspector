using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    /// <summary>
    /// <p>Determine numerical type of a number</p>
    /// 
    /// <p>The first 24 bits is enough to store the type, and the last 8 bit will be act as a flag.</p>
    /// <p>31th bit: Unsigned</p>
    /// </summary>
    public enum NumericType : int {
        NAN, Byte, Short, Int, Long, Float, Double, Decimal,

        Unsigned = 1 << 31,
    }
}