using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class SystemicEditorNode : Node
{
    private SystemicNode m_systemicNode;


    private Vector3Field PositionField;

    private List<NodeComponent> m_nodeComponents = new List<NodeComponent>();

    private List<SystemicEditorNode> _LinkedNodes = new List<SystemicEditorNode>();
    private Dictionary<int,VisualElement> _Links = new Dictionary<int, VisualElement>();

    public SystemicEditorNode(SystemicNode node)
    {
        this.AddToClassList("systemic-node");

        m_systemicNode = node;

        Type typeInfo = node.GetType();
        NodeInfoAttribute info = typeInfo.GetCustomAttribute<NodeInfoAttribute>();

        title = info.NodeTitle;

        //replace " " by "-"
        string[] depths = info.MenuItem.Split('/');
        foreach (string depth in depths)
        {
            this.AddToClassList(depth.ToLower().Replace(' ', '-'));
        }

        this.name = typeInfo.Name;
        if (node.title != "")
        {
            title = node.title;
        }

        Button selectButton = new Button();
        selectButton.text = "Select";
        selectButton.clicked += OnSelect;
        titleButtonContainer.Add(selectButton);
        Button focusButton = new Button();
        focusButton.text = "Focus";
        focusButton.clicked += OnFocus;
        titleButtonContainer.Add(focusButton);


        //Components
        Component[] components = node.LinkedGameObject.GetComponents(typeof(Component));

        foreach (Component component in components)
        {
            if (component.GetType().GetCustomAttributes(typeof(SystemicTagAttribute), false).Length <= 0)
            {
                continue;
            }
            NodeComponent nodeComponent = new NodeComponent(component, component.GetType().Name);
            m_nodeComponents.Add(nodeComponent);
            extensionContainer.Add(nodeComponent);
        }

        expanded = false;
        expanded = true;
    }

    public void MoveOverlapping()
    {
        /*SystemicToolWindow[] toolWindows = Resources.FindObjectsOfTypeAll<SystemicToolWindow>();
        

        foreach (SystemicNode systNode in toolWindows[0].CurrentViewData.nodes)
        {
            if (systNode.EditorNode == null)
            {
                continue;
            }
            Rect rect1 = worldBound;
            Rect rect2 = systNode.EditorNode.worldBound;

            bool isOverlapping = rect1.Overlaps(rect2);

            if (isOverlapping)
            {
                float moveLength = rect1.width - (rect2.x - rect1.x);
                systNode.rect = new Rect(rect2.position + new Vector2(moveLength, 0), systNode.rect.size);
            }
        }*/

    }

    #region links

    public void UpdateLinked()
    {
        List<GameObject> linkedGameObjects = new List<GameObject>();
        foreach (NodeComponent nodeComponent in m_nodeComponents)
        {
            foreach (GameObject gameobj in nodeComponent.GetLinkedNodes())
            {
                linkedGameObjects.Add(gameobj);
            }
        }

        foreach (SystemicEditorNode linkedNode in _LinkedNodes)
        {
            
        }
    }

    public void UpdateLinks()
    {
        for (int i = 0; i < _LinkedNodes.Count; i++)
        {
            _Links[i].RemoveFromHierarchy();
            _Links.Remove(i);
            LinkNodes(i, this, _LinkedNodes[i]);
        }
    }

    public void LinkNodes(int linkedNodeIndex, Node startNode, Node endNode)
    {
        var startPoint = startNode.GetPosition().position + new Vector2(startNode.worldBound.width / 2, startNode.worldBound.height / 2);
        var endPoint = endNode.GetPosition().position + new Vector2(endNode.worldBound.width / 2, endNode.worldBound.height / 2);

        var edge = new VisualElement();
        edge.style.position = Position.Absolute;
        edge.style.top = startPoint.y;
        edge.style.left = startPoint.x;
        edge.style.width = Vector2.Distance(startPoint, endPoint);
        edge.style.height = 2;
        edge.style.backgroundColor = Color.white;

        this.Add(edge);
        _Links.Add(linkedNodeIndex, edge);

    }

    #endregion

    private void OnSelect()
    {
        Selection.SetActiveObjectWithContext(m_systemicNode.LinkedGameObject, m_systemicNode.LinkedGameObject);
    }

    private void OnFocus()
    {
        Selection.SetActiveObjectWithContext(m_systemicNode.LinkedGameObject, m_systemicNode.LinkedGameObject);
        EditorCoroutineUtility.StartCoroutineOwnerless(FocusAfter(0.01f));
    }

    private IEnumerator FocusAfter(float sec)
    {
        yield return new EditorWaitForSeconds(sec);
        SceneView.lastActiveSceneView.FrameSelected();
    }

    public override void OnUnselected()
    {
        base.OnUnselected();
        m_systemicNode.rect = GetPosition();
        SystemicToolWindow.window.SetPreviewFocus(null);

    }

    public override void OnSelected()
    {
        base.OnSelected();
        SystemicToolWindow.window.SetPreviewFocus(m_systemicNode.LinkedGameObject);
    }
}
