using HybridBT2;
using NUnit.Framework;
using System;
using UnityEngine;
/// <summary>
/// This class tests functionality related to the BT part of HybridBT2.
/// </summary>
public class HybridBT2BTTests
{
    /// <summary>Returns a leaf that always returns the given state.</summary>
    static LeafNode ConstantLeaf(string name, NodeState returnState)
        => new(name, (n, ctx) => returnState);

    /// <summary>
    /// Returns a leaf that returns RUNNING for <paramref name="runCount"/> evaluations,
    /// then SUCCESS. onEnter resets a counter stored at <paramref name="key"/>.
    /// </summary>
    static LeafNode CountingLeaf(string name, int runCount, Blackboard.Keys key = 0)
        => new(name,
            (n, ctx) =>
            {
                int v = ctx.GetData<int>(key) + 1;
                ctx.SetData(key, v);
                return v <= runCount ? NodeState.RUNNING : NodeState.SUCCESS;
            },
            onEnter: (n, ctx) => ctx.SetData(key, 0),
            onExit: (n, ctx) => ctx.SetData(key, 0));

    [Test]
    public void Context_SetAndGetData_RoundTrips()
    {
        var ctx = new Blackboard(null);
        ctx.SetData(Blackboard.Keys.Target, 42);
        Assert.AreEqual(42, ctx.GetData<int>(Blackboard.Keys.Target));
    }

    [Test]
    public void Context_GetMissingKey_ReturnsDefault()
    {
        var ctx = new Blackboard(null);
        Assert.AreEqual(0, ctx.GetData<int>(Blackboard.Keys.Target));
        Assert.IsNull(ctx.GetData<string>(Blackboard.Keys.Target));
    }

    [Test]
    public void Context_OverwriteData_ReturnsNewValue()
    {
        var ctx = new Blackboard(null);
        ctx.SetData(0, 10);
        ctx.SetData(0, 99);
        Assert.AreEqual(99, ctx.GetData<int>(0));
    }

    [Test]
    public void LeafNode_InitialState_IsFailure()
    {
        var leaf = ConstantLeaf("l", NodeState.SUCCESS);
        Assert.AreEqual(NodeState.FAILURE, leaf.State);
    }

    [Test]
    public void LeafNode_ReturnsSuccess_SetsStateSuccess()
    {
        var ctx = new Blackboard(null);
        var leaf = ConstantLeaf("l", NodeState.SUCCESS);
        leaf.Evaluate(ctx);
        Assert.AreEqual(NodeState.SUCCESS, leaf.State);
    }

    [Test]
    public void LeafNode_ReturnsFailure_SetsStateFailure()
    {
        var ctx = new Blackboard(null);
        var leaf = ConstantLeaf("l", NodeState.FAILURE);
        leaf.Evaluate(ctx);
        Assert.AreEqual(NodeState.FAILURE, leaf.State);
    }

    [Test]
    public void LeafNode_ReturnsRunning_SetsStateRunning()
    {
        var ctx = new Blackboard(null);
        var leaf = ConstantLeaf("l", NodeState.RUNNING);
        leaf.Evaluate(ctx);
        Assert.AreEqual(NodeState.RUNNING, leaf.State);
    }

    [Test]
    public void LeafNode_OnEnter_CalledOnFirstEvaluate()
    {
        var ctx = new Blackboard(null);
        bool called = false;
        var leaf = new LeafNode("l", (n, c) => NodeState.RUNNING,
                          onEnter: (n, c) => called = true);
        leaf.Evaluate(ctx);
        Assert.IsTrue(called);
    }

    [Test]
    public void LeafNode_OnEnter_NotCalledWhileRunning()
    {
        var ctx = new Blackboard(null);
        int count = 0;
        var leaf = new LeafNode("l", (n, c) => NodeState.RUNNING,
                         onEnter: (n, c) => count++);
        leaf.Evaluate(ctx);
        leaf.Evaluate(ctx);
        leaf.Evaluate(ctx);
        Assert.AreEqual(1, count);
    }

    [Test]
    public void LeafNode_OnExit_CalledWhenLeavingRunning()
    {
        var ctx = new Blackboard(null);
        bool called = false;
        var leaf = CountingLeaf("l", runCount: 1, key: 0);
        var leaf2 = new LeafNode("l2", (n, c) =>
        {
            int v = c.GetData<int>(Blackboard.Keys.Target) + 1;
            c.SetData(Blackboard.Keys.Target, v);
            return v == 1 ? NodeState.RUNNING : NodeState.SUCCESS;
        },
            onExit: (n, c) => called = true);

        leaf2.Evaluate(ctx);
        leaf2.Evaluate(ctx);
        Assert.IsTrue(called);
    }

