using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Utility {
    public class MiscellaneousUtilities {
        // https://en.cppreference.com/w/cpp/numeric/has_single_bit
        public static bool IsSingleBit(int x) {
            return x != 0 && (x & (x - 1)) == 0;
        }

        public const double Log10_2 = 0.30102999566;

        public static int RightmostBitPosition(int x) {
            return (int)((Math.Log10(x & -x)) / Log10_2);
        }
    }
}