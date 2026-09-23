using System;
using UnityEngine;

namespace HybridBT2
{
    public abstract class Decorator : Node
    {
        protected Node child;
        public Node Child
        {
            get => child;
            set
            {
                child ??= value;
                child.Parent = this;
            }
        }
        protected Decorator(string name, Action<Node, Blackboard> onEnter, Action<Node, Blackboard> onExit) : base(name, onEnter, onExit)
        {

        }
        public override string GetInfo(int indentation)
        {
            return base.GetInfo(indentation) + "\n" + DecoratorInfo(indentation) + '\n' + Child.GetInfo(indentation + 1);
        }
        protected abstract string DecoratorInfo(int indentation);
    }

    public abstract class DecoratorData : NodeData
    {
        public NodeData Child;
        public override Node GetNodeExternal()
        {
            var n = (Decorator)base.GetNodeExternal();
            if (n == null) return null;
            n.Child = Child.GetNodeExternal();
            return n;
        }
    }
    public class Inverter : Decorator
    {
        public Inverter(string name, Action<Node, Blackboard> onEnter, Action<Node, Blackboard> onExit) : base(name, onEnter, onExit)
        {
        }
        protected override string DecoratorInfo(int indentation)
        {
            return "Inverter";
        }
        protected override void ExecuteUnderlyingBehaviour(Blackboard context)
        {
            Child.Evaluate(context);
            if (Child.State == NodeState.SUCCESS) SetState(NodeState.FAILURE, context);
            else if (Child.State == NodeState.FAILURE) SetState(NodeState.SUCCESS, context);
            else SetState(NodeState.RUNNING, context);
        }
    }
    public class TimeLimit : Decorator
    {
        public float MaxTime { get; protected set; }
        public float StartTime { get; protected set; }
        public float TimeLeft => MaxTime - StartTime;
        public TimeLimit(string name, float maxTime, Action<Node, Blackboard> onEnter, Action<Node, Blackboard> onExit) : base(name, onEnter, onExit)
        {
            MaxTime = maxTime;
            this.onEnter += (_, _) =>
            {
                //Debugging.Log("Resetting start time to " + 0);
                StartTime = 0;
            };
        }
        protected override string DecoratorInfo(int indentation)
        {
            return $"[Time left: {TimeLeft}]";
        }
        public bool CanExecute()
        {
            return StartTime < MaxTime;
        }
        protected override void ExecuteUnderlyingBehaviour(Blackboard context)
        {
            if (CanExecute())
            {
                Child.Evaluate(context);
                SetState(Child.State, context);
                StartTime += context.DeltaTime;
            }
            else SetState(NodeState.FAILURE, context);
        }
    }
    public class Repeater : Decorator
    {
        public int MaxNumber = -1;
        public int Count { get; protected set; }
        public Repeater(string name, int maxNumber, Action<Node, Blackboard> onEnter, Action<Node, Blackboard> onExit) : base(name, onEnter, onExit)
        {
            Debug.Assert(maxNumber != 0);
            MaxNumber = maxNumber;
            this.onEnter += (_, _) => Count = 0;
        }
        protected override string DecoratorInfo(int indentation)
        {
            return MaxNumber > 0 ? $"[Repeats: {Count}/{MaxNumber}]" : $"[Repeats:{Count}]";
        }

        protected override void ExecuteUnderlyingBehaviour(Blackboard context)
        {
            Child.Evaluate(context);
            if (Child.State != NodeState.FAILURE)
            {
                if (MaxNumber < 0 || Count < MaxNumber - 1)
                {
                    SetState(NodeState.RUNNING, context);
                }
                else SetState(NodeState.SUCCESS, context);
                Count++;
            }
            else SetState(NodeState.FAILURE, context);
        }
    }
    public class Retry : Decorator
    {
        public int MaxNumber = -1;
        public int Count { get; protected set; }
        public Retry(string name, int maxNumber, Action<Node, Blackboard> onEnter, Action<Node, Blackboard> onExit) : base(name, onEnter, onExit)
        {
            Debug.Assert(maxNumber != 0);
            MaxNumber = maxNumber;
            this.onEnter = onEnter ?? delegate { };
            this.onEnter += (_, _) => Count = 0;
        }
        protected override string DecoratorInfo(int indentation)
        {
            return MaxNumber > 0 ? $"[Repeats: {Count}/{MaxNumber}]" : $"[Repeats:{Count}]";
        }

        protected override void ExecuteUnderlyingBehaviour(Blackboard context)
        {
            Child.Evaluate(context);
            if (Child.State != NodeState.SUCCESS)
            {
                if (MaxNumber < 0 || Count < MaxNumber - 1)
                {
                    SetState(NodeState.RUNNING, context);
                }
                else SetState(NodeState.FAILURE, context);
                Count++;
            }
            else SetState(NodeState.SUCCESS, context);
        }
    }
    public class CooldownDecorator : Decorator
    {
        public float LastTime { get; protected set; }
        public readonly float Cooldown;
        protected bool hasExecuted;
        public CooldownDecorator(string name, float cooldown, Action<Node, Blackboard> onEnter, Action<Node, Blackboard> onExit) : base(name, onEnter, onExit)
        {
            Cooldown = cooldown;
            LastTime = -Cooldown;
            this.onExit ??= delegate { };
            this.onExit += (_, _) =>
            {
                if (hasExecuted)
                {
                    hasExecuted = false;
                    LastTime = Time.time;
                }
            };
        }
        public bool CanExecute()
        {
            return Time.time - LastTime >= Cooldown;
        }
        protected override string DecoratorInfo(int indentation)
        {
            return State == NodeState.RUNNING ? "Operating" : (CanExecute() ? "[Ready]" : $"[{Time.time - LastTime}/{Cooldown}]");
        }

        protected override void ExecuteUnderlyingBehaviour(Blackboard context)
        {
            if (CanExecute())
            {
                hasExecuted = true;
                Child.Evaluate(context);
                SetState(Child.State, context);
            }
            else SetState(NodeState.FAILURE, context);
        }
    }
}