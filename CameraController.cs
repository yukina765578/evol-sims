using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;
    public float minZoom = 2f;
    public float maxZoom = 20f;
    
    [Header("Pan Settings")]
    public float panSpeed = 10f;
    
    private Camera cam;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (!cam.orthographic)
        {
            cam.orthographic = true;
        }
        cam.orthographicSize = 100f; // Set initial zoom level
    }
    
    void Update()
    {
        HandleZoom();
        HandlePanning();
        HandleReset();
    }
    
    void HandleZoom()
    {
        // Check if mouse exists
        if (Mouse.current == null) return;
        
        Vector2 scroll = Mouse.current.scroll.ReadValue();
        
        if (Mathf.Abs(scroll.y) > 0.1f) // Add threshold
        {
            Debug.Log("Scrolling: " + scroll.y); // Debug line
            
            float zoomDirection = scroll.y > 0 ? -1 : 1; // Invert if needed
            cam.orthographicSize += zoomDirection * zoomSpeed * Time.deltaTime * 10f;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }
    
    void HandlePanning()
    {
        if (Mouse.current == null) return;
        
        // Use middle mouse button instead (easier to test)
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            Debug.Log("Panning: " + mouseDelta); // Debug line
            
            Vector3 move = new Vector3(-mouseDelta.x, -mouseDelta.y, 0);
            move *= panSpeed * Time.deltaTime * cam.orthographicSize * 0.01f;
            
            transform.Translate(move);
        }
    }
    
    void HandleReset()
    {
        if (Keyboard.current == null) return;
        
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("Resetting camera"); // Debug line
            transform.position = new Vector3(0, 0, -10);
            cam.orthographicSize = 100f;
        }
    }
}
