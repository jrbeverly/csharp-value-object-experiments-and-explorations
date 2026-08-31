using Semantic.Abstractions;

namespace Sample;

[LifecycleValue(
    new string[] { "Metadata", "Open", "Closed" },
    new string[] { "Metadata:Open:Open", "Open:Close:Closed" }
)]
public class FileHandle
{
}
