using System;
using System.Collections.Generic;
using UnityEngine;

public class BeginFoldoutGroupAttribute : OverseerBeginGroupAttribute {
    public FontStyle FontStyle { get; set; } = FontStyle.Normal;

    public BeginFoldoutGroupAttribute(string name) {
        Name = name;
    }

    public override string ToString() {
        return "BeginFoldoutGroupAttribute(" + Name + ")";
    }
}
