using System.Collections;
using UnityEngine;
namespace HybridBT2
{
    /// <summary>
    /// Base behaviour tree class. Contains a blackboard and root node.
    /// </summary>
    /// <typeparam name="T">TargetKey data type.</typeparam>
    public abstract class BT : MonoBehaviour, IResettable
    {
        protected Blackboard blackboard;
        [SerializeField] protected NodeData rootData;
        /// <summary>
        /// Time between tree ticks.
        /// </summary>
        [SerializeField] protected float tickInterval = 0.1f;
        protected WaitForSeconds intervalWait;
        [SerializeField] protected bool shouldTick = true;
        protected WaitUntil pauseWait;
        public Node Root { get; protected set; }
        protected Coroutine coroutine;
        protected virtual void Awake()
        {
            intervalWait = new WaitForSeconds(tickInterval);
            pauseWait = new WaitUntil(() => shouldTick);
            blackboard = new(transform);
            SetupBlackboard();
            SetupTree();
        }
        protected abstract void SetupBlackboard();
        /// <summary>
        /// In the base version, sets up the root node from the root node data.
        /// </summary>
        protected virtual void SetupTree()
        {
            Root = rootData.GetNodeExternal();
        }
        protected virtual void OnEnable()
        {
            PerformReset();
            coroutine = StartCoroutine(TickLoop());
        }
        protected virtual void OnDisable()
        {
            blackboard.Clear();
            if (coroutine != null) StopCoroutine(coroutine);
        }

        public void PerformReset()
        {
            Resume();
            if (blackboard == null) return;
            Root?.Abort(blackboard);
            blackboard.PerformReset();
        }

        protected IEnumerator TickLoop()
        {
            while (gameObject.activeSelf)
            {
                yield return intervalWait;
                //if (!shouldTick) Debug.Log("Tree is paused.");
                yield return pauseWait;
                Root.Evaluate(blackboard);
                blackboard.Tick(tickInterval);
            }
        }

        public void SetValue(Blackboard.Keys key, object value) => blackboard.SetData(key, value);

        /// <summary>
        /// Sets shouldRun to false, pausing the tree.
        /// </summary>
        public void Pause() => shouldTick = false;

        /// <summary>
        /// Sets shouldRun to true, resuming the tree.
        /// </summary>
        public void Resume() => shouldTick = true;

        public void GetInfo() => Root.GetInfo(0);
    }
}
