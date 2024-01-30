// MIT License.

using System;
using System.Web.UI;

namespace SystemWebUISample;

public partial class SiteMaster : MasterPage
{
    private static int count = 0;

    // Use a static count to validate that the static instance is shared
    public string OtherText = $"{count++}";
}
