// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Web.Optimization;
using System.Web.UI;

namespace Microsoft.AspNet.Web.Optimization.WebForms
{
    [DefaultProperty("Path")]
    [NonVisualControl]
    public class BundleReference : Control
    {
        private const string PathKey = "Path";

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        public string Path
        {
            get
            {
                return ViewState[PathKey] as string ?? String.Empty;
            }
            set
            {
                ViewState[PathKey] = value;
            }
        }

        public override void RenderControl(HtmlTextWriter writer)
        {
            writer.Write(Styles.Render(Path));
        }
    }
}