    [Test]
    public void LeafNode_Condition_BlocksExecutionOnFailure()
    {
        var ctx = new Blackboard(null);
        int hits = 0;
        var leaf = new LeafNode("l", (n, c) => { hits++; return NodeState.RUNNING; });
        leaf.AddCondition("never", (n, c) => false);
        leaf.Evaluate(ctx);
        Assert.AreEqual(0, hits);
        Assert.AreEqual(NodeState.FAILURE, leaf.State);
    }

    [Test]
    public void LeafNode_Condition_AllowsExecutionWhenTrue()
    {
        var ctx = new Blackboard(null);
        int hits = 0;
        var leaf = new LeafNode("l", (n, c) => { hits++; return NodeState.RUNNING; });
        leaf.AddCondition("always", (n, c) => true);
        leaf.Evaluate(ctx);
        Assert.AreEqual(1, hits);
    }

    [Test]
    public void LeafNode_Condition_AbortsRunningNode()
    {
        var ctx = new Blackboard(null);
        var leaf = new LeafNode("l", (n, c) => NodeState.RUNNING);
        leaf.AddCondition("check", (n, c) => c.GetData<int>(Blackboard.Keys.Target) == 0);

        leaf.Evaluate(ctx);
        Assert.AreEqual(NodeState.RUNNING, leaf.State);

        ctx.SetData(Blackboard.Keys.Target, 1);
        leaf.Evaluate(ctx);
        Assert.AreEqual(NodeState.FAILURE, leaf.State);
    }

    [Test]
    public void LeafNode_Service_ExecutedEachEvaluate()
    {
        var ctx = new Blackboard(null);
        var leaf = new LeafNode("l", (n, c) => NodeState.RUNNING);
        leaf.AddService((n, c) => c.SetData(Blackboard.Keys.Target, c.GetData<int>(Blackboard.Keys.Target) + 1));

        leaf.Evaluate(ctx);
        leaf.Evaluate(ctx);
        Assert.AreEqual(2, ctx.GetData<int>(Blackboard.Keys.Target));
    }

    [Test]
    public void LeafNode_MultipleConditions_AllMustPass()
    {
        var ctx = new Blackboard(null);
        int hits = 0;
        var leaf = new LeafNode("l", (n, c) => { hits++; return NodeState.RUNNING; });
        leaf.AddCondition("pass", (n, c) => true);
        leaf.AddCondition("block", (n, c) => false);
        leaf.Evaluate(ctx);
        Assert.AreEqual(0, hits);
    }

    [Test]
    public void Sequence_AllSuccess_ReturnsSuccess()
    {
        var ctx = new Blackboard(null);
        var seq = new Sequence("seq");
        seq.AddChild(ConstantLeaf("a", NodeState.SUCCESS));
        seq.AddChild(ConstantLeaf("b", NodeState.SUCCESS));
        seq.Evaluate(ctx);

        seq.Evaluate(ctx);
        Assert.AreEqual(NodeState.SUCCESS, seq.State);
    }

    [Test]
    public void Sequence_FirstChildFails_ReturnsFailure()
    {
        var ctx = new Blackboard(null);
        var seq = new Sequence("seq");
        seq.AddChild(ConstantLeaf("fail", NodeState.FAILURE));
        seq.AddChild(ConstantLeaf("ok", NodeState.SUCCESS));
        seq.Evaluate(ctx);
        Assert.AreEqual(NodeState.FAILURE, seq.State);
    }

    [Test]
    public void Sequence_FirstChildRunning_BlocksSecondChild()
    {
        var ctx = new Blackboard(null);
        bool second = false;
        var seq = new Sequence("seq");
        seq.AddChild(ConstantLeaf("run", NodeState.RUNNING));
        seq.AddChild(new LeafNode("b", (n, c) => { second = true; return NodeState.SUCCESS; }));
        seq.Evaluate(ctx);
        Assert.IsFalse(second);
        Assert.AreEqual(NodeState.RUNNING, seq.State);
    }

    [Test]
    public void Sequence_ResetsChildIndexOnReEntry()
    {
        var ctx = new Blackboard(null);
        int aHits = 0, bHits = 0;
        var a = new LeafNode("a", (n, c) => { aHits++; return NodeState.SUCCESS; });
        var b = new LeafNode("b", (n, c) => { bHits++; return NodeState.SUCCESS; });
        var seq = new Sequence("seq");
        seq.AddChild(a);
        seq.AddChild(b);

        seq.Evaluate(ctx); // a runs
        seq.Evaluate(ctx); // b runs 
        Assert.AreEqual(NodeState.SUCCESS, seq.State);

        seq.Evaluate(ctx);
        Assert.AreEqual(2, aHits);
    }

    [Test]
    public void Sequence_SecondChildFails_AbortsSequence()
    {
        var ctx = new Blackboard(null);
        var seq = new Sequence("seq");
        seq.AddChild(ConstantLeaf("ok", NodeState.SUCCESS));
        seq.AddChild(ConstantLeaf("fail", NodeState.FAILURE));
        seq.Evaluate(ctx); // first child succeeds
        seq.Evaluate(ctx); // second child fails
        Assert.AreEqual(NodeState.FAILURE, seq.State);
    }

