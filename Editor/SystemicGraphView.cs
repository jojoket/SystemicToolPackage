using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Search;

public class SystemicGraphView : GraphView
{
    private SystemicToolWindow m_toolWindow;
    public SystemicToolWindow ToolWindow => m_toolWindow;

    public List<SystemicEditorNode> m_graphNodes;
    public Dictionary<string, SystemicEditorNode> m_nodeDictionnary;

    public SystemicWindowSearchProvider m_searchProvider;

    public SystemicGraphView(SystemicToolWindow window)
    {
        m_toolWindow = window;

        m_graphNodes = new List<SystemicEditorNode>();
        m_nodeDictionnary = new Dictionary<string, SystemicEditorNode>();

        m_searchProvider = ScriptableObject.CreateInstance<SystemicWindowSearchProvider>();
        m_searchProvider.graph = this;
        this.nodeCreationRequest = ShowSearchWindow;

        StyleSheet style = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.jojoket.systemictool/Editor/USS/SystemicGraphView.uss");
        styleSheets.Add(style);

        GridBackground background = new GridBackground();
        background.name = "Grid";
        Add(background);
        background.SendToBack();

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new ClickSelector());

        DrawNodes();
    }

    private void ShowSearchWindow(NodeCreationContext context)
    {
        m_searchProvider.target = (VisualElement)focusController.focusedElement;
        SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), m_searchProvider);
    }

    public void Add(SystemicNode node)
    {
        m_toolWindow.CurrentViewData.nodes.Add(node);
        AddNodeToGraph(node);
    }

    private void AddNodeToGraph(SystemicNode node)
    {
        node.TypeName = node.GetType().AssemblyQualifiedName;
        SystemicEditorNode editorNode = new SystemicEditorNode(node);
        editorNode.SetPosition(node.rect);
        m_graphNodes.Add(editorNode);
        m_nodeDictionnary.Add(node.id, editorNode);
        node.EditorNode = editorNode;

        AddElement(editorNode);
    }

    private void DrawNodes()
    {
        foreach (SystemicNode node in m_toolWindow.CurrentViewData.nodes)
        {
            if (node.isViewed)
                AddNodeToGraph(node);
        }

        
    }

    public void ManageOverlapping()
    {
        foreach (SystemicEditorNode editorNode in m_graphNodes)
            editorNode.MoveOverlapping();
    }

    public void RefreshNodes()
    {
        foreach (SystemicEditorNode node in m_graphNodes)
        {
            node.RemoveFromHierarchy();
        }
        m_graphNodes.Clear();
        m_nodeDictionnary.Clear();
        DrawNodes();
    }

}
