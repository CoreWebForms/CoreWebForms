//------------------------------------------------------------------------------
// <copyright file="ScriptControlDescriptor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web.Resources;

namespace System.Web.UI
{
    public class ScriptControlDescriptor : ScriptComponentDescriptor {

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")]
        public ScriptControlDescriptor(string type, string elementID)
            : base(type, elementID) {
            RegisterDispose = false;
        }

        public override string ClientID {
            get {
                return ElementID;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")]
        public string ElementID {
            get {
                return base.ElementIDInternal;
            }
        }

        public override string ID {
            get {
                return base.ID;
            }
            set {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.InvariantCulture, AtlasWeb.ScriptControlDescriptor_IDNotSettable,
                                  "ID", typeof(ScriptControlDescriptor).FullName));
            }
        }
    }
}
