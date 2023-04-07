using UnityEngine;

public class KeepInFrame : MonoBehaviour
{
    public float speed = 1.0f;
    public bool forceFront = true;
    public Bounds boundingBox;

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
        if (exceedsFrustum)
        {
            Vector3 pushDirection = mainCamera.transform.forward;
            transform.position += pushDirection * speed * Time.deltaTime;
        }
    }

    // Get the vertices of the bounding box
    private Vector3[] GetBoundingBoxVertices()
    {
        Vector3[] vertices = new Vector3[8];
        vertices[0] = transform.TransformPoint(boundingBox.min);
        vertices[1] = transform.TransformPoint(new Vector3(boundingBox.min.x, boundingBox.min.y, boundingBox.max.z));
        vertices[2] = transform.TransformPoint(new Vector3(boundingBox.min.x, boundingBox.max.y, boundingBox.min.z));
        vertices[3] = transform.TransformPoint(new Vector3(boundingBox.min.x, boundingBox.max.y, boundingBox.max.z));
        vertices[4] = transform.TransformPoint(new Vector3(boundingBox.max.x, boundingBox.min.y, boundingBox.min.z));
        vertices[5] = transform.TransformPoint(new Vector3(boundingBox.max.x, boundingBox.min.y, boundingBox.max.z));
        vertices[6] = transform.TransformPoint(new Vector3(boundingBox.max.x, boundingBox.max.y, boundingBox.min.z));
        vertices[7] = transform.TransformPoint(boundingBox.max);

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

