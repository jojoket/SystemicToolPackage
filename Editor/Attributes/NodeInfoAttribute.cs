using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeInfoAttribute : Attribute
{
    private string m_nodeTitle;
    private string m_menuItem;
    public string NodeTitle => m_nodeTitle;
    public string MenuItem => m_menuItem;

    public NodeInfoAttribute(string nodeTitle, string menuItem = "")
    {
        m_nodeTitle = nodeTitle;
        m_menuItem = menuItem;
    }
}
