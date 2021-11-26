using System;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public abstract class OverseerBeginGroupAttribute : BaseOverseerDrawerAttribute {
    public string Name { get; set; }
    public int ID { get; set; }
}