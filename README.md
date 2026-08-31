# C# Semantic Value Object Experiments

Explores source-generated constrained scalars, composite identifiers, lifecycle-state types, and localized messages.

```csharp
[ConstrainedScalar(typeof(int), ScalarConstraint.Positive)]
public readonly partial struct PositiveCount { }

[LifecycleValue(
    new string[] { "Metadata", "Open", "Closed" },
    new string[] { "Metadata:Open:Open", "Open:Close:Closed" })]
public class FileHandle { }
```

Generated lifecycle states, one class per state (see `evidence/`):

```csharp
public sealed class FileHandleMetadata
{
    public FileHandleOpen Open() => new FileHandleOpen();
}

public sealed class FileHandleOpen
{
    public FileHandleClosed Close() => new FileHandleClosed();
}
```

`new FileHandleMetadata().Close()` fails to compile with `CS1061`.

```sh
make build
make test
```

## Notes

- Constrained scalar and composite values combine generated parsing, formatting, equality, and validation.
- Lifecycle declarations emit one type per state so invalid transitions fail at compile time.
- Localized messages generate typed placeholder parameters and culture-aware rendering.
- Analyzer diagnostics reject malformed declarations while runtime validation handles external input.
