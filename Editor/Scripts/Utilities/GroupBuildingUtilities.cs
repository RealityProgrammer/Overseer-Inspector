using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GroupBuildingUtilities
{
    private struct GroupElementStack {
        public OverseerBeginGroupAttribute Attribute { get; private set; }
        public BaseGroupAttributeDrawer GroupDrawer { get; private set; }

        public GroupElementStack(OverseerBeginGroupAttribute attr, BaseGroupAttributeDrawer drawer) {
            Attribute = attr;
            GroupDrawer = drawer;
        }
    }

    internal static void BeginStackGroupTest(List<SerializedFieldContainer> allFields, SerializedFieldContainer begin, ref int fieldPointer, out BaseAttributeDrawer outputDrawer) {
        _qegt.Clear();
        _stackGroupTest.Clear();

        Debug.Log("BeginStackGroupTest. Reset everything");

        // OverseerEditorUtilities.SearchAndIncrementIndex(allFields, begin, ref fieldPointer);
        QueueGroupTest(allFields, begin, ref fieldPointer, out outputDrawer);
    }

    static Dictionary<SerializedFieldContainer, int> _qegt = new Dictionary<SerializedFieldContainer, int>();
    static Stack<GroupElementStack> _stackGroupTest = new Stack<GroupElementStack>();
    private static void QueueGroupTest(List<SerializedFieldContainer> allFields, SerializedFieldContainer begin, ref int index, out BaseAttributeDrawer outputDrawer) {
        outputDrawer = null;

        for (int i = 0; i < begin.BeginGroups.Count; i++) {
            if (AttributeDrawerCollector.TryCreateDrawerInstance(begin.BeginGroups[i].GetType(), begin.BeginGroups[i], begin, out var drawerInstance)) {
                if (drawerInstance is BaseGroupAttributeDrawer _drawerInstance) {
                    _stackGroupTest.Push(new GroupElementStack(begin.BeginGroups[i], _drawerInstance));
                    OverseerEditorUtilities.RPDevelopmentDebug("Push group: " + _drawerInstance.GroupName + ". Current field index: " + index + ". Stack count: " + _stackGroupTest.Count);
                }
            }
        }

        while (_stackGroupTest.Count != 0 && index < allFields.Count) {
            var currField = allFields[index];
            bool continueFlag = false;
            //bool breakFlag = false;

            bool recursionCond = !ReferenceEquals(currField, begin) && currField.BeginGroups != null && currField.BeginGroups.Count >= 1;
            if (recursionCond) {
                QueueGroupTest(allFields, currField, ref index, out outputDrawer);
                continue;
            }

            if (currField.IsEndGroup) {
                if (!_qegt.ContainsKey(currField)) {
                    _qegt[currField] = currField.EndGroupCount;
                }

                while (_qegt[currField] > 0) {
                    var pop = _stackGroupTest.Pop();

                    if (_stackGroupTest.Count >= 1) {
                        _stackGroupTest.Peek().GroupDrawer.AddChild(pop.GroupDrawer);
                    } else {
                        outputDrawer = pop.GroupDrawer;
                    }

                    // Debug.Log("Pop " + pop.Attribute.Name + ". Remain: " + _stackGroupTest.Count);

                    if (_qegt[currField] == currField.EndGroupCount) {
                        if (OverseerEditorUtilities.TryHandlePropertyDrawer(currField, out var child)) {
                            pop.GroupDrawer.AddChild(child);
                        }

                        OverseerEditorUtilities.RPDevelopmentDebug("Handle end group field " + currField.UnderlyingField.Name + ", Pop: " + pop.Attribute.Name + ", Remain: " + _stackGroupTest.Count + ", Current end count: " + (_qegt[currField] - 1) + ", Current field index: " + index);
                    } else {
                        OverseerEditorUtilities.RPDevelopmentDebug("End group field handled " + currField.UnderlyingField.Name + ", Pop: " + pop.Attribute.Name + ", Remain: " + _stackGroupTest.Count + ", Current end count: " + (_qegt[currField] - 1) + ", Current field index: " + index);
                    }

                    _qegt[currField]--;

                    if (_qegt[currField] == 0) {
                        continueFlag = true;
                        break;
                    }
                }
            } else {
                if (OverseerEditorUtilities.TryHandlePropertyDrawer(currField, out var child)) {
                    _stackGroupTest.Peek().GroupDrawer.AddChild(child);
                }

                OverseerEditorUtilities.RPDevelopmentDebug("Handle field " + currField.UnderlyingField.Name + " to group " + _stackGroupTest.Peek().Attribute.Name + ". Current field index: " + index);
            }

            if (index >= allFields.Count) {
                break;
            }

            //if (breakFlag)
            //    break;

            if (continueFlag)
                continue;

            //currField = allFields[index];
            //bool recursionCond = !ReferenceEquals(currField, begin) && currField.BeginGroups != null && currField.BeginGroups.Count >= 1;
            //if (recursionCond) {
            //    QueueGroupTest(allFields, currField, ref index, out outputDrawer);
            //    continue;
            //} else {
            //    OverseerEditorUtilities.RPDevelopmentDebug("Handle field " + currField.UnderlyingField.Name + " to group " + _stackGroupTest.Peek().Attribute.Name + ". Current field index: " + index);
            //}

            index++;
        }
    }

    #region Backups
    /*
    static Dictionary<SerializedFieldContainer, int> _qegt = new Dictionary<SerializedFieldContainer, int>();
    static Stack<GroupElementStack> _stackGroupTest = new Stack<GroupElementStack>();
    private static void QueueGroupTest(List<SerializedFieldContainer> allFields, SerializedFieldContainer begin, ref int index, out BaseAttributeDrawer outputDrawer) {
        outputDrawer = null;

        for (int i = 0; i < begin.BeginGroups.Count; i++) {
            if (AttributeDrawerCollector.TryCreateDrawerInstance(begin.BeginGroups[0].GetType(), begin.BeginGroups[0], begin, out var drawerInstance)) {
                if (drawerInstance is BaseGroupAttributeDrawer _drawerInstance) {
                    _stackGroupTest.Push(new GroupElementStack(begin.BeginGroups[i], _drawerInstance));
                }
            }

            OverseerEditorUtilities.RPDevelopmentDebug("Push group: " + begin.BeginGroups[i].Name + ". Current field index: " + index);
        }

        while (_stackGroupTest.Count != 0 && index < allFields.Count) {
            var currField = allFields[index];
            bool continueFlag = false;
            bool breakFlag = false;

            if (currField.IsEndGroup) {
                if (!_qegt.ContainsKey(currField)) {
                    _qegt[currField] = currField.EndGroupCount;
                }

                while (_qegt[currField] > 0) {
                    var pop = _stackGroupTest.Pop();

                    if (_qegt[currField] == currField.EndGroupCount) {
                        //if (AttributeDrawerCollector.TryCreateDrawerInstance()
                        OverseerEditorUtilities.RPDevelopmentDebug("Handle end group field " + currField.UnderlyingField.Name + ", Pop: " + pop.Attribute.Name + ". Current end count: " + (_qegt[currField] - 1) + ". Current field index: " + index);
                    } else {
                        OverseerEditorUtilities.RPDevelopmentDebug("End group field handled " + currField.UnderlyingField.Name + ", Pop: " + pop.Attribute.Name + ". Current end count: " + (_qegt[currField] - 1) + ". Current field index: " + index);
                    }

                    _qegt[currField]--;

                    if (_qegt[currField] == 0) {
                        if (++index >= allFields.Count) {
                            index--;
                            breakFlag = true;
                            break;
                        }

                        continueFlag = true;

                        break;
                    }
                }
            }

            if (breakFlag)
                break;

            if (continueFlag)
                continue;

            currField = allFields[index];
            bool recursionCond = !ReferenceEquals(currField, begin) && currField.BeginGroups != null && currField.BeginGroups.Count >= 1;
            if (recursionCond) {
                QueueGroupTest(allFields, currField, ref index, out outputDrawer);
                continue;
            } else {
                OverseerEditorUtilities.RPDevelopmentDebug("Handle field " + currField.UnderlyingField.Name + " to group " + _stackGroupTest.Peek().Attribute.Name + ". Current field index: " + index);
            }

            index++;
        }
    }

    static Dictionary<SerializedFieldContainer, int> _qegt = new Dictionary<SerializedFieldContainer, int>();
    static Stack<OverseerBeginGroupAttribute> _stackGroupTest = new Stack<OverseerBeginGroupAttribute>();
    private static void QueueGroupTest(List<SerializedFieldContainer> allFields, SerializedFieldContainer begin, ref int index) {
        for (int i = 0; i < begin.BeginGroups.Count; i++) {
            _stackGroupTest.Push(begin.BeginGroups[i]);
            OverseerEditorUtilities.RPDevelopmentDebug("Push group: " + begin.BeginGroups[i].Name + ". Current field index: " + index);
        }

        while (_stackGroupTest.Count != 0 && index < allFields.Count) {
            var currField = allFields[index];
            bool continueFlag = false;
            bool breakFlag = false;

            if (currField.IsEndGroup) {
                if (!_qegt.ContainsKey(currField)) {
                    _qegt[currField] = currField.EndGroupCount;
                }

                while (_qegt[currField] > 0) {
                    if (_qegt[currField] == currField.EndGroupCount) {
                        OverseerEditorUtilities.RPDevelopmentDebug("Handle end group field " + currField.UnderlyingField.Name + ", Pop: " + _stackGroupTest.Pop().Name + ". Current end count: " + (_qegt[currField] - 1) + ". Current field index: " + index);
                    } else {
                        OverseerEditorUtilities.RPDevelopmentDebug("End group field handled " + currField.UnderlyingField.Name + ", Pop: " + _stackGroupTest.Pop().Name + ". Current end count: " + (_qegt[currField] - 1) + ". Current field index: " + index);
                    }

                    _qegt[currField]--;

                    if (_qegt[currField] == 0) {
                        if (++index >= allFields.Count) {
                            index--;
                            breakFlag = true;
                            break;
                        }

                        continueFlag = true;

                        break;
                    }
                }
            }

            if (breakFlag)
                break;

            if (continueFlag)
                continue;

            currField = allFields[index];
            bool recursionCond = !ReferenceEquals(currField, begin) && currField.BeginGroups != null && currField.BeginGroups.Count >= 1;
            if (recursionCond) {
                QueueGroupTest(allFields, currField, ref index);
                continue;
            } else {
                OverseerEditorUtilities.RPDevelopmentDebug("Handle field " + currField.UnderlyingField.Name + " to group " + _stackGroupTest.Peek().Name + ". Current field index: " + index);
            }

            index++;
        }
    }
    */
    #endregion
}
