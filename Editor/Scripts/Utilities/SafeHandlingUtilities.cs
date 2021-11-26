using System;
using System.Collections.Generic;
using UnityEngine;

internal static class SafeHandlingUtilities {
    public static void SafeWhileLoop(Func<bool> condtion, Func<int, bool> code, int maxIteration, string warn = null) {
        int iteration = 0;

        while (condtion()) {
            if (iteration < maxIteration) {
                if (code(iteration)) {
                    break;
                }

                iteration++;
            } else {
                Debug.LogWarning("SafeHandlingUtilities: " + (warn ?? "While loop iteration exceeded. Max iteration count: " + maxIteration));
                break;
            }
        }
    }
}
