using System;
using System.Globalization;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class OverseerEditorUtilities
{
    private static readonly Action<BaseGroupAttributeDrawer, int> groupNestingLevelAssign;
    static OverseerEditorUtilities() {
        groupNestingLevelAssign = (Action<BaseGroupAttributeDrawer, int>)typeof(BaseGroupAttributeDrawer).GetProperty(nameof(BaseGroupAttributeDrawer.NestingLevel), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetSetMethod(true).CreateDelegate(typeof(Action<BaseGroupAttributeDrawer, int>));
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
                    Debug.LogWarning("Hex color code can only be either 6 or 8 digits.");
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

    public static Rect GetCurrentLayoutRect() {
        var r = EditorGUILayout.BeginVertical();
        EditorGUILayout.EndVertical();

        return r;
    }

    public static Rect GetNextRect() {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.EndVertical();

        var r = GUILayoutUtility.GetLastRect();
        r.y += r.height + EditorGUIUtility.standardVerticalSpacing - 1;

        return r;
    }

    public static List<BaseDisplayable> AutoInitializeInspector(SerializedObject serializedObject) {
        List<BaseDisplayable> _displayables = new List<BaseDisplayable>();

        var allFields = CachingUtilities.GetAllCachedFields(serializedObject, false);

        for (int i = 0; i < allFields.Count; i++) {
            var field = allFields[i];

            if (field.BeginGroups != null) {
                if (TryHandleGroupDrawer(serializedObject, field, out var end, out var drawer)) {
                    _displayables.Add(drawer);

                    if (end != null) {
                        while (!ReferenceEquals(allFields[i], end)) {
                            i++;

                            if (i >= allFields.Count)
                                break;
                        }
                    }
                }
            } else {
                if (TryHandlePropertyDrawer(field, out var drawer)) {
                    _displayables.Add(drawer);
                }
            }
        }

        return _displayables;
    }

    private static int groupCounter = -1;
    public static bool TryHandleGroupDrawer(SerializedObject so, SerializedFieldContainer begin, out SerializedFieldContainer groupEnd, out BaseAttributeDrawer drawer) {
        if (begin.BeginGroups == null) {
            if (TryHandlePropertyDrawer(begin, out drawer)) {
                groupEnd = null;
                return true;
            }

            groupEnd = null;
            return false;
        }

        if (begin.BeginGroups.Count == 1) {
            var groupAttr = begin.BeginGroups[0];

            bool tryCreate = AttributeDrawerCollector.TryCreateDrawerInstance(groupAttr.GetType(), groupAttr, begin, out var drawerInstance);

            if (tryCreate) {
                if (drawerInstance is BaseGroupAttributeDrawer groupDrawer) {
                    groupNestingLevelAssign(groupDrawer, ++groupCounter);

                    groupDrawer.EditorInitialize();

                    var allFields = CachingUtilities.GetAllCachedFields(so, false);
                    int index = 0;

                    while (true) {
                        if (ReferenceEquals(allFields[index], begin)) {
                            break;
                        }

                        index++;
                        if (index == allFields.Count) {
                            Debug.LogError("Something happened");
                            break;
                        }
                    }

                    if (TryHandlePropertyDrawer(allFields[index], out var beginFieldDrawer)) {
                        groupDrawer.AddChild(beginFieldDrawer);
                    }

                    if (allFields[index].IsEndGroup) {
                        groupCounter--;
                        groupEnd = begin;
                        drawer = groupDrawer;

                        return true;
                    }

                    index++;

                    while (index < allFields.Count) {
                        if (TryHandleGroupDrawer(so, allFields[index], out var nestedEnd, out var nestedDrawers)) {
                            groupDrawer.AddChild(nestedDrawers);
                        }

                        if (nestedEnd == null) {
                            if (allFields[index].IsEndGroup) {
                                groupCounter--;
                                groupEnd = allFields[index];
                                drawer = groupDrawer;

                                return true;
                            }
                        } else {
                            while (!ReferenceEquals(allFields[index], nestedEnd)) {
                                index++;

                                if (index >= allFields.Count)
                                    break;
                            }
                        }

                        index++;
                    }

                    //groupEnd = allFields[index - 1];
                    //return groupDrawer;
                }
            }

            drawer = null;
            groupEnd = null;
            return false;
        } else {
            int index = 0;
            var allFields = CachingUtilities.GetAllCachedFields(so, false);

            BaseAttributeDrawer previousDrawer = null;
            for (int groupAttr = begin.BeginGroups.Count - 1; groupAttr >= 0; groupAttr--) {
                bool tryCreate = AttributeDrawerCollector.TryCreateDrawerInstance(begin.BeginGroups[groupAttr].GetType(), begin.BeginGroups[groupAttr], begin, out var drawerInstance);

                if (tryCreate) {
                    if (drawerInstance is BaseGroupAttributeDrawer groupDrawer) {
                        groupNestingLevelAssign(groupDrawer, groupAttr);

                        if (previousDrawer != null) {
                            groupDrawer.AddChild(previousDrawer);
                        }

                        previousDrawer = groupDrawer;
                        groupDrawer.EditorInitialize();

                        if (TryHandlePropertyDrawer(allFields[index], out var beginFieldDrawer)) {
                            previousDrawer.AddChild(beginFieldDrawer);
                        }

                        if (allFields[index].IsEndGroup) {
                            groupCounter--;
                            index++;
                            continue;
                        }

                        index++;

                        bool isBreakOut = false;
                        while (!isBreakOut && index < allFields.Count) {
                            if (TryHandleGroupDrawer(so, allFields[index], out var nestedEnd, out var nestedDrawer)) {
                                previousDrawer.AddChild(nestedDrawer);
                            }

                            if (allFields[index].IsEndGroup) {
                                groupCounter--;
                                isBreakOut = true;
                                index++;

                                continue;
                            }

                            index++;
                        }
                    }
                }
            }

            drawer = previousDrawer;

            groupEnd = allFields[index - 1];
            return true;
        }
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
}