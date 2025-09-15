
[assembly: FluentAssertions.Extensibility.AssertionEngineInitializer(
    typeof(AssertionEngineInitializer),
    nameof(AssertionEngineInitializer.AcknowledgeSoftWarning))]

public static class AssertionEngineInitializer
{
    public static void AcknowledgeSoftWarning()
    {
        FluentAssertions.License.Accepted = true;
    }
}