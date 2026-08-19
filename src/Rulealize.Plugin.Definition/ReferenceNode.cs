// Copyright (c) 2026 Reny
// Licensed under the Apache License, Version 2.0.

using Rulealize.Abstraction.Building;
using Rulealize.Abstraction.Evaluation;
using Rulealize.Abstraction.Node;
using Rulealize.Abstraction.Plugin;
using Rulealize.Abstraction.Value;

namespace Rulealize.Plugin.Definition
{
    /// <summary>Evaluates a definition that takes no parameters.</summary>
    /// <remarks>
    /// <para>
    /// A name is resolved while the rule set is built, which is also what records the edge
    /// the core needs to reject a cyclic <c>definitions</c> section.
    /// </para>
    /// <para>
    /// The body is evaluated on every reference. Because bodies are pure, the runtime is
    /// free to memoise the result for a given snapshot and set of arguments, and it matters
    /// here: Reversi's flip computation is reached from a guard and from the effect that
    /// follows it, with the same argument, for each of sixty-four candidate moves.
    /// </para>
    /// </remarks>
    internal sealed class ReferenceNode(DefinitionDescriptor definition) : ExpressionNode
    {
        public static ExpressionNode Build(INodeBuildContext context) =>
            Resolve(context, context.RequireString("name"), "name");

        /// <summary>Resolves a nullary definition by name.</summary>
        /// <param name="context">The surrounding build state.</param>
        /// <param name="name">The name, without any shorthand prefix.</param>
        /// <param name="propertyName">The property to blame, or null to blame the node.</param>
        /// <returns>The node.</returns>
        public static ExpressionNode Resolve(IBuildContext context, string name, string? propertyName)
        {
            if (!context.Definitions.TryResolve(name, out DefinitionDescriptor? definition))
            {
                throw Error(context, propertyName, $"'{name}' is not defined.");
            }

            if (!definition.IsNullary)
            {
                throw Error(
                    context,
                    propertyName,
                    $"'{definition}' takes parameters; apply it with def.call rather than referencing it.");
            }

            return new ReferenceNode(definition);
        }

        public override RuleValue Evaluate(IEvaluationContext context) =>
            context.Invoke(definition, ReadOnlySpan<RuleValue>.Empty);

        private static Exception Error(IBuildContext context, string? propertyName, string message) =>
            propertyName is null ? context.Error(message) : context.Error(propertyName, message);
    }

    /// <summary>Expands <c>"#name"</c> into the node <c>def.ref</c> would have built.</summary>
    internal sealed class ReferenceSugarExpander : ISugarExpander
    {
        public ExpressionNode Expand(IBuildContext context, string text)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(text);

            string name = text[1..];
            if (name.Length == 0)
            {
                throw context.Error("'#' on its own does not name a definition.");
            }

            return ReferenceNode.Resolve(context, name, propertyName: null);
        }
    }
}
