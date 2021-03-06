using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Runtime.Drawers {
    /// <summary>
    /// Display a message box with customizable Icon
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class MessageBoxAttribute : AdditionDrawerAttribute {
        /// <summary>
        /// Display primary message
        /// </summary>
        public string Message { get; private set; }

        public MessageBoxIconType IconType { get; set; } = MessageBoxIconType.Info;

        private string _customIconPath;

        /// <summary>
        /// Customize the argument used for icon. Use built-in argument <c>"Info"</c>, <c>"Warning"</c>, <c>"Error"</c> for built-in Unity message icon, or use custom icon by input the texture asset path. Example: "Assets/Resources/etc..."
        /// </summary>
        public string IconArgument {
            get => _customIconPath;
            set {
                if (value == null) return;

                IconType = MessageBoxIconType.Custom;
                _customIconPath = value;
            }
        }

        public MessageBoxAttribute(string msg) {
            Message = msg;
        }
    }

    public enum MessageBoxIconType {
        Info, Warning, Error, Custom
    }
}