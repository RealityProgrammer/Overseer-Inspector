using System;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class BeginTabGroupAttribute : OverseerBeginGroupAttribute
{
    public BeginTabGroupAttribute(string path) {
        Name = path;
    }

    public override string ToString() {
        return "BeginTabGroupAttribute(" + Name + ", " + ID + ")";
    }
}