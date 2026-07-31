using UnityEngine;

public class MaterialColorTest : MonoBehaviour
{
    [SerializeField] protected Color initialColor, finalColor;
    [Range(0, 1)]
    public float t = 0;
    private void Update()
    {
        GetComponent<Renderer>().material.color = Color.Lerp(initialColor, finalColor, t);
    }
}
