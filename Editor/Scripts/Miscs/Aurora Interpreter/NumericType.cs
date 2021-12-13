using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public enum NumericType {
        NAN, Byte, Short, Int, Long, Float, Double, Decimal,

        Unsigned = 1 << 31,
    }
}