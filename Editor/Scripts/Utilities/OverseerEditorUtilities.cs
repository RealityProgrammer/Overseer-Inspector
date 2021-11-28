using System;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Runtime.Validation;
using RealityProgrammer.OverseerInspector.Editors.Drawers;
using RealityProgrammer.OverseerInspector.Editors.Drawers.Group;

namespace RealityProgrammer.OverseerInspector.Editors.Utility {
    public static class OverseerEditorUtilities {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.Conditional("OVERSEER_INSPECTOR_RPDEVDEBUG")]
        internal static void RPDevelopmentDebug(string message) {
            Debug.Log(message);
        }

        public static bool CheckValidations(IEnumerable<ConditionalValidationAttribute> conditions, object target) {
            foreach (var condition in conditions) {
                bool validate = condition.Validation(target);

                if (!validate) {
                    return false;
                }
            }

            return true;
        }

        private static readonly Dictionary<string, Color> _colors = new Dictionary<string, Color> {
            ["red"] = Color.red,
            ["green"] = Color.green,
            ["blue"] = Color.blue,
            ["yellow"] = Color.yellow,
            ["orange"] = new Color(1, 0.5f, 0),
        };
        public static bool TryHandleColorString(string input, out Color value) {
            value = default;

            if (input.StartsWith("#")) {
                string substring = input.Substring(1, input.Length - 1);
                int colorMask;

                switch (input.Length) {
                    case 7:
                        if (int.TryParse(substring, NumberStyles.HexNumber, null, out colorMask)) {
                            value = new Color32((byte)((colorMask & 0x00FF0000) >> 16), (byte)((colorMask & 0x0000FF00) >> 8), (byte)(colorMask & 0x0000FF), 255);
                            return true;
                        }

                        Debug.LogWarning("Cannot parse color hex of #" + substring + ".");
                        break;

                    case 9:
                        if (int.TryParse(substring, NumberStyles.HexNumber, null, out colorMask)) {
                            value = new Color32((byte)((colorMask & 0xFF000000) >> 24), (byte)((colorMask & 0x00FF0000) >> 16), (byte)((colorMask & 0x0000FF00) >> 8), (byte)(colorMask & 0x000000FF));
                            return true;
                        }

                        Debug.LogWarning("Cannot parse color hex of #" + substring + ".");
                        break;

                    default:
                        Debug.LogWarning("Hex color code can only be either 6 or 8 hex characters.");
                        break;
                }
            } else if (input.StartsWith("rgba(") && input.EndsWith(")")) {
                string[] parameters = input.Substring(5, input.Length - 6).Split(',');

                if (parameters.Length == 0) {
                    Debug.LogWarning("Cannot parse string via rgba because no parameter provided");

                    value = default;
                    return false;
                }

                for (int i = 0; i < Mathf.Min(4, parameters.Length); i++) {
                    string treated = parameters[i].Trim();

                    if (treated.IndexOf('.') >= 0) {
                        if (float.TryParse(parameters[i], out float channelValue)) {
                            value[i] = channelValue;
                        } else {
                            Debug.LogWarning("Cannot parse the floating point number parameter of: " + treated);
                            value[i] = 0;
                        }
                    } else {
                        if (int.TryParse(parameters[i], out int channelValue)) {
                            value[i] = channelValue / 255f;
                        } else {
                            value[i] = 0;
                            Debug.LogWarning("Cannot parse the integer number parameter of: " + treated);
                        }
                    }

                    value[i] = Mathf.Clamp01(value[i]);
                }

                return true;
            } else if (input.StartsWith("rgb(") && input.EndsWith(")")) {
                string[] parameters = input.Substring(4, input.Length - 5).Split(',');

                if (parameters.Length == 0) {
                    Debug.LogWarning("Cannot parse string via rgb because no parameter provided");

                    value = default;
                    return false;
                }

                for (int i = 0; i < Mathf.Min(3, parameters.Length); i++) {
                    string treated = parameters[i].Trim();

                    if (treated.IndexOf('.') >= 0) {
                        if (float.TryParse(treated, out float channelValue)) {
                            value[i] = channelValue;
                        } else {
                            Debug.LogWarning("Cannot parse the floating point number parameter of: " + parameters[i] + " index " + i + "/" + parameters.Length + " (" + input + ")");
                            value[i] = 0;
                        }
                    } else {
                        if (int.TryParse(treated, out int channelValue)) {
                            value[i] = channelValue / 255f;
                        } else {
                            value[i] = 0;
                            Debug.LogWarning("Cannot parse the integer number parameter of: " + parameters[i] + " index " + i + "/" + parameters.Length + " (" + input + ")");
                        }
                    }

                    value[i] = Mathf.Clamp01(value[i]);
                }

                value.a = 1;
                return true;
            } else if (_colors.TryGetValue(input, out value)) {
                return true;
            }

            return false;
        }
        public static void DebugControlRect(Color col, float w = 0, float h = 0) {
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(GUILayout.Width(w), GUILayout.Height(h)), col);
        }

        public static List<BaseDisplayable> AutoInitializeInspector(SerializedObject serializedObject) {
            List<BaseDisplayable> _displayables = new List<BaseDisplayable>();

            var allFields = CachingUtilities.GetAllCachedFields(serializedObject, false);

            for (int i = 0; i < allFields.Count; i++) {
                var field = allFields[i];

                if (field.BeginGroups != null) {
                    GroupBuildingUtilities.BeginStackGroupTest(allFields, allFields[i], ref i, out var outputDrawer);

                    if (outputDrawer != null) {
                        _displayables.Add(outputDrawer);
                    }
                } else {
                    if (TryHandlePropertyDrawer(field, out var drawer)) {
                        _displayables.Add(drawer);
                    }
                }
            }

            return _displayables;
        }

        internal static bool StackPopMultiple<T>(Stack<T> stack, int count) {
            if (stack.Count <= count)
                return false;

            for (int i = 0; i < count; i++) {
                stack.Pop();
            }

            return true;
        }

        internal static bool SearchAndIncrementIndex(List<SerializedFieldContainer> all, SerializedFieldContainer search, ref int index) {
            if (index >= all.Count)
                return false;

            while (!ReferenceEquals(all[index], search)) {
                index++;

                if (index >= all.Count) {
                    return false;
                }
            }

            return true;
        }

        public static bool TryHandlePropertyDrawer(SerializedFieldContainer field, out BaseAttributeDrawer drawer) {
            if (field.PrimaryDrawerAttribute != null) {
                bool tryCreate = AttributeDrawerCollector.TryCreateDrawerInstance(field.PrimaryDrawerAttribute.GetType(), field.PrimaryDrawerAttribute, field, out var drawerInstance);

                if (tryCreate) {
                    if (drawerInstance is BasePrimaryAttributeDrawer _cast) {
                        foreach (var addition in HandleAdditionDrawers(field)) {
                            _cast.AddChild(addition);
                        }

                        drawer = _cast;
                        return true;
                    }
                }

                drawer = null;
                return false;
            } else {
                AttributeDrawerCollector.TryCreateDrawerInstance(null, null, field, out var drawerInstance);

                drawer = drawerInstance;
                foreach (var addition in HandleAdditionDrawers(field)) {
                    drawer.AddChild(addition);
                }

                return true;
            }
        }

        public static IEnumerable<BaseAttributeDrawer> HandleAdditionDrawers(SerializedFieldContainer field) {
            if (field.Additionals != null) {
                foreach (var attr in field.Additionals) {
                    bool tryCreate = AttributeDrawerCollector.TryCreateDrawerInstance(attr.GetType(), attr, field, out var drawerInstance);

                    if (tryCreate) {
                        if (drawerInstance is BaseAttributeDrawer drawer) {
                            yield return drawer;
                        }
                    }
                }
            }
        }

        public delegate object TypeLayoutDrawerDelegate(object input, string label);
        private static readonly Dictionary<Type, TypeLayoutDrawerDelegate> _typeLayoutDrawer = new Dictionary<Type, TypeLayoutDrawerDelegate>() {
            [typeof(int)] = (input, label) => {
                return EditorGUILayout.IntField(label, (int)input);
            },
            [typeof(string)] = (input, label) => {
                return EditorGUILayout.TextField(label, (string)input);
            },
            [typeof(bool)] = (input, label) => {
                return EditorGUILayout.Toggle(label, (bool)input);
            },
            [typeof(float)] = (input, label) => {
                return EditorGUILayout.FloatField(label, (float)input);
            },
            [typeof(byte)] = (input, label) => {
                return Mathf.Clamp(EditorGUILayout.IntField(label, (int)input), byte.MinValue, byte.MaxValue);
            },
            [typeof(sbyte)] = (input, label) => {
                return Mathf.Clamp(EditorGUILayout.IntField(label, (int)input), sbyte.MinValue, sbyte.MaxValue);
            },
            [typeof(char)] = (input, label) => {
                var cast = ((char)input).ToString();
                var field = EditorGUILayout.TextField(label, cast);

                return field.Length == 0 ? '\0' : field[0];
            },
            [typeof(double)] = (input, label) => {
                return EditorGUILayout.DoubleField(label, (double)input);
            },
            [typeof(uint)] = (input, label) => {
                return Mathf.Clamp(EditorGUILayout.LongField(label, (uint)input), uint.MinValue, uint.MaxValue);
            },
            [typeof(long)] = (input, label) => {
                return EditorGUILayout.LongField(label, (long)input);
            },
            [typeof(short)] = (input, label) => {
                return Mathf.Clamp(EditorGUILayout.LongField(label, (short)input), short.MinValue, short.MaxValue);
            },
            [typeof(ushort)] = (input, label) => {
                return Mathf.Clamp(EditorGUILayout.LongField(label, (ushort)input), ushort.MinValue, ushort.MaxValue);
            },
            [typeof(Vector2)] = (input, label) => {
                return EditorGUILayout.Vector2Field(label, (Vector2)input);
            },
            [typeof(Vector3)] = (input, label) => {
                return EditorGUILayout.Vector3Field(label, (Vector3)input);
            },
            [typeof(Vector2Int)] = (input, label) => {
                return EditorGUILayout.Vector2IntField(label, (Vector2Int)input);
            },
            [typeof(Vector3Int)] = (input, label) => {
                return EditorGUILayout.Vector3IntField(label, (Vector3Int)input);
            },
            [typeof(Vector4)] = (input, label) => {
                return EditorGUILayout.Vector4Field(label, (Vector4)input);
            },
            [typeof(Quaternion)] = (input, label) => {
                var q = (Quaternion)input;

                EditorGUILayout.LabelField(label);
                q.x = Mathf.Clamp01(EditorGUILayout.FloatField("X", q.x));
                q.y = Mathf.Clamp01(EditorGUILayout.FloatField("Y", q.y));
                q.z = Mathf.Clamp01(EditorGUILayout.FloatField("Z", q.z));
                q.w = Mathf.Clamp01(EditorGUILayout.FloatField("W", q.w));

                return q;
            },
            [typeof(Rect)] = (input, label) => {
                return EditorGUILayout.RectField(label, (Rect)input);
            },
            [typeof(RectInt)] = (input, label) => {
                return EditorGUILayout.RectIntField(label, (RectInt)input);
            },
            [typeof(Bounds)] = (input, label) => {
                return EditorGUILayout.BoundsField(label, (Bounds)input);
            },
            [typeof(BoundsInt)] = (input, label) => {
                return EditorGUILayout.BoundsIntField(label, (BoundsInt)input);
            },
            [typeof(Matrix4x4)] = (input, label) => {
                var q = (Matrix4x4)input;

                EditorGUILayout.LabelField(label);

                EditorGUILayout.BeginVertical();
                for (int y = 0; y < 4; y++) {
                    EditorGUILayout.BeginHorizontal();
                    for (int x = 0; x < 4; x++) {
                        q[y, x] = EditorGUILayout.FloatField(q[y, x]);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();

                return q;
            },
            [typeof(Color32)] = (input, label) => {
                return EditorGUILayout.ColorField(label, (Color32)input);
            },
            [typeof(Color)] = (input, label) => {
                return EditorGUILayout.ColorField(label, (Color)input);
            },
            [typeof(LayerMask)] = (input, label) => {
                return EditorGUILayout.LayerField(label, (LayerMask)input);
            },
            [typeof(AnimationCurve)] = (input, label) => {
                return EditorGUILayout.CurveField(label, (AnimationCurve)input);
            },
            [typeof(Gradient)] = (input, label) => {
                return EditorGUILayout.GradientField(label, (Gradient)input);
            },
        };
        public static object DrawLayoutBasedOnType(object input, Type type, string label) {
            if (input is UnityEngine.Object) {
                return EditorGUILayout.ObjectField(label, (UnityEngine.Object)input, input.GetType(), true);
            } else {
                if (_typeLayoutDrawer.TryGetValue(type, out var @delegate)) {
                    return @delegate.Invoke(input, label); 
                }

                return null;
            }
        }
    }
}