    [Test]
    public void Sequence_ThreeChildren_RunInOrder()
    {
        var ctx = new Blackboard(null);
        int order = 0;
        var seq = new Sequence("seq");

        var a = new LeafNode("a", (n, c) => { order = 1; return NodeState.SUCCESS; });
        var b = new LeafNode("b", (n, c) => { order = 2; return NodeState.SUCCESS; });
        var c2 = new LeafNode("c", (n, c) => { order = 3; return NodeState.SUCCESS; });
        seq.AddChild(a); seq.AddChild(b); seq.AddChild(c2);

        seq.Evaluate(ctx); Assert.AreEqual(1, order);
        seq.Evaluate(ctx); Assert.AreEqual(2, order);
        seq.Evaluate(ctx); Assert.AreEqual(3, order);
        Assert.AreEqual(NodeState.SUCCESS, seq.State);
    }

    [Test]
    public void Selector_FirstChildSucceeds_ReturnsSuccess()
    {
        var ctx = new Blackboard(null);
        var sel = new Selector("sel");
        sel.AddChild(ConstantLeaf("ok", NodeState.SUCCESS));
        sel.AddChild(ConstantLeaf("skip", NodeState.SUCCESS));
        sel.Evaluate(ctx);
        Assert.AreEqual(NodeState.SUCCESS, sel.State);
        Assert.AreEqual(0, sel.PrevChild);
    }

    [Test]
    public void Selector_AllChildrenFail_ReturnsFailure()
    {
        var ctx = new Blackboard(null);
        var sel = new Selector("sel");
        sel.AddChild(ConstantLeaf("a", NodeState.FAILURE));
        sel.AddChild(ConstantLeaf("b", NodeState.FAILURE));
        sel.Evaluate(ctx);
        Assert.AreEqual(NodeState.FAILURE, sel.State);
    }

    [Test]
    public void Selector_FirstFailsSecondSucceeds_ReturnsSuccess()
    {
        var ctx = new Blackboard(null);
        var sel = new Selector("sel");
        sel.AddChild(ConstantLeaf("fail", NodeState.FAILURE));
        sel.AddChild(ConstantLeaf("ok", NodeState.SUCCESS));
        sel.Evaluate(ctx);
        Assert.AreEqual(NodeState.SUCCESS, sel.State);
        Assert.AreEqual(1, sel.PrevChild);
    }

    [Test]
    public void Selector_HigherPriorityChildBecomesAvailable_AbortsLower()
    {
        var ctx = new Blackboard(null);
        bool aborted = false;

        var high = new LeafNode("high", (n, c) =>
            c.GetData<int>(0) >= 0 ? NodeState.RUNNING : NodeState.FAILURE);

        var low = new LeafNode("low", (n, c) => NodeState.RUNNING,
                      onExit: (n, c) => aborted = true);

        var sel = new Selector("sel1");
        sel.AddChild(high);
        sel.AddChild(low);

        ctx.SetData(0, -1);
        sel.Evaluate(ctx);
        Assert.AreEqual(1, sel.PrevChild);

        ctx.SetData(0, 1);
        sel.Evaluate(ctx);
        Assert.IsTrue(aborted);
        Assert.AreEqual(0, sel.PrevChild);
    }

    [Test]
    public void Selector_PrevChildResetOnReEntry()
    {
        var ctx = new Blackboard(null);
        var sel = new Selector("sel");
        sel.AddChild(ConstantLeaf("ok", NodeState.SUCCESS));

        sel.Evaluate(ctx);
        Assert.AreEqual(NodeState.SUCCESS, sel.State);

        sel.Evaluate(ctx);
        Assert.AreEqual(0, sel.PrevChild);
    }

    [Test]
    public void Parallel_BothRunning_ReturnsRunning()
    {
        var ctx = new Blackboard(null);
        var par = new ParallelNode("par",
            ConstantLeaf("a", NodeState.RUNNING),
            ConstantLeaf("b", NodeState.RUNNING));
        par.Evaluate(ctx);
        Assert.AreEqual(NodeState.RUNNING, par.State);
    }

    [Test]
    public void Parallel_FirstChildSuccess_PassesThrough()
    {
        var ctx = new Blackboard(null);
        var par = new ParallelNode("par",
            ConstantLeaf("a", NodeState.SUCCESS),
            ConstantLeaf("b", NodeState.RUNNING));
        par.Evaluate(ctx);
        Assert.AreEqual(NodeState.SUCCESS, par.State);
    }

