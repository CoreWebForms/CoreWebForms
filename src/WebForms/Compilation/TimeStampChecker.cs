//------------------------------------------------------------------------------
// <copyright file="TimeStampChecker.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

    using System;
    using System.Collections;
    using System.IO;
    using System.Web;
    using System.Web.Hosting;

    internal class TimeStampChecker {
        private Hashtable _timeStamps = new Hashtable(StringComparer.OrdinalIgnoreCase);

        private static AsyncLocal<TimeStampChecker> _currentTSC = new AsyncLocal<TimeStampChecker>();
        private static TimeStampChecker Current {
            get {
                // Create it on demand
                if (_currentTSC.Value == null) {
                    _currentTSC.Value = new TimeStampChecker();
                    Debug.WriteLine("TimeStampChecker", "Creating new TimeStampChecker");
                }

                return _currentTSC.Value;
            }
        }

        internal static void AddFile(string virtualPath, string path) {
            Current.AddFileInternal(virtualPath, path);
        }

        private void AddFileInternal(string virtualPath, string path) {
            DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(path);

            if (_timeStamps.Contains(virtualPath)) {
                DateTime storedValue = (DateTime)_timeStamps[virtualPath];

                // Already found to have changed before
                if (storedValue == DateTime.MaxValue) {
                    Debug.WriteLine("TimeStampChecker", "AddFileInternal: Same time stamp (" + path + ")");
                    return;
                }

                // If it's different, set it to MaxValue as marker of being invalid
                if (storedValue != lastWriteTimeUtc) {
                    _timeStamps[virtualPath] = DateTime.MaxValue;
                    Debug.WriteLine("TimeStampChecker", "AddFileInternal: Changed time stamp (" + path + ")");
                }
            }
            else {
                // New path: just add it
                _timeStamps[virtualPath] = lastWriteTimeUtc;
                Debug.WriteLine("TimeStampChecker", "AddFileInternal: New path (" + path + ")");
            }
        }

        internal static bool CheckFilesStillValid(string key, ICollection virtualPaths) {
            if (virtualPaths == null)
                return true;

            return Current.CheckFilesStillValidInternal(key, virtualPaths);
        }

        private bool CheckFilesStillValidInternal(string key, ICollection virtualPaths) {
            Debug.WriteLine("TimeStampChecker", "CheckFilesStillValidInternal (" + key + ")");

            foreach (string virtualPath in virtualPaths) {

                if (!_timeStamps.Contains(virtualPath))
                    continue;

                string path = new VirtualPath(virtualPath).MapPath();

                DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(path);
                DateTime storedValue = (DateTime)_timeStamps[virtualPath];

                // If it changed, then it's not valid
                if (lastWriteTimeUtc != storedValue) {
                    Debug.WriteLine("TimeStampChecker", "CheckFilesStillValidInternal: File (" + path + ") has changed!");

                    return false;
                }
            }

            Debug.WriteLine("TimeStampChecker", "CheckFilesStillValidInternal (" + key + ") is still valid");
            return true;
        }
    }
}

