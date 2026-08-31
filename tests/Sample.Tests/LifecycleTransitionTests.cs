namespace Sample.Tests;

public class LifecycleTransitionTests
{
    [Fact]
    public void Valid_transition_sequence_Metadata_to_Open_to_Closed()
    {
        // Each local variable is explicitly typed to prove the generated state type is correct.
        FileHandleMetadata metadata = new();
        FileHandleOpen open = metadata.Open();    // Metadata → Open
        FileHandleClosed closed = open.Close();  // Open → Closed

        Assert.NotNull(closed);
    }

    // Negative compile-time evidence — these lines do not compile:
    //
    //   new FileHandleMetadata().Close();
    //   // error CS1061: 'FileHandleMetadata' does not contain a definition for 'Close'
    //
    //   new FileHandleOpen().Open();
    //   // error CS1061: 'FileHandleOpen' does not contain a definition for 'Open'
    //
    // FileHandleMetadata exposes only Open(); FileHandleOpen exposes only Close().
    // An operation not valid for the current state is unrepresentable at compile time.
}
