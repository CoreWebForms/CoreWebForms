// MIT License.

using System;
using System.Web.UI;

namespace SystemWebUISample;

public partial class SiteMaster : MasterPage
{
    private static int count;

    public string Message { get; } = $"Count: {count++}";
}
