# Rulealize.Plugin.Definition

References and applications of a [Rulealize](https://github.com/reny-develop/Rulealize)
rule set's `definitions` section.

| | |
| --- | --- |
| Plugin id | `Rulealize.Plugin.Definition` |
| Namespace | `def` |
| Reserved prefix | `#` |
| Depends on | `Rulealize.Abstraction` |

The `definitions` section is structurally the core's. It holds each entry as a name, a
parameter list and a body, and it rejects a cyclic reference graph before anything runs.
What the core never does is evaluate a body — that is this plugin's part, reached through
the evaluation context. So a rule set that defines nothing need not load this plugin at
all.

## Operations

```jsonc
{ "op": "def.ref", "name": "<name>" }                          // shorthand: "#<name>"

{ "op": "def.call", "def": "<name>", "args": { "<param>": <expr>, … } }
```

A definition with no parameters is referenced; one with parameters is called. Using the
wrong form is a build error.

## The section itself

```jsonc
"definitions": {
  "<name>": { "params": ["<name>", …], "body": <expr> }
}
```

`params` may be omitted, and then the short form applies: an object with an `op` key
written directly is read as a body with no parameters.

```jsonc
"me": { "op": "state.get", "path": "turn" }
```

## Definitions are hygienic

**A body cannot see the caller's locals.** The scope it evaluates in holds the state, the
other definitions, and its own parameters. Nothing else.

Arguments are the only channel in, and that is what fixes a definition's meaning
independently of where it is used. Othello's single-direction flip helper takes `at` and
`dir` as parameters, and is unaffected by its caller happening to bind a loop variable
named `d`:

```jsonc
{ "op": "seq.selectMany",
  "source": { "op": "grid.directions", "of": "$board", "kind": "eight" },
  "as": "d",
  "select": { "op": "def.call", "def": "flips1", "args": { "at": "@at", "dir": "@d" } } }
```

Both `@at` and `@d` are evaluated in the caller's scope, before the call.

Arguments are named rather than positional. Two coordinates of the same kind are easy to
transpose, and naming them lets the document rule that out instead of the reader having to.
The key set must match the parameter list exactly; anything else is a build error.

## No recursion

The reference graph must be acyclic, checked when the rule set is built.

Two reasons. Termination could not be guaranteed otherwise, and `GetValidInputs` evaluates
hundreds of candidates — one non-terminating definition stops everything. And with the call
graph fixed, the cost of an evaluation has an upper bound that can be estimated.

Othello does not need recursion. Neither, as far as this design has been pushed, does
anything else that iteration can express.

## Memoisation

The body is evaluated on every reference, but bodies are pure, so the runtime is free to
cache a result against the definition, its arguments, and the state snapshot.

That freedom is worth real time here. Othello's flip computation is reached from a guard
and again from the effect that follows it, with the same argument — and `GetValidInputs`
does this for each of sixty-four candidate moves, each of which walks eight rays.

## Building

`Rulealize.Abstraction` is not on nuget.org yet, so `NuGet.config` points at a folder
feed. Produce it from the abstraction repository first:

```
dotnet pack path\to\Rulealize.Abstraction\src\Rulealize.Abstraction -c Release -o path\to\LocalNuGet
```

with `LocalNuGet` a sibling of this repository. Then `dotnet build`.

## License

Apache-2.0.
