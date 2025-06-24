using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(SystemicViewData))]
public class SystemicViewDataEditor : Editor
{
    public VisualTreeAsset m_InspectorXML;
    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceId, int index)
    {
        Object asset = EditorUtility.InstanceIDToObject(instanceId);
        if (asset.GetType() == typeof(SystemicViewData))
        {
            if (SystemicToolWindow.window == null)
                SystemicToolWindow.OpenView((SystemicViewData)asset);
            else
                SystemicToolWindow.window.Load((SystemicViewData)asset, false, true);
            return true;
        }

        return false;
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement myInspector = new VisualElement();
        m_InspectorXML.CloneTree(myInspector);

        foreach (var child in myInspector.hierarchy.Children())
        {
            if (child.GetType() == typeof(MaskField))
            {
                MaskField maskField = (MaskField)child;

                List<string> choices = RetrieveAllTags();
                maskField.choices = choices;
                SystemicViewData viewData = (SystemicViewData)target;
                maskField.value = viewData.TagMask;
                maskField.RegisterValueChangedCallback(evt => {
                    viewData.TagMask = evt.newValue;
                    SystemicToolWindow.window.Load((SystemicViewData)target, false, true);

                });
            }
            if (child.GetType() == typeof(Button))
            {
                Button button = (Button)child;
                button.clicked += () =>
                {
                    if (SystemicToolWindow.window == null)
                        SystemicToolWindow.OpenView((SystemicViewData)target);
                    else
                        SystemicToolWindow.window.Load((SystemicViewData)target, false, true);
                };
            }
        }

        return myInspector;
    }


    private List<string> RetrieveAllTags()
    {
        List<string> tags = new List<string>();

        Transform[] transforms = GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None);

        foreach (Transform transform in transforms)
        {
            Component[] components = transform.GetComponents(typeof(Component));

            foreach (Component component in components)
            {
                object[] attributes = component.GetType().GetCustomAttributes(typeof(SystemicTagAttribute), false);
                if (attributes.Length <= 0)
                    continue;
                SystemicTagAttribute systemicTagAttribute = attributes[0] as SystemicTagAttribute;
                if (tags.Contains(systemicTagAttribute.TagName))
                    continue;
                tags.Add(systemicTagAttribute.TagName);
            }
        }

        return tags;
    }
}
