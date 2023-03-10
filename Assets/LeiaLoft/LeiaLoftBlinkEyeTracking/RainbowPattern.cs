
using UnityEngine;

public class RainbowPattern : MonoBehaviour
{
    [SerializeField] private Material[] materials;
    [SerializeField] private Shader shader;
    [SerializeField] private Transform planePrefab;

    void Start()
    {
        for (int i = 0; i < 9; i++)
        {
            Transform newPlane = Instantiate(
                planePrefab,
                Vector3.zero,
                Quaternion.identity
                );
            MeshRenderer mr = newPlane.GetComponent<MeshRenderer>();
            mr.material = materials[i];

            newPlane.gameObject.layer = LayerMask.NameToLayer("View"+i);
            newPlane.rotation = Quaternion.Euler(90,180,0);
            newPlane.parent = transform;
        }
    }
}
