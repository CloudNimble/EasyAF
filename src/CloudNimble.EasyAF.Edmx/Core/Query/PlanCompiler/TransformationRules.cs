// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Query.InternalTrees;
using QueryRule = System.Data.Entity.Core.Query.InternalTrees.Rule;

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    // <summary>
    // The list of all transformation rules to apply
    // </summary>
    internal static class TransformationRules
    {
        // <summary>
        // A lookup table for built from all rules
        // The lookup table is an array indexed by OpType and each entry has a list of rules.
        // </summary>
        internal static readonly ReadOnlyCollection<ReadOnlyCollection<QueryRule>> AllRulesTable = BuildLookupTableForRules(AllRules);

        // <summary>
        // A lookup table for built only from ProjectRules
        // The lookup table is an array indexed by OpType and each entry has a list of rules.
        // </summary>
        internal static readonly ReadOnlyCollection<ReadOnlyCollection<QueryRule>> ProjectRulesTable =
            BuildLookupTableForRules(ProjectOpRules.Rules);

        // <summary>
        // A lookup table built only from rules that use key info
        // The lookup table is an array indexed by OpType and each entry has a list of rules.
        // </summary>
        internal static readonly ReadOnlyCollection<ReadOnlyCollection<QueryRule>> PostJoinEliminationRulesTable =
            BuildLookupTableForRules(PostJoinEliminationRules);

        // <summary>
        // A lookup table built only from rules that rely on nullability of vars and other rules
        // that may be able to perform simplificatios if these have been applied.
        // The lookup table is an array indexed by OpType and each entry has a list of rules.
        // </summary>
        internal static readonly ReadOnlyCollection<ReadOnlyCollection<QueryRule>> NullabilityRulesTable =
            BuildLookupTableForRules(NullabilityRules);

        // <summary>
        // A look-up table of rules that may cause modifications such that projection pruning may be useful
        // after they have been applied.
        // </summary>
        internal static readonly HashSet<QueryRule> RulesRequiringProjectionPruning = InitializeRulesRequiringProjectionPruning();

        // <summary>
        // A look-up table of rules that may cause modifications such that reapplying the nullability rules
        // may be useful after they have been applied.
        // </summary>
        internal static readonly HashSet<QueryRule> RulesRequiringNullabilityRulesToBeReapplied =
            InitializeRulesRequiringNullabilityRulesToBeReapplied();

        internal static readonly ReadOnlyCollection<ReadOnlyCollection<QueryRule>> NullSemanticsRulesTable =
            BuildLookupTableForRules(NullSemanticsRules);

        #region private state maintenance

        private static List<QueryRule> allRules;

        private static List<QueryRule> AllRules
        {
            get
            {
                if (allRules is null)
                {
                    allRules =
                    [
                        .. ScalarOpRules.Rules,
                        .. FilterOpRules.Rules,
                        .. ProjectOpRules.Rules,
                        .. ApplyOpRules.Rules,
                        .. JoinOpRules.Rules,
                        .. SingleRowOpRules.Rules,
                        .. SetOpRules.Rules,
                        .. GroupByOpRules.Rules,
                        .. SortOpRules.Rules,
                        .. ConstrainedSortOpRules.Rules,
                        .. DistinctOpRules.Rules,
                    ];
                }
                return allRules;
            }
        }

        private static List<QueryRule> postJoinEliminationRules;

        private static List<QueryRule> PostJoinEliminationRules
        {
            get
            {
                postJoinEliminationRules ??=
                    [
                        .. ProjectOpRules.Rules,
                        //these don't use key info per-se, but can help after the distinct op rules.
                        .. DistinctOpRules.Rules,
                        .. FilterOpRules.Rules,
                        .. ApplyOpRules.Rules,
                        .. JoinOpRules.Rules,
                        .. NullabilityRules,
                    ];
                return postJoinEliminationRules;
            }
        }

        private static List<QueryRule> nullabilityRules;

        private static List<QueryRule> NullabilityRules
        {
            get
            {
                nullabilityRules ??=
                    [
                        ScalarOpRules.Rule_IsNullOverVarRef,
                        ScalarOpRules.Rule_AndOverConstantPred1,
                        ScalarOpRules.Rule_AndOverConstantPred2,
                        ScalarOpRules.Rule_SimplifyCase,
                        ScalarOpRules.Rule_NotOverConstantPred,
                    ];
                return nullabilityRules;
            }
        }

        private static List<QueryRule> nullSemanticsRules;

        private static List<QueryRule> NullSemanticsRules
        {
            get
            {
                nullSemanticsRules ??=
                    [
                        ScalarOpRules.Rule_IsNullOverAnything,
                        ScalarOpRules.Rule_NullCast,
                        ScalarOpRules.Rule_EqualsOverConstant,
                        ScalarOpRules.Rule_AndOverConstantPred1,
                        ScalarOpRules.Rule_AndOverConstantPred2,
                        ScalarOpRules.Rule_OrOverConstantPred1,
                        ScalarOpRules.Rule_OrOverConstantPred2,
                        ScalarOpRules.Rule_NotOverConstantPred,
                        ScalarOpRules.Rule_LikeOverConstants,
                        ScalarOpRules.Rule_SimplifyCase,
                        ScalarOpRules.Rule_FlattenCase,
                    ];
                return nullSemanticsRules;
            }
        }

        private static ReadOnlyCollection<ReadOnlyCollection<QueryRule>> BuildLookupTableForRules(IEnumerable<QueryRule> rules)
        {
            var NoRules = new ReadOnlyCollection<QueryRule>([]);

            var lookupTable = new List<QueryRule>[(int)OpType.MaxMarker];

            foreach (var rule in rules)
            {
                var opRules = lookupTable[(int)rule.RuleOpType];
                if (opRules is null)
                {
                    opRules = [];
                    lookupTable[(int)rule.RuleOpType] = opRules;
                }
                opRules.Add(rule);
            }

            var rulesPerType = new ReadOnlyCollection<QueryRule>[lookupTable.Length];
            for (var i = 0; i < lookupTable.Length; ++i)
            {
                if (null != lookupTable[i])
                {
                    rulesPerType[i] = new ReadOnlyCollection<QueryRule>(lookupTable[i].ToArray());
                }
                else
                {
                    rulesPerType[i] = NoRules;
                }
            }
            return new ReadOnlyCollection<ReadOnlyCollection<QueryRule>>(rulesPerType);
        }

        private static HashSet<QueryRule> InitializeRulesRequiringProjectionPruning()
        {
            var rulesRequiringProjectionPruning = new HashSet<QueryRule>
            {
                ApplyOpRules.Rule_OuterApplyOverProject,
                JoinOpRules.Rule_CrossJoinOverProject1,
                JoinOpRules.Rule_CrossJoinOverProject2,
                JoinOpRules.Rule_InnerJoinOverProject1,
                JoinOpRules.Rule_InnerJoinOverProject2,
                JoinOpRules.Rule_OuterJoinOverProject2,
                ProjectOpRules.Rule_ProjectWithNoLocalDefs,
                FilterOpRules.Rule_FilterOverProject,
                FilterOpRules.Rule_FilterWithConstantPredicate,
                GroupByOpRules.Rule_GroupByOverProject,
                GroupByOpRules.Rule_GroupByOpWithSimpleVarRedefinitions
            };

            return rulesRequiringProjectionPruning;
        }

        private static HashSet<QueryRule> InitializeRulesRequiringNullabilityRulesToBeReapplied()
        {
            var rulesRequiringNullabilityRulesToBeReapplied = new HashSet<QueryRule>
            {
                FilterOpRules.Rule_FilterOverLeftOuterJoin
            };

            return rulesRequiringNullabilityRulesToBeReapplied;
        }

        #endregion

        // <summary>
        // Apply the rules that belong to the specified group to the given query tree.
        // </summary>
        internal static bool Process(PlanCompiler compilerState, TransformationRulesGroup rulesGroup)
        {
            ReadOnlyCollection<ReadOnlyCollection<QueryRule>> rulesTable = null;
            switch (rulesGroup)
            {
                case TransformationRulesGroup.All:
                    rulesTable = AllRulesTable;
                    break;
                case TransformationRulesGroup.PostJoinElimination:
                    rulesTable = PostJoinEliminationRulesTable;
                    break;
                case TransformationRulesGroup.Project:
                    rulesTable = ProjectRulesTable;
                    break;
                case TransformationRulesGroup.NullSemantics:
                    rulesTable = NullSemanticsRulesTable;
                    break;
            }

            // If any rule has been applied after which reapplying nullability rules may be useful,
            // reapply nullability rules.
            if (Process(compilerState, rulesTable, out var projectionPrunningRequired))
            {
                Process(compilerState, NullabilityRulesTable, out var projectionPrunningRequired2);
                projectionPrunningRequired = projectionPrunningRequired || projectionPrunningRequired2;
            }
            return projectionPrunningRequired;
        }

        // <summary>
        // Apply the rules that belong to the specified rules table to the given query tree.
        // </summary>
        // <param name="projectionPruningRequired"> is projection pruning required after the rule application </param>
        // <returns> Whether any rule has been applied after which reapplying nullability rules may be useful </returns>
        private static bool Process(
            PlanCompiler compilerState, ReadOnlyCollection<ReadOnlyCollection<QueryRule>> rulesTable, out bool projectionPruningRequired)
        {
            var ruleProcessor = new RuleProcessor();
            var context = new TransformationRulesContext(compilerState);
            compilerState.Command.Root = ruleProcessor.ApplyRulesToSubtree(context, rulesTable, compilerState.Command.Root);
            projectionPruningRequired = context.ProjectionPrunningRequired;
            return context.ReapplyNullabilityRules;
        }
    }
}
