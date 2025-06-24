using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SystemicView",menuName = "SystemicTool/Systemic View")]
public class SystemicViewData : ScriptableObject
{
    public List<SystemicNode> nodes = new List<SystemicNode>();
    public int TagMask;
}
