using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeComponent : VisualElement
{
    public Component ManagedComponent;
    private SerializedObject serializedComponent;
    public TextElement ComponentTitle;
    public Foldout ComponentFoldout;

    public List<PropertyField> PropertyFields = new List<PropertyField>();

    private List<GameObject> _LinkedNodes = new List<GameObject>();

    public NodeComponent(Component component, string title)
    {
        ManagedComponent = component;
        serializedComponent = new SerializedObject(ManagedComponent);
        style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        
        ComponentFoldout = new Foldout();
        ComponentFoldout.text = title;
        ComponentFoldout.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Add(ComponentFoldout);
        if (ManagedComponent is Transform)
        {
            SerializedProperty positionProperty = serializedComponent.FindProperty("m_LocalPosition");
            PropertyField positionField = new PropertyField(positionProperty, "Position");
            positionField.name = "Position";
            positionField.label = "Position";
            positionField.Bind(serializedComponent);
            positionField.SendToBack();
            ComponentFoldout.contentContainer.Add(positionField);

            // to update dynamically
            positionField.RegisterCallback<ChangeEvent<Vector3>>(evt =>
            {
                serializedComponent.ApplyModifiedProperties();
            });
        }
        int index = 0;
        foreach (FieldInfo field in ManagedComponent.GetType().GetFields())
        {
            SerializedProperty serializedProperty = serializedComponent.FindProperty(field.Name);
            PropertyField propertyField = new PropertyField(serializedProperty);
            propertyField.label = field.Name;
            propertyField.style.backgroundColor = new Color(0,0,0,0.3f);
            propertyField.Bind(serializedComponent);
            propertyField.SendToBack();
            ComponentFoldout.contentContainer.Add(propertyField);
            //Add(propertyField);

            if (!(field.FieldType.Name == "String") && !(field.FieldType.IsPrimitive))
            {
                /*//if we can convert the value of the field to a component not null, add it to linked nodes
                Debug.Log(field.GetValue(field));
                Component comp = field.GetValue(field) as Component;
                if (comp.gameObject)
                {
                    AddToLinkedNodes(index, comp.gameObject);
                }

                //setup callback on field change
                propertyField.RegisterValueChangeCallback(evt => {
                    Component comp = field.GetValue(field) as Component;
                    if (comp.gameObject)
                    {
                        ModifyLinkedNode(index, comp.gameObject);
                    }
                });

                index++;*/
            }
        }
    }

    private void AddToLinkedNodes(int index, GameObject gameObject)
    {
        if (_LinkedNodes.Count < index)
        {
            for (int i = 0; i < index - _LinkedNodes.Count; i++)
            {
                _LinkedNodes.Add(null);
            }
        }
        _LinkedNodes[index] = gameObject;
    }

    private void ModifyLinkedNode(int index, GameObject gameObject)
    {
        if (_LinkedNodes[index]!=null)
        {
            _LinkedNodes[index] = gameObject;
        }
        else
        {
            AddToLinkedNodes(index, gameObject);
        }
    }

    public List<GameObject> GetLinkedNodes()
    {
        return _LinkedNodes;
    }
}
