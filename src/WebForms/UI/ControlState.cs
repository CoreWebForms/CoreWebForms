// MIT License.

// TODO: Remove once implemented
#pragma warning disable CA1822 // Mark members as static

namespace System.Web.UI;

internal enum ControlState
{
    Constructed = 0,
    FrameworkInitialized = 1,
    ChildrenInitialized = 2,
    Initialized = 3,
    ViewStateLoaded = 4,
    Loaded = 5,
    PreRendered = 6,
}
