using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityToolbarExtender;

public class SystemicToolWindow : EditorWindow, IHasCustomMenu
{
    private SystemicGraphView m_currentView;
    private VisualElement m_currentTopLeftWindow;
    private Preview m_currentPreview;
    public VisualTreeAsset TopLeftWindowXML;
    public Texture ButtonImage;
    public SystemicViewData CurrentViewData;
    public GameObject currentFocusedObject;
    public static SystemicToolWindow window = null;



    [InitializeOnLoadMethod]
    private static void OnEditorLoad()
    {
        ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        window = null;
    }

    static SystemicToolWindow()
    {

    }

    private void EditorUpdate()
    {
        Repaint();
        m_currentView.ManageOverlapping();
    }

    #region Buttons and toolbars

    static void OnToolbarGUI()
    {
        SystemicViewData FirstSystemicViewData;
        string[] viewDatas = AssetDatabase.FindAssets("t:SystemicViewData");
        if (viewDatas.Length <= 0)
            return;

        GUILayout.FlexibleSpace();

        FirstSystemicViewData = (SystemicViewData)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(viewDatas[0]), typeof(SystemicViewData));

        if (GUILayout.Button(new GUIContent(EditorGUIUtility.GetIconForObject(FirstSystemicViewData), "Open Systemic View")))
        {
            if (window)
            {
                window.Load(window.CurrentViewData, false, true);
                window.Focus();
            }
            else
                OpenView(FirstSystemicViewData);
        }
    }

    [MenuItem("Window/Systemic Editor")]
    public static void ShowWindow()
    {
        GetWindow<SystemicToolWindow>("Systemic Editor");
    }

    void IHasCustomMenu.AddItemsToMenu(UnityEditor.GenericMenu menu)
    {
        //To add button to menu topbar right click
        /*GUIContent add = new GUIContent("Add Node");
        GUIContent clear = new GUIContent("Clear Nodes");
        menu.AddItem(add, false, AddNode);
        menu.AddItem(clear, false, ClearNodes);*/

    }

    #endregion

    #region Opening / Closing

    public static void ReOpenView(SystemicViewData target)
    {
        CloseWindow();
        OpenView(target);
    }

    public static void OpenView(SystemicViewData target)
    {
        SystemicToolWindow[] windows = Resources.FindObjectsOfTypeAll<SystemicToolWindow>();

        //Display given target viewData
        foreach (SystemicToolWindow systWindow in windows)
        {
            if (systWindow.CurrentViewData == target)
            {
                systWindow.Focus();
                return;
            }
        }

        window = CreateWindow<SystemicToolWindow>(typeof(SystemicToolWindow), typeof(SceneView));
        window.titleContent = new GUIContent($"{target.name}", EditorGUIUtility.ObjectContent(null, typeof(SystemicViewData)).image);
        window.Load(target, true, true);
    }

    public void Load(SystemicViewData target, bool withNewGraphView, bool withPropWind)
    {
        CurrentViewData = target;
        RetrieveComponentsToNodes();
        CheckIntegrityOfNodes();
        if (withNewGraphView)
            DrawGraph();
        else
            m_currentView.RefreshNodes();

        if (withPropWind)
            MakePropWindows();
        EditorApplication.update += EditorUpdate;
    }

    public static void CloseWindow()
    {
        Debug.Log("closed");
        window = EditorWindow.GetWindow<SystemicToolWindow>();
        window.Close();
        DestroyImmediate(window);
        window = null;
    }

    #endregion

    #region previewCamera


    public void SetPreviewFocus(GameObject focused)
    {
        m_currentPreview.ActivateSelf(true);
        currentFocusedObject = focused;
        m_currentPreview.FocusedObject = focused;
        if (focused == null)
        {
            m_currentPreview.ActivateSelf(false);
        }
    }
    #endregion


    #region PropWindows

    public void MakePropWindows()
    {
        MakeTopLeftWindow();
        MakePreviewWindow();
    }

    public void UpdateTopLeftWindow()
    {
        foreach (var element in m_currentTopLeftWindow.hierarchy.Children())
        {
            foreach (var child in element.Children())
            {
                if (child.GetType() == typeof(Label))
                {
                    Label label = (Label)child;
                    label.text = CurrentViewData.name;
                }
                if (child.GetType() == typeof(MaskField))
                {
                    MaskField maskField = (MaskField)child;

                    List<string> choices = RetrieveAllTags();
                    maskField.choices = choices;
                    SystemicViewData viewData = CurrentViewData;
                    maskField.value = viewData.TagMask;
                    maskField.UnregisterValueChangedCallback(ChangeMaskFieldCallBack);
                    maskField.RegisterValueChangedCallback(ChangeMaskFieldCallBack);
                }
            }
        }
    }

    private void MakeTopLeftWindow()
    {
        if (m_currentTopLeftWindow != null)
        {
            m_currentTopLeftWindow.RemoveFromHierarchy();
            m_currentTopLeftWindow = null;
        }
        m_currentTopLeftWindow = new VisualElement();

        m_currentTopLeftWindow.style.width = 250;

        TopLeftWindowXML.CloneTree(m_currentTopLeftWindow);
        foreach (var element in m_currentTopLeftWindow.hierarchy.Children())
        {
            foreach (var child in element.Children())
            {
                if (child.GetType() == typeof(Label))
                {
                    Label label = (Label)child;
                    label.text = CurrentViewData.name;
                }
                if (child.GetType() == typeof(ObjectField))
                {
                    ObjectField objectField = (ObjectField)child;
                    objectField.value = CurrentViewData;
                    objectField.RegisterValueChangedCallback(evt =>
                    {
                        window.Load((SystemicViewData)evt.newValue, false, false);
                    });
                }
                if (child.GetType() == typeof(MaskField))
                {
                    MaskField maskField = (MaskField)child;

                    List<string> choices = RetrieveAllTags();
                    maskField.choices = choices;
                    SystemicViewData viewData = CurrentViewData;
                    maskField.value = viewData.TagMask;
                    maskField.RegisterValueChangedCallback(ChangeMaskFieldCallBack);
                }
            }
        }

        m_currentTopLeftWindow.RegisterCallback<FocusOutEvent>(focusEvent =>
        {
            UpdateTopLeftWindow();
        });
        window.m_currentView.Add(m_currentTopLeftWindow);
    }

    private void ChangeMaskFieldCallBack(ChangeEvent<int> evt)
    {
        SystemicViewData viewData = CurrentViewData;
        viewData.TagMask = evt.newValue;
        window.Load(CurrentViewData, false, false);
    }

    private void MakePreviewWindow()
    {
        if (m_currentPreview != null)
        {
            m_currentPreview.Clean();
            m_currentPreview.RemoveFromHierarchy();
            m_currentPreview = null;
        }
        m_currentPreview = new Preview();
        m_currentPreview.style.alignSelf = Align.FlexEnd;

        window.m_currentView.Add(m_currentPreview);
    }

    #endregion

    private void DrawGraph()
    {
        m_currentView = new SystemicGraphView(this);
        rootVisualElement.Add(m_currentView);
    }

    private void RetrieveComponentsToNodes()
    {
        Transform[] transforms = GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None);

        foreach (Transform transform in transforms)
        {
            if (CurrentViewData.nodes.Find(x => x.title == transform.name) != null)
                continue;
            SystemicNode node = new SystemicNode(transform.name);
            node.LinkedGameObject = transform.gameObject;
            node.isViewed = true;
            if (!node.HasSystemicAttribute() || !IsNodeInMask(node.GetSystemicAttributes()))
            {
                node.isViewed = false;
            }
            CurrentViewData.nodes.Add(node);
        }
    }

    private void CheckIntegrityOfNodes()
    {
        for (int i = 0; i < CurrentViewData.nodes.Count; i++)
        {
            SystemicNode node = CurrentViewData.nodes[i];

            node.isViewed = true;
            if (!node.HasSystemicAttribute() || !IsNodeInMask(node.GetSystemicAttributes()))
            {
                node.isViewed = false;
            }
        }
    }

    private void OnEnable()
    {
        if (CurrentViewData != null)
        {
            ReOpenView(CurrentViewData);
        }
    }


    private bool IsNodeInMask(List<string> nodeTags)
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

        bool isInMask = false;
        foreach (string nodeTag in nodeTags)
        {
            int index = tags.FindIndex(x => x == nodeTag);
            if (index != -1)
            {
                if (IsMaskFieldSet(CurrentViewData.TagMask, index))
                {
                    isInMask = true;
                }
            }
        }

        return isInMask;
    }

    private bool IsMaskFieldSet(int mask, int fieldPosition)
    {
        return (mask & (1 << fieldPosition)) != 0;
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