    [Test]
    public void Parallel_FirstChildFails_AbortsSecondIfRunning()
    {
        var ctx = new Blackboard(null);
        bool aborted = false;
        var right = new LeafNode("b", (n, c) => NodeState.RUNNING,
                          onExit: (n, c) => aborted = true);
        var par = new ParallelNode("par",
            ConstantLeaf("a", NodeState.FAILURE), right);

        right.Evaluate(ctx);
        Assert.AreEqual(NodeState.RUNNING, right.State);

        par.Evaluate(ctx);
        Assert.AreEqual(NodeState.FAILURE, par.State);
        Assert.IsTrue(aborted);
    }

    [Test]
    public void Parallel_FirstFailsSecondNotRunning_NoAbort()
    {
        var ctx = new Blackboard(null);
        bool aborted = false;
        var right = new LeafNode("b", (n, c) => NodeState.SUCCESS,
                          onExit: (n, c) => aborted = true);
        var par = new ParallelNode("par",
            ConstantLeaf("a", NodeState.FAILURE), right);
        par.Evaluate(ctx);
        Assert.IsFalse(aborted);
    }

    [Test]
    public void Inverter_InvertsSuccess_ToFailure()
    {
        var ctx = new Blackboard(null);
        var inv = new Inverter("inv", null, null);
        inv.Child = ConstantLeaf("ok", NodeState.SUCCESS);
        inv.Evaluate(ctx);
        Assert.AreEqual(NodeState.FAILURE, inv.State);
    }

    [Test]
    public void Inverter_InvertsFailure_ToSuccess()
    {
        var ctx = new Blackboard(null);
        var inv = new Inverter("inv", null, null);
        inv.Child = ConstantLeaf("fail", NodeState.FAILURE);
        inv.Evaluate(ctx);
        Assert.AreEqual(NodeState.SUCCESS, inv.State);
    }

    [Test]
    public void Inverter_PassesThrough_Running()
    {
        var ctx = new Blackboard(null);
        var inv = new Inverter("inv", null, null);
        inv.Child = ConstantLeaf("run", NodeState.RUNNING);
        inv.Evaluate(ctx);
        Assert.AreEqual(NodeState.RUNNING, inv.State);
    }
    [Test]
    public void TimeLimit_WithinTime_PassesChildState()
    {
        var ctx = new Blackboard(null);
        ctx.SetDeltaTime(0.1f);
        var tl = new TimeLimit("tl", 5f, null, null);
        tl.Child = ConstantLeaf("run", NodeState.RUNNING);
        tl.Evaluate(ctx);
        Assert.AreEqual(NodeState.RUNNING, tl.State);
    }

    [Test]
    public void TimeLimit_ExceedsTime_ReturnsFailure()
    {
        var ctx = new Blackboard(null);
        ctx.SetDeltaTime(1f);
        var tl = new TimeLimit("tl", 2f, null, null);
        tl.Child = ConstantLeaf("run", NodeState.RUNNING);

        tl.Evaluate(ctx);
        tl.Evaluate(ctx);
        tl.Evaluate(ctx);
        Assert.AreEqual(NodeState.FAILURE, tl.State);
    }

    [Test]
    public void TimeLimit_ChildSucceeds_BeforeTimeout()
    {
        var ctx = new Blackboard(null);
        ctx.SetDeltaTime(0.1f);
        var tl = new TimeLimit("tl", 10f, null, null);
        tl.Child = ConstantLeaf("ok", NodeState.SUCCESS);
        tl.Evaluate(ctx);
        Assert.AreEqual(NodeState.SUCCESS, tl.State);
    }

    [Test]
    public void TimeLimit_StartTimeResetsOnReEntry()
    {
        var ctx = new Blackboard(null);
        ctx.SetDeltaTime(1f);
        var tl = new TimeLimit("tl", 1f, null, null);
        tl.Child = ConstantLeaf("run", NodeState.RUNNING);

        tl.Evaluate(ctx);
        tl.Evaluate(ctx);
        Assert.AreEqual(NodeState.FAILURE, tl.State);

        tl.Evaluate(ctx);
        Assert.AreEqual(NodeState.RUNNING, tl.State);
    }
    [Test]
    public void Repeater_CountsUpToMax_ThenSucceeds()
    {
        var ctx = new Blackboard(null);
        var rep = new Repeater("rep", 3, null, null);
        rep.Child = ConstantLeaf("ok", NodeState.SUCCESS);

        rep.Evaluate(ctx);
        Assert.AreEqual(NodeState.RUNNING, rep.State);
        Assert.AreEqual(1, rep.Count);
        rep.Evaluate(ctx);
        Assert.AreEqual(NodeState.RUNNING, rep.State);
        Assert.AreEqual(2, rep.Count);
        rep.Evaluate(ctx);
        Assert.AreEqual(NodeState.SUCCESS, rep.State);
        Assert.AreEqual(3, rep.Count);
    }

    [Test]
    public void Repeater_ChildFails_StopsAndReturnsFailure()
    {
        var ctx = new Blackboard(null);
        var rep = new Repeater("rep", 5, null, null)
        {
            Child = ConstantLeaf("fail", NodeState.FAILURE)
        };
        rep.Evaluate(ctx);
        Assert.AreEqual(NodeState.FAILURE, rep.State);
    }

