using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Drawers {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class VectorRangeAttribute : BasePrimaryDrawerAttribute {
        public AxisDisplayMode XDisplay { get; private set; } = AxisDisplayMode.Slider;
        public AxisDisplayMode YDisplay { get; private set; } = AxisDisplayMode.Slider;
        public AxisDisplayMode ZDisplay { get; private set; } = AxisDisplayMode.Slider;
        public AxisDisplayMode WDisplay { get; private set; } = AxisDisplayMode.Slider;

        public string XMinRange { get; set; }
        public string XMaxRange { get; set; }
        public string YMinRange { get; set; }
        public string YMaxRange { get; set; }
        public string ZMinRange { get; set; }
        public string ZMaxRange { get; set; }
        public string WMinRange { get; set; }
        public string WMaxRange { get; set; }

        public VectorRangeAttribute() : this(AxisDisplayMode.Field, AxisDisplayMode.Field, AxisDisplayMode.Field, AxisDisplayMode.Field) { }
        public VectorRangeAttribute(AxisDisplayMode xdisplay) : this(xdisplay, AxisDisplayMode.Field, AxisDisplayMode.Field, AxisDisplayMode.Field) {
        }
        public VectorRangeAttribute(AxisDisplayMode xdisplay, AxisDisplayMode ydisplay) : this(xdisplay, ydisplay, AxisDisplayMode.Field, AxisDisplayMode.Field) {
        }
        public VectorRangeAttribute(AxisDisplayMode xdisplay, AxisDisplayMode ydisplay, AxisDisplayMode zdisplay) : this(xdisplay, ydisplay, zdisplay, AxisDisplayMode.Field) {
        }
        public VectorRangeAttribute(AxisDisplayMode xdisplay, AxisDisplayMode ydisplay, AxisDisplayMode zdisplay, AxisDisplayMode wdisplay) {
            XDisplay = xdisplay;
            YDisplay = ydisplay;
            ZDisplay = zdisplay;
            WDisplay = wdisplay;
        }
    }

    public enum AxisDisplayMode {
        Field, Slider,
    }
}