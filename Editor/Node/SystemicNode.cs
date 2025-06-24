using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[System.Serializable, NodeInfo("Node", "Process/Node")]
public class SystemicNode
{
    public SystemicEditorNode EditorNode;
    public Rect rect;
    public string title;
    public string TypeName;
    public GameObject LinkedGameObject;
    [SerializeField]
    private string m_guid;
    public string id => m_guid;
    public bool isDragged;
    public bool isSelected;
    public bool isViewed;

    //Node's property
    public List<float> floats = new List<float>();

    public SystemicNode()
    {
        rect = new Rect(0, 0, 100, 100);
        this.title = "";
        floats.Add(0);
        m_guid = Guid.NewGuid().ToString();
    }

    public SystemicNode(string title)
    {
        rect = new Rect(0, 0, 100, 100);
        this.title = title;
        floats.Add(0);
        m_guid = Guid.NewGuid().ToString();
    }

    public SystemicNode(Vector2 position, float width, float height, string title)
    {
        rect = new Rect(position.x, position.y, width, height);
        this.title = title;
        floats.Add(0);
        m_guid = Guid.NewGuid().ToString();
    }

    public void Draw()
    {
        GUI.Box(rect, title);
        
        foreach (var item in floats)
        {
            Rect pos = new Rect(rect);
            pos.size = new Vector2(50,20);
            pos.position += new Vector2(30,20);
            GUI.TextField(pos, item.ToString());
        }
    }

    public bool HasSystemicAttribute()
    {
        bool hasSystemic = false;
        if (!LinkedGameObject)
        {
            return false;
        }
        Component[] components = LinkedGameObject.GetComponents(typeof(Component));

        foreach (Component component in components)
        {
            if (component.GetType().GetCustomAttributes(typeof(SystemicTagAttribute), false).Length > 0)
            {
                hasSystemic = true;
            }
        }
        return hasSystemic;
    }

    public List<string> GetSystemicAttributes()
    {
        List<string> attributes = new List<string>();

        Component[] components = LinkedGameObject.GetComponents(typeof(Component));

        foreach (Component component in components)
        {
            if (component.GetType().GetCustomAttributes(typeof(SystemicTagAttribute), false).Length > 0)
            {
                foreach (SystemicTagAttribute attribute in component.GetType().GetCustomAttributes(typeof(SystemicTagAttribute), false))
                {
                    attributes.Add(attribute.TagName);
                }
            }
        }
        return attributes;
    }

    public void SetPosition(Vector2 position)
    {
        rect.position = position;
    }
}
