using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Preview : VisualElement
{
    private RenderTexture m_renderTexture;
    private Camera m_camera;
    private Image m_currentImage;
    public GameObject cameraObject;
    public GameObject FocusedObject;
    private float m_angle = 0;

    private void GetTexture()
    {
        cameraObject = GameObject.Find("PreviewCamera");
        if (cameraObject == null)
        {
            cameraObject = new GameObject("PreviewCamera");
            m_camera = cameraObject.AddComponent<Camera>();
        }
        else
        {
            m_camera = cameraObject.GetComponent<Camera>();
        }

        if (!m_renderTexture)
        {
            // Create a RenderTexture
            m_renderTexture = new RenderTexture(256, 256, 16);
        }
        m_camera.targetTexture = m_renderTexture;

    }
    public void Clean()
    {
        // Clean up
        if (m_camera != null)
        {
            m_camera.targetTexture = null;
        }
        if (m_renderTexture != null)
        {
            m_renderTexture.Release();
        }
    }

    private void EditorUpdateFocus()
    {
        if (cameraObject && FocusedObject)
        {
            m_angle += Time.deltaTime % 360;
            Vector3 offSet = new Vector3(Mathf.Cos(m_angle), 1, Mathf.Sin(m_angle));
            cameraObject.transform.position = FocusedObject.transform.position + offSet * 5;
            cameraObject.transform.rotation = Quaternion.LookRotation(FocusedObject.transform.position - cameraObject.transform.position);
        }
    }



    public Preview()
    {
        GetTexture();
        if (m_renderTexture == null)
        {
            return;
        }

        m_currentImage = new Image();
        m_currentImage.image = m_renderTexture;
        Add(m_currentImage);
        ActivateSelf(true);
        EditorApplication.update += EditorUpdateFocus;
    }

    public void ActivateSelf(bool doActive)
    {
        m_currentImage.tintColor = doActive ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0);
        if (doActive)
        {
            GetTexture();
        }
    }
}
