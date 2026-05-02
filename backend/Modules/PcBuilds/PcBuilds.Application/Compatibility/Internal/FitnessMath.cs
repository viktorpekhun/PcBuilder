namespace PcBuilds.Application.Compatibility.Internal
{
    internal static class FitnessMath
    {
        internal static double Lerp(double a, double b, double t) => a + (b - a) * Math.Clamp(t, 0.0, 1.0);
    }
}
