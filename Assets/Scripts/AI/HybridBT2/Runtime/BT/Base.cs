using System;
using System.Collections.Generic;
using UnityEngine;

namespace HybridBT2
{
    public enum NodeState : byte
    {
        SUCCESS,
        RUNNING,
        FAILURE
    }
    public abstract class BaseElement
    {
        public readonly string Name;
        public BaseElement(string name)
        {
            Name = name;
        }
        /// <summary>
        /// Get the name of this object, indented a certain nr of times.
        /// </summary>
        /// <param name="indentation">How many times we should apply an indent.</param>
        /// <returns>The name of the object.</returns>
        public virtual string GetInfo(int indentation)
        {
            return new string('\t', indentation) + Name;
        }
    }
    public abstract class BaseElementData : ScriptableObject
    {
        [SerializeField] protected string myName;
        public string Name
        {
            get => myName.Length > 0 ? myName : name;
        }
    }
    public abstract class Node : BaseElement
    {
        /// <summary>
        /// HasTarget wrapper. Stores the last result and a name, together with the actual function.
        /// </summary>
        protected class Condition : BaseElement
        {
            readonly Func<Node, Blackboard, bool> function;
            public bool Result { get; protected set; }
            public Condition(string name, Func<Node, Blackboard, bool> function) : base(name)
            {
                this.function = function;
                Result = false;
            }
            public bool Evaluate(Node node, Blackboard context)
            {
                Result = function(node, context);
                return Result;
            }
        }
        protected NodeState state = NodeState.FAILURE;
        /// <summary>
        /// Current state of the node. Initially set to FAILURE.
        /// </summary>
        public NodeState State { get => state; }
        /// <summary>
        /// If the new value is RUNNING, will connect Abort to parent's Abort.
        /// Otherwise if the previous state was RUNNING, will disconnect Abort to parent's Abort and 
        /// invoke onExit.
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="context"></param>
        public void SetState(NodeState newValue, Blackboard context)
        {
            if (state == newValue) return;

            if (newValue == NodeState.RUNNING)
            {
                onEnter?.Invoke(this, context);
                if (Parent != null) Parent.Abort += Abort;
            }
            else if (state == NodeState.RUNNING)
            {
                if (Parent != null) Parent.Abort -= Abort;
                onExit?.Invoke(this, context);
            }
            state = newValue;
        }
        /// <summary>
        /// Gets invoked when the node was previously RUNNING and either has blocking decorators or its parent aborted.
        /// </summary>
        public Action<Blackboard> Abort { get; set; }
        protected Node parent;
        public Node Parent
        {
            get => parent;
            set
            {
                parent ??= value;
            }
        }
        /// <summary>
        /// Fires when the node exits RUNNING status.
        /// </summary>
        protected Action<Node, Blackboard> onExit;
        /// <summary>
        /// Fires when the node is evaluated and not RUNNING.
        /// </summary>
        protected Action<Node, Blackboard> onEnter;
        protected readonly List<Condition> conditions = new();
        protected readonly List<Action<Node, Blackboard>> services = new();
        protected bool? alwaysCheck = null;
        public bool AlwaysCheck
        {
            get
            {
                return alwaysCheck ?? false;
            }
            set
            {
                if (alwaysCheck == null) alwaysCheck = value;
                else throw new InvalidOperationException($"Cannot set value of alwaysCheck after construction for {this}!");
            }
        }

        public Node(string name, Action<Node, Blackboard> onEnter, Action<Node, Blackboard> onExit) : base(name)
        {
            this.onEnter = onEnter ?? delegate { };
            this.onExit = onExit ?? delegate { };
            Abort += (ctx) => SetState(NodeState.FAILURE, ctx);
        }
        /// <summary>
        /// Evaluates all conditions. If one fails, invokes Abort and stops execution.
        /// Otherwise will execute all services, invoke onEnter if the previous state of the node was not
        /// RUNNING, and then will fire ExecuteUnderlyingBehaviour.
        /// </summary>
        /// <param name="context">The context of the agent.</param>
        public virtual void Evaluate(Blackboard context)
        {
            for (var i = 0; i < conditions.Count; i++)
            {
                if (!conditions[i].Evaluate(this, context))
                {
                    if (State == NodeState.RUNNING) Abort(context);
                    return;
                }
            }
            for (var i = 0; i < services.Count; i++) services[i](this, context);

            SetState(NodeState.RUNNING, context);

            ExecuteUnderlyingBehaviour(context);
        }
        /// <summary>
        /// Contains the action the node should execute if possible.
        /// </summary>
        /// <param name="context">The context of the agent.</param>
        protected abstract void ExecuteUnderlyingBehaviour(Blackboard context);
        public void AddService(Action<Node, Blackboard> service)
        {
            if (service == null) throw new ArgumentNullException($"{this} was passed a null service!");
            services.Add(service);
        }
        public void AddCondition(ConditionData data)
        {
            if (data == null) throw new ArgumentNullException($"{this} was passed a null condition!");
            conditions.Add(new(data.Name, data.Function));
        }
        /// <summary>
        /// Get this node's name, state, and status of conditions.
        /// </summary>
        /// <param name="indentation">How many times indentation should be applied.</param>
        /// <returns>A string containing info about the node.</returns>
        public override string GetInfo(int indentation)
        {
            var s = base.GetInfo(indentation) + $"[{state}]";
            if (conditions.Count > 0)
            {
                s += "[";
                foreach (var condition in conditions)
                {
                    s += $"{condition.Name} {condition.Result};";
                }
                s += "]";
            }
            return s;
        }
    }
    public abstract class NodeData : BaseElementData
    {
        /// <summary>
        /// If true, this node will always be checked by selectors if it has priority.
        /// </summary>
        public bool AlwaysCheck = true;
        protected virtual Action<Node, Blackboard> onEnter { get => delegate { }; }
        protected virtual Action<Node, Blackboard> onExit { get => delegate { }; }
        public List<ConditionData> Conditions = new();
        public List<ServiceData> Services = new();
        protected abstract Node GetNodeInternal();
        public virtual Node GetNodeExternal()
        {
            var node = GetNodeInternal();
            node.AlwaysCheck = AlwaysCheck;
            foreach (var item in Conditions) node.AddCondition(item);
            foreach (var item in Services) node.AddService(item.Action);
            return node;
        }
    }
    public abstract class ConditionData : BaseElementData
    {
        public abstract Func<Node, Blackboard, bool> Function { get; }
    }
    public abstract class ServiceData : BaseElementData
    {
        public abstract Action<Node, Blackboard> Action { get; }
    }
}