    [Test]
    public void Repeater_Infinite_KeepsRunning()
    {
        var ctx = new Blackboard(null);
        var rep = new Repeater("rep", -1, null, null)
        {
            Child = ConstantLeaf("ok", NodeState.SUCCESS)
        };  // -1 means infinite
        for (int i = 0; i < 100; i++)
        {
            rep.Evaluate(ctx);
            Assert.AreEqual(NodeState.RUNNING, rep.State);
        }
    }

    [Test]
    public void Repeater_CountResetOnReEntry()
    {
        var ctx = new Blackboard(null);
        var rep = new Repeater("rep", 2, null, null)
        {
            Child = ConstantLeaf("ok", NodeState.SUCCESS)
        };

        rep.Evaluate(ctx);
        rep.Evaluate(ctx);
        Assert.AreEqual(NodeState.SUCCESS, rep.State);

        rep.Evaluate(ctx);
        Assert.AreEqual(1, rep.Count);
    }

    [Test]
    public void Retry_ChildFails_RetriesUpToMax_ThenFails()
    {
        var ctx = new Blackboard(null);
        var ret = new Retry("ret", 3, null, null);
        ret.Child = ConstantLeaf("fail", NodeState.FAILURE);

        ret.Evaluate(ctx);
        Assert.AreEqual(NodeState.RUNNING, ret.State);
        Assert.AreEqual(1, ret.Count);
        ret.Evaluate(ctx);
        Assert.AreEqual(NodeState.RUNNING, ret.State);
        Assert.AreEqual(2, ret.Count);
        ret.Evaluate(ctx);
        Assert.AreEqual(NodeState.FAILURE, ret.State);
        Assert.AreEqual(3, ret.Count);
    }

    [Test]
    public void Retry_ChildSucceeds_ReturnsSuccess()
    {
        var ctx = new Blackboard(null);
        var ret = new Retry("ret", 3, null, null);
        ret.Child = ConstantLeaf("ok", NodeState.SUCCESS);
        ret.Evaluate(ctx);
        Assert.AreEqual(NodeState.SUCCESS, ret.State);
    }

    [Test]
    public void Retry_Infinite_KeepsRunningOnFailure()
    {
        var ctx = new Blackboard(null);
        var ret = new Retry("ret", -1, null, null);
        ret.Child = ConstantLeaf("fail", NodeState.FAILURE);
        for (int i = 0; i < 50; i++)
        {
            ret.Evaluate(ctx);
            Assert.AreEqual(NodeState.RUNNING, ret.State);
        }
    }

    [Test]
    public void Retry_CountResetOnReEntry()
    {
        var ctx = new Blackboard(null);
        var ret = new Retry("ret", 2, null, null)
        {
            Child = ConstantLeaf("fail", NodeState.FAILURE)
        };

        ret.Evaluate(ctx);
        ret.Evaluate(ctx);
        Assert.AreEqual(NodeState.FAILURE, ret.State);

        ret.Evaluate(ctx);
        Assert.AreEqual(1, ret.Count);
    }
    [Test]
    public void Abort_SetsNodeToFailure()
    {
        var ctx = new Blackboard(null);
        var leaf = ConstantLeaf("l", NodeState.RUNNING);
        leaf.Evaluate(ctx);
        Assert.AreEqual(NodeState.RUNNING, leaf.State);

        leaf.Abort(ctx);
        Assert.AreEqual(NodeState.FAILURE, leaf.State);
    }

    [Test]
    public void Abort_PropagatesFromParentToChild()
    {
        var ctx = new Blackboard(null);
        var seq = new Sequence("seq");
        var child = ConstantLeaf("c", NodeState.RUNNING);
        seq.AddChild(child);

        seq.Evaluate(ctx);
        Assert.AreEqual(NodeState.RUNNING, child.State);

        seq.Abort(ctx);
        Assert.AreEqual(NodeState.FAILURE, seq.State);
    }

    [Test]
    public void Abort_OnlyTriggersOnExit_WhenWasRunning()
    {
        var ctx = new Blackboard(null);
        bool exited = false;
        var leaf = new LeafNode("l", (n, c) => NodeState.RUNNING,
                          onExit: (n, c) => exited = true);

        leaf.Abort(ctx);
        Assert.IsFalse(exited);

        leaf.Evaluate(ctx);
        leaf.Abort(ctx);
        Assert.IsTrue(exited);
    }

    [Test]
    public void GetInfo_ContainsNodeName()
    {
        var leaf = ConstantLeaf("MyLeaf", NodeState.SUCCESS);
        var info = leaf.GetInfo(0);
        StringAssert.Contains("MyLeaf", info);
    }

