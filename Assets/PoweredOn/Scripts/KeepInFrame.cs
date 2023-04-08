using UnityEngine;

/* require a BoxCollider component on the game object */
[RequireComponent(typeof(BoxCollider))]
public class KeepInFrame : MonoBehaviour
{
    public float speed = 1.0f;
    public bool forceFront = true;
    private BoxCollider boundingBox;

    private void Awake()
    {
        boundingBox = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get a fresh reference to the main scene camera every update cycle
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found. Make sure there's a camera with the 'MainCamera' tag in your scene.");
            return;
        }

        // Draw debug gizmo of the camera frustum
        Debug.DrawLine(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)), mainCamera.ViewportToWorldPoint(new Vector3(1, 0, mainCamera.nearClipPlane)), Color.red);
        Debug.DrawLine(mainCamera.ViewportToWorldPoint(new Vector3(0, 1, mainCamera.nearClipPlane)), mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)), Color.red);
        Debug.DrawLine(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)), mainCamera.ViewportToWorldPoint(new Vector3(0, 1, mainCamera.nearClipPlane)), Color.red);
        Debug.DrawLine(mainCamera.ViewportToWorldPoint(new Vector3(1, 0, mainCamera.nearClipPlane)), mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)), Color.red);

        // Draw lines connecting the camera to the frustum
        Debug.DrawLine(mainCamera.transform.position, mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)), Color.red);
        Debug.DrawLine(mainCamera.transform.position, mainCamera.ViewportToWorldPoint(new Vector3(1, 0, mainCamera.nearClipPlane)), Color.red);
        Debug.DrawLine(mainCamera.transform.position, mainCamera.ViewportToWorldPoint(new Vector3(0, 1, mainCamera.nearClipPlane)), Color.red);
        Debug.DrawLine(mainCamera.transform.position, mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)), Color.red);
        

        // Check if the game object is in front of the camera
        Vector3 toObject = transform.position - mainCamera.transform.position;
        bool inFront = Vector3.Dot(mainCamera.transform.forward, toObject) > 0;

        // Lerp the game object towards the camera's forward position
        if (forceFront && !inFront)
        {
            Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.forward * toObject.magnitude;
            transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
        }

        // Check if the bounding box exceeds the view frustum
        bool exceedsFrustum = false;
        Vector3[] vertices = GetBoundingBoxVertices();

        foreach (Vector3 vertex in vertices)
        {
            Vector3 viewportPosition = mainCamera.WorldToViewportPoint(vertex);
            if (viewportPosition.x < 0 || viewportPosition.x > 1 || viewportPosition.y < 0 || viewportPosition.y > 1 || viewportPosition.z < 0)
            {
                exceedsFrustum = true;
                break;
            }
        }

        // Push the game object back in z-space if the bounding box exceeds the view frustum
        Vector3 pushDirection = mainCamera.transform.forward;
        if (exceedsFrustum)
        {
            transform.position += pushDirection * speed * Time.deltaTime;
        }
        else
        {
            // push the towards the camera so it fills as much of the view as possible
            transform.position -= pushDirection * speed * Time.deltaTime;
        }
    }

    // Get the vertices of the bounding box
    private Vector3[] GetBoundingBoxVertices()
    {
        Vector3[] vertices = new Vector3[8];
        /* get the vertex positions of the bounding box */
        vertices[0] = boundingBox.transform.TransformPoint(boundingBox.center + new Vector3(boundingBox.size.x, boundingBox.size.y, boundingBox.size.z) * 0.5f);
        vertices[1] = boundingBox.transform.TransformPoint(boundingBox.center + new Vector3(boundingBox.size.x, boundingBox.size.y, -boundingBox.size.z) * 0.5f);
        vertices[2] = boundingBox.transform.TransformPoint(boundingBox.center + new Vector3(boundingBox.size.x, -boundingBox.size.y, boundingBox.size.z) * 0.5f);
        vertices[3] = boundingBox.transform.TransformPoint(boundingBox.center + new Vector3(boundingBox.size.x, -boundingBox.size.y, -boundingBox.size.z) * 0.5f);
        vertices[4] = boundingBox.transform.TransformPoint(boundingBox.center + new Vector3(-boundingBox.size.x, boundingBox.size.y, boundingBox.size.z) * 0.5f);
        vertices[5] = boundingBox.transform.TransformPoint(boundingBox.center + new Vector3(-boundingBox.size.x, boundingBox.size.y, -boundingBox.size.z) * 0.5f);
        vertices[6] = boundingBox.transform.TransformPoint(boundingBox.center + new Vector3(-boundingBox.size.x, -boundingBox.size.y, boundingBox.size.z) * 0.5f);
        vertices[7] = boundingBox.transform.TransformPoint(boundingBox.center + new Vector3(-boundingBox.size.x, -boundingBox.size.y, -boundingBox.size.z) * 0.5f);

        return vertices;
    }

    // Draw bounding box gizmos in the scene view
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3[] vertices = GetBoundingBoxVertices();

        // Draw lines connecting the vertices
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(vertices[i], vertices[(i + 1) % 4]);
            Gizmos.DrawLine(vertices[i + 4], vertices[((i + 1) % 4) + 4]);
            Gizmos.DrawLine(vertices[i], vertices[i + 4]);
        }
    }
}

