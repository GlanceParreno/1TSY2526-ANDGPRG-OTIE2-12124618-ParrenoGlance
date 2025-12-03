using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
    [Header("Panning")]
    public float panSpeed = 20f;
    public float edgeScrollSize = 10f; 
    public bool enableEdgeScroll = true;

    [Header("Rotation")]
    public float rotateSpeed = 120f;
    public bool enableRightClickRotate = true;

    [Header("Zoom")]
    public float zoomSpeed = 200f;
    public float minHeight = 10f;
    public float maxHeight = 60f;

    Camera cam;
    Vector3 lastMousePos;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Vector3 move = Vector3.zero;

        
        float h = Input.GetAxis("Horizontal"); 
        float v = Input.GetAxis("Vertical");   
        if (Mathf.Abs(h) > 0.01f) move += transform.right * h;
        if (Mathf.Abs(v) > 0.01f) move += transform.forward * v;

        
        if (enableEdgeScroll)
        {
            Vector3 m = Input.mousePosition;
            if (m.x >= 0 && m.x < edgeScrollSize) move -= transform.right;
            if (m.x <= Screen.width && m.x > Screen.width - edgeScrollSize) move += transform.right;
            if (m.y >= 0 && m.y < edgeScrollSize) move -= transform.forward;
            if (m.y <= Screen.height && m.y > Screen.height - edgeScrollSize) move += transform.forward;
        }

        
        if (move.sqrMagnitude > 0.001f)
        {
            transform.position += move.normalized * panSpeed * Time.deltaTime;
        }

        
        if (enableRightClickRotate && Input.GetMouseButtonDown(1))
        {
            lastMousePos = Input.mousePosition;
        }
        if (enableRightClickRotate && Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            float yaw = delta.x * rotateSpeed * Time.deltaTime * 0.1f;
            transform.Rotate(Vector3.up, yaw, Space.World);
            lastMousePos = Input.mousePosition;
        }

        
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            Vector3 pos = transform.position;
            pos += transform.forward * scroll * zoomSpeed * Time.deltaTime;
            pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);
            transform.position = pos;
        }
    }
}
