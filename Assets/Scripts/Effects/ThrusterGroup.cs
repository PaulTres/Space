using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class ThrusterGroup : MonoBehaviour
{
    [SerializeField] protected Vector3 minScale = new(1, 1, .1f);
    [SerializeField] protected Vector3 maxScale = Vector3.one;
    [Tooltip("Multiplier applied to delta time when increasing or decreasing thruster size.")]
    [SerializeField] protected Vector3 scaleFactor = Vector3.one;
    [SerializeField] protected List<Transform> thrusterEffects;
    [SerializeField] protected bool editorTest = false;
    [SerializeField] protected Vector3 testScale = Vector3.one;

    protected void OnEnable()
    {
        SetScale(minScale);
    }

    protected void OnValidate()
    {
        if (thrusterEffects.IsEmpty())
        {
            for (int i = 0; i < transform.childCount; i++) thrusterEffects.Add(transform.GetChild(i));
        }
        if (editorTest)
        {
            //editorTest = false;
            SetScale(testScale);
        }
    }

    public void SetScale(Vector3 scale)
    {
        scale = new Vector3(
            Mathf.Clamp(scale.x, minScale.x, maxScale.x),
            Mathf.Clamp(scale.y, minScale.y, maxScale.y),
            Mathf.Clamp(scale.z, minScale.z, maxScale.z));
        for (int i = 0; i < thrusterEffects.Count; i++)
        {
            thrusterEffects[i].localScale = scale;
        }
    }
    public void Increase(float dt)
    {
        SetScale(thrusterEffects[0].localScale + dt * scaleFactor);
    }

    public void Decrease(float dt)
    {
        SetScale(thrusterEffects[0].localScale - dt * scaleFactor);
    }
}
