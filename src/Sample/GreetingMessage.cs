using Semantic.Abstractions;

namespace Sample;

[LocalizedMessage(
    "Hello, {name}! Score: {score}",
    new string[] { "string:name", "decimal:score" }
)]
public partial class GreetingMessage
{
}