    [Test]
    public void GetInfo_ShowsConditionResult()
    {
        var ctx = new Blackboard(null);
        var leaf = new LeafNode("l", (n, c) => NodeState.RUNNING);
        leaf.AddCondition("myCondition", (n, c) => true);
        leaf.Evaluate(ctx);
        var info = leaf.GetInfo(0);
        StringAssert.Contains("myCondition", info);
    }

    [Test]
    public void Sequence_GetInfo_ListsChildren()
    {
        var seq = new Sequence("seq");
        seq.AddChild(ConstantLeaf("childA", NodeState.SUCCESS));
        seq.AddChild(ConstantLeaf("childB", NodeState.SUCCESS));
        var info = seq.GetInfo(0);
        StringAssert.Contains("childA", info);
        StringAssert.Contains("childB", info);
    }

    [Test]
    public void Integration_SequenceInsideSelector()
    {
        var ctx = new Blackboard(null);
        var seq = new Sequence("seq");
        seq.AddChild(ConstantLeaf("fail", NodeState.FAILURE));
        seq.AddChild(ConstantLeaf("ok", NodeState.SUCCESS));

        var sel = new Selector("sel");
        sel.AddChild(seq);
        sel.AddChild(ConstantLeaf("fallback", NodeState.SUCCESS));

        sel.Evaluate(ctx);
        Assert.AreEqual(NodeState.SUCCESS, sel.State);
        Assert.AreEqual(1, sel.PrevChild);
    }

    [Test]
    public void Integration_InverterWrappingSequence()
    {
        var ctx = new Blackboard(null);

        var inv = new Inverter("inv", null, null)
        {
            Child = ConstantLeaf("ok", NodeState.SUCCESS)
        };

        inv.Evaluate(ctx);
        Assert.AreEqual(NodeState.FAILURE, inv.State);
    }

    [Test]
    public void Integration_RepeaterWithCountingLeaf()
    {
        var ctx = new Blackboard(null);

        var rep = new Repeater("rep", 2, null, null)
        {
            Child = ConstantLeaf("ok", NodeState.SUCCESS)
        };

        rep.Evaluate(ctx);
        Assert.AreEqual(NodeState.RUNNING, rep.State);
        rep.Evaluate(ctx);
        Assert.AreEqual(NodeState.SUCCESS, rep.State);
    }

    [Test]
    public void Integration_TimeLimitWrapsSelector()
    {
        var ctx = new Blackboard(null);
        ctx.SetDeltaTime(1f);

        var tl = new TimeLimit("tl", 2f, null, null)
        {
            Child = ConstantLeaf("run", NodeState.RUNNING)
        };

        tl.Evaluate(ctx);
        Assert.AreEqual(NodeState.RUNNING, tl.State);
        tl.Evaluate(ctx);
        Assert.AreEqual(NodeState.RUNNING, tl.State);
        tl.Evaluate(ctx);
        Assert.AreEqual(NodeState.FAILURE, tl.State);
    }

    [Test]
    public void Integration_ServiceRunsBeforeExecute()
    {
        var ctx = new Blackboard(null);
        int order = 0;
        int serviceOrder = -1, executeOrder = -1;

        var leaf = new LeafNode("l", (n, c) =>
        {
            executeOrder = order++;
            return NodeState.RUNNING;
        });
        leaf.AddService((n, c) => serviceOrder = order++);

        leaf.Evaluate(ctx);
        Assert.Less(serviceOrder, executeOrder, "Service should run before ExecuteUnderlyingBehaviour");
    }

    /// <summary>Consideration whose score is set directly on the instance.</summary>
    private class StubConsideration : Consideration
    {
        public float Value;
        public override float Evaluate(Blackboard context) => Value;
    }

    /// <summary>ConditionData backed by a plain Func.</summary>
    private class StubConditionData : ConditionData
    {
        public Func<Node, Blackboard, bool> Func = (_, __) => false;
        public override Func<Node, Blackboard, bool> Function => Func;
    }

    /// <summary>Creates a StubConsideration that always returns <paramref name="value"/>.</summary>
    static StubConsideration MakeConsideration(float value)
    {
        var c = ScriptableObject.CreateInstance<StubConsideration>();
        c.Value = value;
        return c;
    }


    [Test]
    public void UtilityWrapper_ScoreMatchesConsiderationValue()
    {
        var ctx = new Blackboard(null);
        var wrapper = new UtilityWrapper("w", MakeConsideration(0.75f), null, null);
        wrapper.Child = ConstantLeaf("c", NodeState.SUCCESS);
        wrapper.GetUtility(ctx);

        Assert.AreEqual(0.75f, wrapper.Score, 1e-5f);
    }

    [Test]
    public void UtilityWrapper_ScoreStoredOnInstance()
    {
        var ctx = new Blackboard(null);
        var wrapper = new UtilityWrapper("w", MakeConsideration(0.4f), null, null);
        wrapper.Child = ConstantLeaf("c", NodeState.SUCCESS);
        wrapper.GetUtility(ctx);

        Assert.AreEqual(0.4f, wrapper.Score, 1e-5f);
    }

