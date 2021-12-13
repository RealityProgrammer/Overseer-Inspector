using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Runtime.Drawers;
using RealityProgrammer.OverseerInspector.Editors.Attributes;

namespace RealityProgrammer.OverseerInspector.Editors.Drawers {
    [BindDrawerTo(typeof(MessageBoxAttribute))]
    public class MessageBoxDrawer : BaseAttributeDrawer {
        public const string InfoIconArgument = "Info";
        public const string WarningIconArgument = "Warning";
        public const string ErrorIconArgument = "Error";

        private MessageBoxAttribute underlying;

        private static readonly Dictionary<string, Texture2D> _textureCache;

        static MessageBoxDrawer() {
            _textureCache = new Dictionary<string, Texture2D>();
        }

        public override void DrawLayout() {
            if (!AssociatedMember.ConditionalCheck)
                return;

            DrawAllChildsLayout();

            if (underlying == null) {
                underlying = (MessageBoxAttribute)AssociatedAttribute;
            }

            switch (underlying.IconType) {
                case MessageBoxIconType.Info:
                default:
                    EditorGUILayout.HelpBox(underlying.Message, MessageType.Info, true);
                    break;

                case MessageBoxIconType.Warning:
                    EditorGUILayout.HelpBox(underlying.Message, MessageType.Warning, true);
                    break;

                case MessageBoxIconType.Error:
                    EditorGUILayout.HelpBox(underlying.Message, MessageType.Error, true);
                    break;

                case MessageBoxIconType.Custom:
                    EditorGUILayout.LabelField(new GUIContent(underlying.Message, RetrieveCacheTexture(underlying.IconArgument)), EditorStyles.helpBox);
                    break;
            }
        }

        private static Texture2D RetrieveCacheTexture(string path) {
            if (_textureCache.TryGetValue(path, out var output)) {
                return output;
            } else {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture == null) {
                    Debug.LogWarning("Cannot load message box texture at path " + path + ". Falling back to something...");
                    texture = Resources.Load<Texture2D>("MessageBoxFallback");
                }

                _textureCache.Add(path, texture);
                return texture;
            }
        }
    }
}