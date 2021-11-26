using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
        if (!AssociatedField.LastValidation) return;

        DrawAllChildsLayout();

        if (underlying == null) {
            underlying = (MessageBoxAttribute)AssociatedAttribute;
        }

        switch (underlying.IconArgument) {
            case InfoIconArgument:
            case null:
            case "":
                EditorGUILayout.HelpBox(underlying.Message, MessageType.Info, true);
                break;

            case WarningIconArgument:
                EditorGUILayout.HelpBox(underlying.Message, MessageType.Warning, true);
                break;

            case ErrorIconArgument:
                EditorGUILayout.HelpBox(underlying.Message, MessageType.Error, true);
                break;

            default:
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
