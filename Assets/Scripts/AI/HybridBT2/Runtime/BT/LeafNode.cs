using System;
using System.Collections.Generic;

namespace HybridBT2
{

    public sealed class LeafNode : Node
    {
        readonly Func<LeafNode, Blackboard, NodeState> onEvaluate;
        readonly Dictionary<int, object> data = new();

        public LeafNode(string name, Func<LeafNode, Blackboard, NodeState> onEvaluate, Action<Node, Blackboard> onEnter = null, Action<Node, Blackboard> onExit = null) : base(name, onEnter, onExit)
        {
            this.onEvaluate = onEvaluate;
        }

        public void SetData<R>(int key, R value) => data[key] = value;

        public R GetData<R>(int key)
        {
            return data.TryGetValue(key, out var value) ? (R)value : default;
        }

        protected override void ExecuteUnderlyingBehaviour(Blackboard context)
        {
            var newState = onEvaluate(this, context);
            SetState(newState, context);
        }
    }
    public abstract class LeafNodeData : NodeData
    {
        protected abstract Func<LeafNode, Blackboard, NodeState> onEvaluate { get; }
        protected override Node GetNodeInternal()
        {
            return new LeafNode(Name, onEvaluate, onEnter, onExit);
        }
    }
}