    [Test]
    public void UtilityWrapper_PassesThroughChildSuccess()
    {
        var ctx = new Blackboard(null);
        var wrapper = new UtilityWrapper("w", MakeConsideration(1f), null, null);
        wrapper.Child = ConstantLeaf("c", NodeState.SUCCESS);
        wrapper.Evaluate(ctx);

        Assert.AreEqual(NodeState.SUCCESS, wrapper.State);
    }

    [Test]
    public void UtilityWrapper_PassesThroughChildFailure()
    {
        var ctx = new Blackboard(null);
        var wrapper = new UtilityWrapper("w", MakeConsideration(1f), null, null);
        wrapper.Child = ConstantLeaf("c", NodeState.FAILURE);
        wrapper.Evaluate(ctx);

        Assert.AreEqual(NodeState.FAILURE, wrapper.State);
    }

    [Test]
    public void UtilityWrapper_PassesThroughChildRunning()
    {
        var ctx = new Blackboard(null);
        var wrapper = new UtilityWrapper("w", MakeConsideration(1f), null, null);
        wrapper.Child = ConstantLeaf("c", NodeState.RUNNING);
        wrapper.Evaluate(ctx);

        Assert.AreEqual(NodeState.RUNNING, wrapper.State);
    }

    [Test]
    public void UtilityWrapper_GetInfo_ContainsScore()
    {
        var ctx = new Blackboard(null);
        var wrapper = new UtilityWrapper("w", MakeConsideration(0.5f), null, null);
        wrapper.Child = ConstantLeaf("c", NodeState.SUCCESS);
        wrapper.GetUtility(ctx);

        StringAssert.Contains("Utility", wrapper.GetInfo(0));
    }

    [Test]
    public void UtilitySelector_AddChild_NullThrowsArgumentNullException()
    {
        var sel = new UtilitySelector("sel");
        Assert.Throws<ArgumentNullException>(() => sel.AddChild(null));
    }

    [Test]
    public void UtilitySelector_AddChild_NonWrapperThrowsArgumentException()
    {
        var sel = new UtilitySelector("sel");
        Assert.Throws<ArgumentException>(() => sel.AddChild(ConstantLeaf("l", NodeState.SUCCESS)));
    }

    [Test]
    public void UtilitySelector_AddChild_AssignsSequentialIndices()
    {
        var sel = new UtilitySelector("sel");
        var w0 = new UtilityWrapper("w0", MakeConsideration(1f), null, null);
        var w1 = new UtilityWrapper("w1", MakeConsideration(1f), null, null);
        w0.Child = ConstantLeaf("c0", NodeState.SUCCESS);
        w1.Child = ConstantLeaf("c1", NodeState.SUCCESS);

        sel.AddChild(w0);
        sel.AddChild(w1);

        Assert.AreEqual(0, w0.Index);
        Assert.AreEqual(1, w1.Index);
    }

    [Test]
    public void UtilitySelector_PicksHighestScoringChild()
    {
        var ctx = new Blackboard(null);
        var sel = new UtilitySelector("sel");

        var low = new UtilityWrapper("low", MakeConsideration(0.2f), null, null);
        low.Child = ConstantLeaf("lc", NodeState.SUCCESS);

        var high = new UtilityWrapper("high", MakeConsideration(0.9f), null, null);
        high.Child = ConstantLeaf("hc", NodeState.SUCCESS);

        sel.AddChild(low);
        sel.AddChild(high);
        sel.Evaluate(ctx);

        Assert.AreEqual(NodeState.SUCCESS, sel.State);
        Assert.AreEqual(NodeState.SUCCESS, high.State);
        Assert.AreEqual(NodeState.FAILURE, low.State);
    }

    [Test]
    public void UtilitySelector_FallsBackToNextWhenTopFails()
    {
        var ctx = new Blackboard(null);
        var sel = new UtilitySelector("sel");

        var top = new UtilityWrapper("top", MakeConsideration(0.9f), null, null);
        top.Child = ConstantLeaf("fail", NodeState.FAILURE);

        var fallback = new UtilityWrapper("fallback", MakeConsideration(0.4f), null, null);
        fallback.Child = ConstantLeaf("ok", NodeState.SUCCESS);

        sel.AddChild(top);
        sel.AddChild(fallback);
        sel.Evaluate(ctx);

        Assert.AreEqual(NodeState.SUCCESS, sel.State);
        Assert.AreEqual(NodeState.SUCCESS, fallback.State);
    }

    [Test]
    public void UtilitySelector_AllChildrenFail_ReturnsFailure()
    {
        var ctx = new Blackboard(null);
        var sel = new UtilitySelector("sel");

        var w1 = new UtilityWrapper("w1", MakeConsideration(0.9f), null, null);
        w1.Child = ConstantLeaf("f1", NodeState.FAILURE);
        var w2 = new UtilityWrapper("w2", MakeConsideration(0.3f), null, null);
        w2.Child = ConstantLeaf("f2", NodeState.FAILURE);

        sel.AddChild(w1);
        sel.AddChild(w2);
        sel.Evaluate(ctx);

        Assert.AreEqual(NodeState.FAILURE, sel.State);
    }

