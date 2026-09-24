using UnityEngine;

public class DebugScript : MonoBehaviour
{
    [SerializeField] protected string text;

    public void Execute() => Debug.Log(text);

    public void Execute(string text)=>Debug.Log(text);
}
