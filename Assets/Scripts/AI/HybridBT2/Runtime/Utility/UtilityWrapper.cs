using System;

namespace HybridBT2
{
    /// <summary>
    /// Contains one LeafNode and a consideration.
    /// </summary>
    public class UtilityWrapper : Decorator
    {
        /// <summary>
        /// This node's index in the UtilitySelector's children list. Used 
        /// to check if this node was the previous tick's best node.
        /// </summary>
        public int Index { get; set; }
        protected readonly Consideration consideration;
        /// <summary>
        /// The previous utility score, used for debugging and display purposes. 
        /// Updated every time GetUtility is called.
        /// </summary>
        public float Score { get; protected set; }

        public UtilityWrapper(string name, Consideration consideration,
            Action<Node, Blackboard> onEnter, Action<Node, Blackboard> onExit) : base(name, onEnter, onExit)
        {
            this.consideration = consideration;
        }
        /// <summary>
        /// Returns the utility score of this node, which is the result of evaluating the 
        /// consideration. Also updates the Score field for debugging purposes.
        /// </summary>
        /// <param name="context">The blackboard context used for evaluation.</param>
        /// <returns>The utility score of this node.</returns>
        public float GetUtility(Blackboard context)
        {
            //score exists only for debugging
            Score = consideration.Evaluate(context);
            return Score;
        }
        protected override void ExecuteUnderlyingBehaviour(Blackboard context)
        {
            Child.Evaluate(context);
            SetState(Child.State, context);
        }
        protected override string DecoratorInfo(int indentation)
        {
            return $"[Utility: {Score}]";
        }
    }
}