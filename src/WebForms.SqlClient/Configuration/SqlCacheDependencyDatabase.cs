// MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.Configuration;

internal class SqlCacheDependencyDatabase
{
    public bool Enabled { get; internal set; }
    public int PollTime { get; internal set; }
    public object ConnectionStringName { get; internal set; }
}
