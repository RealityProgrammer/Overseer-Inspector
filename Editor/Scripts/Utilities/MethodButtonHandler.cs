using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static RealityProgrammer.OverseerInspector.Editors.Utility.CachingUtilities;

namespace RealityProgrammer.OverseerInspector.Editors.Utility {
    public abstract class MethodButtonHandler {
        public MethodButtonCache Cache { get; private set; }

        protected MethodButtonHandler(MethodButtonCache cache) {
            Cache = cache;
        }

        public abstract void Initialize();
        public abstract object Invoke(object target);
        public abstract void DrawLayoutParameters();
    }

    public sealed class MethodButtonReflectionInvokeHandler : MethodButtonHandler {
        public MethodButtonReflectionInvokeHandler(MethodButtonCache cache) : base(cache) { }

        public object[] Parameters { get; private set; }

        private static readonly HashSet<Type> _defaultInitialization = new HashSet<Type>() {
            typeof(AnimationCurve), typeof(Gradient), typeof(RectOffset), typeof(GUIContent),
        };
        public override void Initialize() {
            Parameters = new object[Cache.Parameters.Length];

            if (Cache.UseParameter) {
                for (int i = 0; i < Parameters.Length; i++) {
                    var pt = Cache.Parameters[i].ParameterType;

                    if (pt.IsValueType) {
                        Parameters[i] = Activator.CreateInstance(pt);
                    } else {
                        if (_defaultInitialization.Contains(pt)) {
                            Parameters[i] = Activator.CreateInstance(pt);
                        } else {
                            Parameters[i] = null;
                        }
                    }
                }
            }
        }

        public override object Invoke(object target) {
            return Cache.Method.Invoke(target, Parameters);
        }

        public override void DrawLayoutParameters() {
            for (int i = 0; i < Parameters.Length; i++) {
                Parameters[i] = OverseerEditorUtilities.DrawLayoutBasedOnType(Parameters[i], Cache.Parameters[i].ParameterType, Cache.Parameters[i].Name);
            }
        }
    }
}