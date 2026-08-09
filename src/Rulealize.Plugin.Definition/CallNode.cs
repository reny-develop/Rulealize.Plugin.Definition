// Copyright (c) 2026 Reny
// Licensed under the Apache License, Version 2.0.

using System.Collections.Immutable;
using System.Text.Json;
using Rulealize.Abstraction.Building;
using Rulealize.Abstraction.Evaluation;
using Rulealize.Abstraction.Node;
using Rulealize.Abstraction.Value;

namespace Rulealize.Plugin.Definition
{
    /// <summary>Applies a definition to arguments.</summary>
    /// <remarks>
    /// <para>
    /// Arguments are evaluated in the caller's scope and passed by value; the body then
    /// runs in a fresh frame holding only them. It cannot see the caller's locals. That is
    /// what keeps a definition's meaning fixed wherever it is used — Othello's
    /// single-direction flip helper takes its direction as a parameter and is unaffected by
    /// the fact that its caller's loop variable happens to be named the same thing.
    /// </para>
    /// <para>
    /// Arguments are named, never positional. Two coordinates of the same kind are easy to
    /// swap by accident, and naming them lets the document rule that out instead of the
    /// reader having to.
    /// </para>
    /// </remarks>
    internal sealed class CallNode(DefinitionDescriptor definition, ImmutableArray<ExpressionNode> arguments)
        : ExpressionNode
    {
        public static ExpressionNode Build(INodeBuildContext context)
        {
            string name = context.RequireString("def");
            if (!context.Definitions.TryResolve(name, out DefinitionDescriptor? definition))
            {
                throw context.Error("def", $"'{name}' is not defined.");
            }

            if (definition.IsNullary)
            {
                throw context.Error("def", $"'{name}' takes no parameters; reference it with def.ref.");
            }

            JsonElement element = context.GetRequiredProperty("args");
            if (element.ValueKind != JsonValueKind.Object)
            {
                throw context.Error("args", "must be an object mapping parameter names to expressions.");
            }

            Dictionary<string, JsonElement> supplied = new(StringComparer.Ordinal);
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (!supplied.TryAdd(property.Name, property.Value))
                {
                    throw context.Error("args", $"'{property.Name}' is given more than once.");
                }
            }

            // Built in the callee's declaration order, so that evaluation can hand the
            // runtime a positional span without the caller having to know about it.
            ImmutableArray<ExpressionNode>.Builder arguments =
                ImmutableArray.CreateBuilder<ExpressionNode>(definition.Parameters.Length);

            foreach (string parameter in definition.Parameters)
            {
                if (!supplied.Remove(parameter, out JsonElement value))
                {
                    throw context.Error("args", $"'{definition}' has a parameter '{parameter}' with no argument.");
                }

                arguments.Add(context.BuildExpression(value, $"args/{parameter}"));
            }

            if (supplied.Count > 0)
            {
                throw context.Error("args", $"'{definition}' has no parameter named '{supplied.Keys.First()}'.");
            }

            return new CallNode(definition, arguments.MoveToImmutable());
        }

        public override RuleValue Evaluate(IEvaluationContext context)
        {
            RuleValue[] values = new RuleValue[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                values[i] = arguments[i].Evaluate(context);
            }

            return context.Invoke(definition, values);
        }
    }
}
