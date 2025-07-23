using Moq.ReflectionProxy.Models.Utils.Markers;

namespace Moq.ReflectionProxy.Models.Intercepted;

public class InterceptSubstitution
{
    public Exception? ByException { get; set; }

    /// <summary>
    ///     TODO: check is not task in controller
    /// </summary>
    public object ByValue { get; set; } = new NotSet();
}