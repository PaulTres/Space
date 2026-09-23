using System;
using System.Collections.Generic;

namespace HybridBT2
{
    public abstract class Composite : Node
    {
        public Composite(string name, Action<Node, Blackboard> onEnter = null,
            Action<Node, Blackboard> onExit = null) : base(name, onEnter, onExit) { }
        public abstract void AddChild(Node child);
    }

    public abstract class RegularComposite : Composite
    {
        protected List<Node> children = new();

        public RegularComposite(string name, Action<Node, Blackboard> onEnter = null,
            Action<Node, Blackboard> onExit = null) : base(name, onEnter, onExit)
        {

        }

        /// <summary>
        /// Add the child to a list.
        /// </summary>
        /// <param name="child"></param>
        /// <exception cref="ArgumentNullException">Thrown if the method was passed a null child.</exception>
        public override void AddChild(Node child)
        {
            if (child == null) throw new ArgumentNullException($"{this} was passed a null child!");
            children.Add(child);
            child.Parent = this;
        }

        /// <summary>
        /// Return data about this composite. Same as the one for Node, but also returns the children 
        /// below, each on a separate line.
        /// </summary>
        /// <param name="indentation"></param>
        /// <returns></returns>
        public override string GetInfo(int indentation)
        {
            var s = base.GetInfo(indentation);
            for (int i = 0; i < children.Count; i++)
            {
                s += "\n" + children[i].GetInfo(indentation + 1);
            }
            return s;
        }
    }

    public abstract class RegularCompositeData : NodeData
    {
        public List<NodeData> Children;

        public override Node GetNodeExternal()
        {
            var node = (RegularComposite)base.GetNodeExternal();
            foreach (var item in Children) node.AddChild(item.GetNodeExternal());
            return node;
        }
    }

    public class Sequence : RegularComposite
    {
        protected int currentChild = 0;

        public Sequence(string name, Action<Node, Blackboard> onEnter = null, Action<Node, Blackboard> onExit = null) : base(name, onEnter, onExit)
        {
            this.onEnter += (_, _) =>
            {
                currentChild = 0;
                //Debugging.Log($"Entering {Name}");
            };
        }

        /// <summary>
        /// ExecuteUnderlyingBehaviour all children in sequence. Abort on child FAILURE.
        /// </summary>
        /// <param name="context"></param>
        protected override void ExecuteUnderlyingBehaviour(Blackboard context)
        {
            children[currentChild].Evaluate(context);
            //Debugging.Log($"{Name} evaluated {children[currentChild].Name} with state {children[currentChild].State}.");
            switch (children[currentChild].State)
            {
                case NodeState.FAILURE:
                    Abort(context);
                    break;
                case NodeState.RUNNING:
                    SetState(NodeState.RUNNING, context);
                    break;
                case NodeState.SUCCESS:
                    var newState = currentChild == children.Count - 1 ? NodeState.SUCCESS : NodeState.RUNNING;
                    SetState(newState, context);
                    currentChild = Math.Min(currentChild + 1, children.Count - 1);
                    break;
            }
        }
    }

    public class Selector : RegularComposite
    {
        protected int prevChild = -1;
        /// <summary>
        /// Added here for testing purposes. Returns the index of the child that was previously executed. -1 if none.
        /// </summary>
        public int PrevChild => prevChild;

        public Selector(string name, Action<Node, Blackboard> onEnter = null, Action<Node, Blackboard> onExit = null) : base(name, onEnter, onExit)
        {
            this.onEnter += (_, _) => prevChild = -1;
        }

        /// <summary>
        /// Executes the first child which does not fail. If previously had a lower priority child, 
        /// it will Abort it.
        /// </summary>
        /// <param name="context"></param>
        protected override void ExecuteUnderlyingBehaviour(Blackboard context)
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (prevChild != -1 && !children[i].AlwaysCheck && i != prevChild) continue;
                children[i].Evaluate(context);
                //Debugging.Log($"{Name} has evaluated {children[i].Name}.");
                if (children[i].State == NodeState.FAILURE)
                {
                    if (i == children.Count - 1)
                    {
                        //Debugging.Log($"{Name} has failed all children!");
                        SetState(NodeState.FAILURE, context);
                        return;
                    }
                    SetState(NodeState.RUNNING, context);
                    continue;
                }
                //if (Name.Contains("HitAndRun")) Debugging.Log(Name + ": " + i + "-" + children[i].Name);
                if (prevChild != -1 && i != prevChild)
                {
                    children[prevChild].Abort(context);
                }
                prevChild = i;
                SetState(children[prevChild].State, context);
                return;
            }
            throw new Exception($"{this} did not finish executing main loop!");
        }
    }

    public class ParallelNode : RegularComposite
    {
        public ParallelNode(string name, Node leftChild, Node rightChild, Action<Node, Blackboard> onEnter = null,
            Action<Node, Blackboard> onExit = null) : base(name, onEnter, onExit)
        {
            AddChild(leftChild);
            AddChild(rightChild);
        }

        /// <summary>
        /// Executes the first child. If not FAILURE, will evaluate the second, otherwise if 
        /// the second is RUNNING it will Abort it.
        /// </summary>
        /// <param name="context"></param>
        protected override void ExecuteUnderlyingBehaviour(Blackboard context)
        {
            children[0].Evaluate(context);
            SetState(children[0].State, context);
            if (State != NodeState.FAILURE) children[1].Evaluate(context);
            else if (children[1].State == NodeState.RUNNING) children[1].Abort(context);
        }
    }
}