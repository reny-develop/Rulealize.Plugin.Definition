# Rulealize.Plugin.Definition

References and applications of a [Rulealize](https://github.com/reny-develop/Rulealize)
rule set's `definitions` section.

| | |
| --- | --- |
| Plugin id | `Rulealize.Plugin.Definition` |
| Namespace | `def` |
| Reserved prefix | `#` |
| Depends on | `Rulealize.Abstraction` |
| Specification | [doc/specification.md](doc/specification.md) |

`def.ref` refers to a definition with no parameters, with `"#name"` as its shorthand;
`def.call` applies one that has parameters.

The `definitions` section is structurally the core's. It holds each entry as a name, a
parameter list and a body, and it rejects a cyclic reference graph before anything runs.
What the core never does is evaluate a body — that is this plugin's part, reached through
the evaluation context. So a rule set that defines nothing need not load this plugin at all.

Two things the specification settles that are worth knowing before you read it. Bodies are
**hygienic**: a body sees the state, the other definitions, and its own parameters, and
nothing from wherever it was called, which is what fixes a definition's meaning
independently of its call sites. And **recursion is refused** — the reference graph has to
be acyclic — because termination could not be guaranteed otherwise and `GetValidInputs`
evaluates hundreds of candidates per call.

## Building

`Rulealize.Abstraction` is not on nuget.org yet, so `NuGet.config` points at a folder
feed. Produce it from the abstraction repository first:

```
dotnet pack path\to\Rulealize.Abstraction\src\Rulealize.Abstraction -c Release -o path\to\LocalNuGet
```

with `LocalNuGet` a sibling of this repository. Then `dotnet build`.

## License

Apache-2.0.