    [Test]
    public void UtilitySelector_SwitchingChild_AbortsThePreviousOne()
    {
        var ctx = new Blackboard(null);
        bool aborted = false;

        var consPrev = MakeConsideration(0.9f);
        var consNext = MakeConsideration(0.1f);

        var prev = new UtilityWrapper("prev", consPrev, null, null);
        prev.Child = new LeafNode("pc", (n, c) => NodeState.RUNNING,
            onExit: (n, c) => aborted = true);

        var next = new UtilityWrapper("next", consNext, null, null);
        next.Child = ConstantLeaf("nc", NodeState.SUCCESS);

        var sel = new UtilitySelector("sel");
        sel.AddChild(prev);
        sel.AddChild(next);

        sel.Evaluate(ctx);
        Assert.AreEqual(NodeState.RUNNING, sel.State);

        consPrev.Value = 0.1f;
        consNext.Value = 0.9f;

        sel.Evaluate(ctx);
        Assert.IsTrue(aborted, "The previously-running child should have been aborted.");
        Assert.AreEqual(NodeState.SUCCESS, sel.State);
    }

    [Test]
    public void UtilitySelector_DoesNotAbortWhenSameChildChosen()
    {
        var ctx = new Blackboard(null);
        bool aborted = false;

        var w = new UtilityWrapper("w", MakeConsideration(0.9f), null, null);
        w.Child = new LeafNode("c", (n, c) => NodeState.RUNNING,
            onExit: (n, c) => aborted = true);

        var sel = new UtilitySelector("sel");
        sel.AddChild(w);

        sel.Evaluate(ctx);
        sel.Evaluate(ctx);

        Assert.IsFalse(aborted, "The same running child should not be aborted when re-selected.");
    }

    [Test]
    public void UtilitySelector_PrevChildResetOnReEntry()
    {
        var ctx = new Blackboard(null);
        var sel = new UtilitySelector("sel");

        var w = new UtilityWrapper("w", MakeConsideration(1f), null, null);
        w.Child = ConstantLeaf("c", NodeState.SUCCESS);
        sel.AddChild(w);

        sel.Evaluate(ctx);
        Assert.AreEqual(NodeState.SUCCESS, sel.State);

        sel.Evaluate(ctx);
        Assert.AreEqual(NodeState.SUCCESS, sel.State);
    }

    [Test]
    public void UtilitySelector_GetInfo_ListsAllChildren()
    {
        var sel = new UtilitySelector("sel");

        var w1 = new UtilityWrapper("Alpha", MakeConsideration(1f), null, null);
        w1.Child = ConstantLeaf("c1", NodeState.SUCCESS);
        var w2 = new UtilityWrapper("Beta", MakeConsideration(0.5f), null, null);
        w2.Child = ConstantLeaf("c2", NodeState.SUCCESS);

        sel.AddChild(w1);
        sel.AddChild(w2);

        var info = sel.GetInfo(0);
        StringAssert.Contains("Alpha", info);
        StringAssert.Contains("Beta", info);
    }

    [Test]
    public void Integration_UtilitySelector_InsideSequence()
    {
        var ctx = new Blackboard(null);

        var low = new UtilityWrapper("low", MakeConsideration(0.1f), null, null);
        low.Child = ConstantLeaf("lc", NodeState.SUCCESS);

        var high = new UtilityWrapper("high", MakeConsideration(0.9f), null, null);
        high.Child = ConstantLeaf("hc", NodeState.SUCCESS);

        var utilSel = new UtilitySelector("util");
        utilSel.AddChild(low);
        utilSel.AddChild(high);

        var seq = new Sequence("seq");
        seq.AddChild(ConstantLeaf("pre", NodeState.SUCCESS));
        seq.AddChild(utilSel);

        seq.Evaluate(ctx); // pre-condition leaf succeeds
        seq.Evaluate(ctx); // utility selector runs

        Assert.AreEqual(NodeState.SUCCESS, seq.State);
        Assert.AreEqual(NodeState.SUCCESS, high.State);
    }

    [Test]
    public void Integration_UtilitySelector_FailsEntireSequenceWhenAllFail()
    {
        var ctx = new Blackboard(null);

        var w = new UtilityWrapper("w", MakeConsideration(1f), null, null);
        w.Child = ConstantLeaf("fail", NodeState.FAILURE);

        var utilSel = new UtilitySelector("util");
        utilSel.AddChild(w);

        var seq = new Sequence("seq");
        seq.AddChild(utilSel);

        seq.Evaluate(ctx);
        Assert.AreEqual(NodeState.FAILURE, seq.State);
    }

}