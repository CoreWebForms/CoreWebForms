// MIT License.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Web.UI
{
    public sealed class RegisteredScript
    {
        private readonly RegisteredScriptType _scriptType;
        private readonly Control _control;
        private readonly string _key;
        private readonly string _script;
        private readonly Type _type;
        private readonly bool _addScriptTags;
        private readonly string _url;

        internal RegisteredScript(Control control, Type type, string key, string url)
        {
            Debug.Assert(control != null);
            Debug.Assert(type != null);
            Debug.Assert(!String.IsNullOrEmpty(url));
            // null and empty "key" are treated different by asp.net script duplicate detection so null is allowed.
            _scriptType = RegisteredScriptType.ClientScriptInclude;
            _control = control;
            _type = type;
            _key = key;
            _url = url;
        }

        internal RegisteredScript(RegisteredScriptType scriptType,
            Control control,
            Type type,
            string key,
            string script,
            bool addScriptTags)
        {

            Debug.Assert(control != null);
            Debug.Assert(
                scriptType != RegisteredScriptType.OnSubmitStatement || !addScriptTags,
                "OnSubmitStatements cannot have addScriptTags.");
            Debug.Assert(type != null);
            // null and empty "key" are treated different by asp.net script duplicate detection so null is allowed.
            // null script allowed

            _scriptType = scriptType;
            _control = control;
            _type = type;
            _key = key;
            _script = script;
            _addScriptTags = addScriptTags;
        }

        public bool AddScriptTags
        {
            get
            {
                return _addScriptTags;
            }
        }

        public Control Control
        {
            get
            {
                return _control;
            }
        }

        public string Key
        {
            get
            {
                // may be null
                return _key;
            }
        }

        public string Script
        {
            get
            {
                // may be null
                return _script;
            }
        }

        public RegisteredScriptType ScriptType
        {
            get
            {
                return _scriptType;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods",
            Justification = "Refers to a Control, not my Object.GetType()")]
        public Type Type
        {
            get
            {
                return _type;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Consistent with RegisterClientScriptInclude.")]
        public string Url
        {
            get
            {
                // null if this is not a client script include or resource
                return _url;
            }
        }
    }
}
