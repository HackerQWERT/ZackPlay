namespace XUnitTest.Base;


public static class AssertionEngineInitializer
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void AcknowledgeSoftWarning()
    {
        // Use reflection to set FluentAssertions.License.Accepted if the type exists,
        // avoiding a compile-time dependency on FluentAssertions.License.
        var licenseType = System.Type.GetType("FluentAssertions.License, FluentAssertions");
        if (licenseType != null)
        {
            var acceptedProp = licenseType.GetProperty("Accepted", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (acceptedProp != null && acceptedProp.CanWrite)
            {
                acceptedProp.SetValue(null, true);
            }
        }
    }
}