using System;
using System.Collections.Generic;
using UnityEngine;

public class SeperatorAttribute : AdditionDrawerAttribute
{
    public float Normalize { get; private set; } = 1;

    public int Height { get; set; } = 1;
    public string ColorParameter { get; set; } = "#CDCDCD";

    public SeperatorAttribute(float normalize) {
        Normalize = Mathf.Clamp01(normalize);
    }
}
