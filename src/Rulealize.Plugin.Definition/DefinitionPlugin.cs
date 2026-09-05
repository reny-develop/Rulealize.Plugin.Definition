// Copyright (c) 2026 Reny
// Licensed under the Apache License, Version 2.0.

using Rulealize.Abstraction.Plugin;

namespace Rulealize.Plugin.Definition
{
    /// <summary>
    /// References and applications of a rule set's definitions, over the <c>def</c>
    /// namespace, and the <c>#</c> shorthand.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>definitions</c> section is structurally the core's: it holds each entry as a
    /// name, a parameter list and a body, and checks the reference graph for cycles. What
    /// the core never does is evaluate a body. That is this plugin's part, reached through
    /// the evaluation context.
    /// </para>
    /// <para>
    /// The split means a rule set that defines nothing does not need to load this plugin at
    /// all.
    /// </para>
    /// </remarks>
    public sealed class DefinitionPlugin : IRulealizePlugin
    {
        /// <inheritdoc />
        public PluginManifest Manifest { get; } =
            new("Rulealize.Plugin.Definition", new Version(1, 0, 1), "def", '#');

        /// <inheritdoc />
        public void Register(IPluginRegistry registry)
        {
            ArgumentNullException.ThrowIfNull(registry);

            registry.AddExpression("ref", ReferenceNode.Build);
            registry.AddExpression("call", CallNode.Build);
            registry.AddSugar(new ReferenceSugarExpander());
        }
    }
}
