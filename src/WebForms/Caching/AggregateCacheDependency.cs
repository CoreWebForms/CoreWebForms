// MIT License.

using System.Collections;
using System.Text;

namespace System.Web.Caching;

internal sealed class AggregateCacheDependency : CacheDependency
{
    ArrayList _dependencies;
    bool _disposed;

    public AggregateCacheDependency()
    {
        // The ctor of every class derived from CacheDependency must call this.
        FinishInit();
    }

    public void Add(params CacheDependency[] dependencies)
    {
        DateTime utcLastModified = DateTime.MinValue;

        if (dependencies == null)
        {
            throw new ArgumentNullException(nameof(dependencies));
        }

        // copy array argument contents so they can't be changed beneath us
        dependencies = (CacheDependency[])dependencies.Clone();

        // validate contents
        foreach (CacheDependency d in dependencies)
        {
            if (d == null)
            {
                throw new ArgumentNullException(nameof(dependencies));
            }

#if PORT_CACHE_TAKE_OWNERSHIP
            if (!d.TakeOwnership())
            {
                throw new InvalidOperationException(SR.GetString(SR.Cache_dependency_used_more_that_once));
            }
#endif
        }

        // add dependencies, and check if any have changed
        bool hasChanged = false;
        lock (this)
        {
            if (!_disposed)
            {
                if (_dependencies == null)
                {
                    _dependencies = new ArrayList();
                }

                _dependencies.AddRange(dependencies);

                foreach (CacheDependency d in dependencies)
                {
                    d.SetCacheDependencyChanged((Object sender, EventArgs args) => {
                        DependencyChanged(sender, args);
                    });

                    if (d.UtcLastModified > utcLastModified)
                    {
                        utcLastModified = d.UtcLastModified;
                    }

                    if (d.HasChanged)
                    {
                        hasChanged = true;
                        break;
                    }
                }
            }
        }

        SetUtcLastModified(utcLastModified);

        // if a dependency has changed, notify others that we have changed.
        if (hasChanged)
        {
            NotifyDependencyChanged(this, EventArgs.Empty);
        }
    }

    // Dispose our dependencies. Note that the call to this
    // function is thread safe.

    protected override void DependencyDispose()
    {
        CacheDependency[] dependencies = null;

        lock (this)
        {
            _disposed = true;
            if (_dependencies != null)
            {
                dependencies = (CacheDependency[])_dependencies.ToArray(typeof(CacheDependency));
                _dependencies = null;
            }
        }

        if (dependencies != null)
        {
            foreach (CacheDependency d in dependencies)
            {
                d.Dispose();
            }
        }
    }

    // Forward call from the aggregate to the CacheEntry

    /// <internalonly/>
    void DependencyChanged(Object sender, EventArgs e)
    {
        NotifyDependencyChanged(sender, e);
    }

    public override string GetUniqueID()
    {
        StringBuilder sb = null;
        CacheDependency[] dependencies = null;

        //VSWhidbey 354570: return null if this AggregateCacheDependency cannot otherwise return a unique ID
        if (_dependencies == null)
        {
            return null;
        }

        lock (this)
        {
            if (_dependencies != null)
            {
                dependencies = (CacheDependency[])_dependencies.ToArray(typeof(CacheDependency));
            }
        }

        if (dependencies != null)
        {
            foreach (CacheDependency dependency in dependencies)
            {
                string id = dependency.GetUniqueID();

                if (id == null)
                {
                    // When AggregateCacheDependency contains a dependency for which GetUniqueID() returns null, 
                    // it should return null itself.  This is because it can no longer generate a UniqueID that 
                    // is guaranteed to be different when any of the dependencies change.
                    return null;
                }

                if (sb == null)
                {
                    sb = new StringBuilder();
                }
                sb.Append(id);
            }
        }

        return sb != null ? sb.ToString() : null;
    }

    internal CacheDependency[] GetDependencyArray()
    {
        CacheDependency[] dependencies = null;

        lock (this)
        {
            if (_dependencies != null)
            {
                dependencies = (CacheDependency[])_dependencies.ToArray(typeof(CacheDependency));
            }
        }

        return dependencies;
    }

#if PORT_CACHE_INTERNAL
    //
    //  This will examine the dependencies and only return true if ALL dependencies are file dependencies
    //
    internal override bool IsFileDependency()
    {
        CacheDependency[] dependencies = null;

        dependencies = GetDependencyArray();
        if (dependencies == null)
        {
            return false;
        }

        foreach (CacheDependency d in dependencies)
        {
            // We should only check if the type is either CacheDependency or the Aggregate.
            // Anything else, we can't guarantee that it's a file only dependency.
            if (!object.ReferenceEquals(d.GetType(), typeof(CacheDependency)) &&
                 !object.ReferenceEquals(d.GetType(), typeof(AggregateCacheDependency)))
            {
                return false;
            }

            if (!d.IsFileDependency())
            {
                return false;
            }
        }

        return true;
    }
#endif

    /// <summary>
    /// This method will return only the file dependencies from this dependency
    /// </summary>
    /// <returns></returns>
    public override string[] GetFileDependencies()
    {
        ArrayList fileNames = null;
        CacheDependency[] dependencies = null;

        dependencies = GetDependencyArray();
        if (dependencies == null)
        {
            return null;
        }

        foreach (CacheDependency d in dependencies)
        {
            // Check if the type is either CacheDependency or an Aggregate;
            // for anything else, we can't guarantee it's a file only dependency.
            if (object.ReferenceEquals(d.GetType(), typeof(CacheDependency))
                || object.ReferenceEquals(d.GetType(), typeof(AggregateCacheDependency)))
            {

                string[] tmpFileNames = d.GetFileDependencies();

                if (tmpFileNames != null)
                {

                    if (fileNames == null)
                    {
                        fileNames = new ArrayList();
                    }

                    fileNames.AddRange(tmpFileNames);
                }
            }
        }

        if (fileNames != null)
        {
            return (string[])fileNames.ToArray(typeof(string));
        }
        else
        {
            return null;
        }
    }
}
