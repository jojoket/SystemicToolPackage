using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class)]
public class SystemicTagAttribute : Attribute
{
    public string TagName { get; private set; } = null;

    public SystemicTagAttribute(string tagName)
    {
        TagName = tagName;
    }
}
