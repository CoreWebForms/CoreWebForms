// Decompiled with JetBrains decompiler
// Type: System.Web.Profile.ProfileBase
// Assembly: System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: BA21739E-A589-408A-8887-2FD9F117EAFB
// Assembly location: C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Web.dll
// XML documentation location: C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Web.xml

using System.CodeDom;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Provider;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.UI;

#nullable disable
namespace System.Web.Profile
{
  /// <summary>Provides untyped access to profile property values and information.</summary>
  public class ProfileBase : SettingsBase
  {
    private Hashtable _Groups = new Hashtable();
    private bool _IsAuthenticated;
    private string _UserName;
    private bool _IsDirty;
    private DateTime _LastActivityDate;
    private DateTime _LastUpdatedDate;
    private bool _DatesRetrieved;
    private static SettingsPropertyCollection s_Properties = (SettingsPropertyCollection) null;
    private static object s_InitializeLock = new object();
    private static Exception s_InitializeException = (Exception) null;
    private static bool s_Initialized = false;
    private static ProfileBase s_SingletonInstance = (ProfileBase) null;
    private static Hashtable s_PropertiesForCompilation = (Hashtable) null;

    /// <summary>Gets or sets a profile property value indexed by the property name.</summary>
    /// <param name="propertyName">The name of the profile property.</param>
    /// <returns>The value of the specified profile property, typed as <see langword="object" />.</returns>
    /// <exception cref="T:System.Configuration.Provider.ProviderException">An attempt was made to set a property value on an anonymous profile where the property's <see langword="allowAnonymous" /> attribute is <see langword="false" />.</exception>
    /// <exception cref="T:System.Configuration.SettingsPropertyNotFoundException">There are no properties defined for the current profile.-or-The specified profile property name does not exist in the current profile.-or-The provider for the specified profile property did not recognize the specified property.</exception>
    /// <exception cref="T:System.Configuration.SettingsPropertyIsReadOnlyException">An attempt was made to set a property value that was marked as read-only.</exception>
    /// <exception cref="T:System.Configuration.SettingsPropertyWrongTypeException">An attempt was made to assign a value to a property using an incompatible type.</exception>
    public override object this[string propertyName]
    {
      get
      {
        // if (!HttpRuntime.DisableProcessRequestInApplicationTrust && HttpRuntime.NamedPermissionSet != null && HttpRuntime.ProcessRequestInApplicationTrust)
        //    HttpRuntime.NamedPermissionSet.PermitOnly();
        return this.GetInternal(propertyName);
      }
      set
      {
        // if (!HttpRuntime.DisableProcessRequestInApplicationTrust && HttpRuntime.NamedPermissionSet != null && HttpRuntime.ProcessRequestInApplicationTrust)
        //   HttpRuntime.NamedPermissionSet.PermitOnly();
        this.SetInternal(propertyName, value);
      }
    }

    [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
    private object GetInternal(string propertyName) => base[propertyName];

    [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
    private void SetInternal(string propertyName, object value)
    {
      if (!this._IsAuthenticated)
      {
        SettingsProperty property = s_Properties[propertyName];
        if (property != null && !(bool) property.Attributes[(object) "AllowAnonymous"])
          throw new ProviderException(SR.GetString("Profile_anonoymous_not_allowed_to_set_property"));
      }
      base[propertyName] = value;
    }

    /// <summary>Gets the value of a profile property.</summary>
    /// <param name="propertyName">The name of the profile property.</param>
    /// <returns>The value of the specified profile property, typed as <see langword="object" />.</returns>
    /// <exception cref="T:System.Configuration.Provider.ProviderException">An attempt was made to set a property value on an anonymous profile where the property's <see langword="allowAnonymous" /> attribute is <see langword="false" />.</exception>
    /// <exception cref="T:System.Configuration.SettingsPropertyNotFoundException">There are no properties defined for the current profile.-or-The specified profile property name does not exist in the current profile.-or-The provider for the specified profile property did not recognize the specified property.</exception>
    public object GetPropertyValue(string propertyName) => this[propertyName];

    /// <summary>Sets the value of a profile property.</summary>
    /// <param name="propertyName">The name of the property to set.</param>
    /// <param name="propertyValue">The value to assign to the property.</param>
    /// <exception cref="T:System.Configuration.Provider.ProviderException">An attempt was made to set a property value on an anonymous profile where the property's <see langword="allowAnonymous" /> attribute is <see langword="false" />.</exception>
    /// <exception cref="T:System.Configuration.SettingsPropertyNotFoundException">There are no properties defined for the current profile.-or-The specified profile property name does not exist in the current profile.-or-The provider for the specified profile property did not recognize the specified property.</exception>
    /// <exception cref="T:System.Configuration.SettingsPropertyIsReadOnlyException">An attempt was made to set a value value on a property that was marked as read-only.</exception>
    /// <exception cref="T:System.Configuration.SettingsPropertyWrongTypeException">An attempt was made to assign a value to a property using an incompatible type.</exception>
    public void SetPropertyValue(string propertyName, object propertyValue)
    {
      this[propertyName] = propertyValue;
    }

    /// <summary>Gets a group of properties identified by a group name.</summary>
    /// <param name="groupName">The name of the group of properties.</param>
    /// <returns>A <see cref="T:System.Web.Profile.ProfileGroupBase" /> object for a group of properties configured with the specified group name.</returns>
    /// <exception cref="T:System.Configuration.Provider.ProviderException">The specified profile property group name was not found in the  configuration section.</exception>
    public ProfileGroupBase GetProfileGroup(string groupName)
    {
      ProfileGroupBase profileGroup = (ProfileGroupBase) this._Groups[(object) groupName];
      if (profileGroup == null)
      {
        Type profileType = BuildManager.GetProfileType();
        if (profileType == (Type) null)
          throw new ProviderException(SR.GetString("Profile_group_not_found", (object) groupName));
        Type type = profileType.Assembly.GetType("ProfileGroup" + groupName, false);
        profileGroup = !(type == (Type) null) ? (ProfileGroupBase) Activator.CreateInstance(type) : throw new ProviderException(SR.GetString("Profile_group_not_found", (object) groupName));
        profileGroup.Init(this, groupName);
      }
      return profileGroup;
    }

    /// <summary>Creates an instance of the <see cref="T:System.Web.Profile.ProfileBase" /> class.</summary>
    /// <exception cref="T:System.Configuration.Provider.ProviderException">The <see langword="enabled" /> attribute of the  section of the Web.config file is <see langword="false" />.</exception>
    /// <exception cref="T:System.Configuration.ConfigurationErrorsException">A property type specified in the  section of the Web.config file could not be created.-or-The <see langword="allowAnonymous" /> attribute for a property in the  section of the Web.config file is set to <see langword="true" /> and the <see langword="enabled" /> attribute of the  element is set to <see langword="false" />.-or-The <see langword="serializeAs" /> attribute for a property in the  section of the Web.config file is set to <see cref="F:System.Configuration.SettingsSerializeAs.Binary" /> and the <see cref="P:System.Type.IsSerializable" /> property of the specified <see langword="type" /> returns <see langword="false" />.-or-The name of a provider specified using the <see langword="provider" /> attribute of a profile property could not be found in the <see cref="P:System.Web.Profile.ProfileManager.Providers" /> collection.-or-The <see langword="type" /> specified for a profile property could not be found.-or-A profile property was specified with a name that matches a property name on the base class specified in the <see langword="inherits" /> attribute of the  section.</exception>
    public ProfileBase()
    {
      if (!ProfileManager.Enabled)
        throw new ProviderException(SR.GetString("Profile_not_enabled"));
      if (s_Initialized)
        return;
      InitializeStatic();
    }

    /// <summary>Initializes the profile property values and information for the current user.</summary>
    /// <param name="username">The name of the user to initialize the profile for.</param>
    /// <param name="isAuthenticated">
    /// <see langword="true" /> to indicate the user is authenticated; <see langword="false" /> to indicate the user is anonymous.</param>
    public void Initialize(string username, bool isAuthenticated)
    {
      this._UserName = username == null ? username : username.Trim();
      SettingsContext context = new SettingsContext();
      context.Add((object) "UserName", (object) this._UserName);
      context.Add((object) "IsAuthenticated", (object) isAuthenticated);
      this._IsAuthenticated = isAuthenticated;
      this.Initialize(context, s_Properties, (SettingsProviderCollection) ProfileManager.Providers);
    }

    /// <summary>Updates the profile data source with changed profile property values.</summary>
    public override void Save()
    {
      // if (!HttpRuntime.DisableProcessRequestInApplicationTrust && HttpRuntime.NamedPermissionSet != null && HttpRuntime.ProcessRequestInApplicationTrust)
      //   HttpRuntime.NamedPermissionSet.PermitOnly();
      this.SaveWithAssert();
    }

    [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
    private void SaveWithAssert()
    {
      base.Save();
      this._IsDirty = false;
      this._DatesRetrieved = false;
    }

    /// <summary>Gets the user name for the profile.</summary>
    /// <returns>The user name for the profile or the anonymous-user identifier assigned to the profile.</returns>
    public string UserName => this._UserName;

    /// <summary>Gets a value indicating whether the user profile is for an anonymous user.</summary>
    /// <returns>
    /// <see langword="true" /> if the user profile is for an anonymous user; otherwise, <see langword="false" />.</returns>
    public bool IsAnonymous => !this._IsAuthenticated;

    /// <summary>Gets a value indicating whether any of the profile properties have been modified.</summary>
    /// <returns>
    /// <see langword="true" /> if any of the profile properties have been modified; otherwise, <see langword="false" />.</returns>
    public bool IsDirty
    {
      get
      {
        if (this._IsDirty)
          return true;
        foreach (SettingsPropertyValue propertyValue in this.PropertyValues)
        {
          if (propertyValue.IsDirty)
          {
            this._IsDirty = true;
            return true;
          }
        }
        return false;
      }
    }

    /// <summary>Gets the most recent date and time that the profile was read or modified.</summary>
    /// <returns>The most recent date and time that the profile was read or modified by the default provider.</returns>
    public DateTime LastActivityDate
    {
      get
      {
        if (!this._DatesRetrieved)
          this.RetrieveDates();
        return this._LastActivityDate.ToLocalTime();
      }
    }

    /// <summary>Gets the most recent date and time that the profile was modified.</summary>
    /// <returns>The most recent date and time that the profile was modified by the default provider.</returns>
    public DateTime LastUpdatedDate
    {
      get
      {
        if (!this._DatesRetrieved)
          this.RetrieveDates();
        return this._LastUpdatedDate.ToLocalTime();
      }
    }

    /// <summary>Used by ASP.NET to create an instance of a profile for the specified user name.</summary>
    /// <param name="username">The name of the user to create a profile for.</param>
    /// <returns>An <see cref="T:System.Web.Profile.ProfileBase" /> that represents the profile for the specified user.</returns>
    /// <exception cref="T:System.Configuration.Provider.ProviderException">The <see langword="enabled" /> attribute of the  section of the Web.config file is <see langword="false" />.</exception>
    /// <exception cref="T:System.Web.HttpException">The current hosting permission level is less than <see cref="F:System.Web.AspNetHostingPermissionLevel.Medium" />.</exception>
    /// <exception cref="T:System.Configuration.ConfigurationErrorsException">A property type specified in the  section of the Web.config file could not be created.-or-The <see langword="allowAnonymous" /> attribute for a property in the  section of the Web.config file is set to <see langword="true" /> and the <see langword="enabled" /> attribute of the  element is set to <see langword="false" />.-or-The <see langword="serializeAs" /> attribute for a property in the  section of the Web.config file is set to <see cref="F:System.Configuration.SettingsSerializeAs.Binary" /> and the <see cref="P:System.Type.IsSerializable" /> property of the specified <see langword="type" /> returns <see langword="false" />.-or-The name of a provider specified using the <see langword="provider" /> attribute of a profile property could not be found in the <see cref="P:System.Web.Profile.ProfileManager.Providers" /> collection.-or-The <see langword="type" /> specified for a profile property could not be found.-or-A profile property was specified with a name that matches a property name on the base class specified in the <see langword="inherits" /> attribute of the  section.</exception>
    public static ProfileBase Create(string username) => Create(username, true);

    /// <summary>Used by ASP.NET to create an instance of a profile for the specified user name. Takes a parameter indicating whether the user is authenticated or anonymous.</summary>
    /// <param name="username">The name of the user to create a profile for.</param>
    /// <param name="isAuthenticated">
    /// <see langword="true" /> to indicate the user is authenticated; <see langword="false" /> to indicate the user is anonymous.</param>
    /// <returns>A <see cref="T:System.Web.Profile.ProfileBase" /> object that represents the profile for the specified user.</returns>
    /// <exception cref="T:System.Configuration.Provider.ProviderException">The <see langword="enabled" /> attribute of the  section of the Web.config file is <see langword="false" />.</exception>
    /// <exception cref="T:System.Web.HttpException">The current hosting permission level is less than <see cref="F:System.Web.AspNetHostingPermissionLevel.Medium" />.</exception>
    /// <exception cref="T:System.Configuration.ConfigurationErrorsException">A property type specified in the  section of the Web.config file could not be created.-or-The <see langword="allowAnonymous" /> attribute for a property in the  section of the Web.config file is set to <see langword="true" /> and the <see langword="enabled" /> attribute of the  element is set to <see langword="false" />.-or-The <see langword="serializeAs" /> attribute for a property in the  section of the Web.config file is set to <see cref="F:System.Configuration.SettingsSerializeAs.Binary" /> and the <see cref="P:System.Type.IsSerializable" /> property of the specified <see langword="type" /> returns <see langword="false" />.-or-The name of a provider specified using the <see langword="provider" /> attribute of a profile property could not be found in the <see cref="P:System.Web.Profile.ProfileManager.Providers" /> collection.-or-The <see langword="type" /> specified for a profile property could not be found.-or-A profile property was specified with a name that matches a property name on the base class specified in the <see langword="inherits" /> attribute of the  section.</exception>
    public static ProfileBase Create(string username, bool isAuthenticated)
    {
      if (!ProfileManager.Enabled)
        throw new ProviderException(SR.GetString("Profile_not_enabled"));
      InitializeStatic();
      if (s_SingletonInstance != null)
        return s_SingletonInstance;
      if (s_Properties.Count == 0)
      {
        lock (s_InitializeLock)
        {
          if (s_SingletonInstance == null)
            s_SingletonInstance = (ProfileBase) new DefaultProfile();
          return s_SingletonInstance;
        }
      }
      else
      {
        //HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, "Feature_not_supported_at_this_level");
        return CreateMyInstance(username, isAuthenticated);
      }
    }

    /// <summary>Gets a collection of <see cref="T:System.Configuration.SettingsProperty" /> objects for each property in the profile.</summary>
    /// <returns>A <see cref="T:System.Configuration.SettingsPropertyCollection" /> of <see cref="T:System.Configuration.SettingsProperty" /> objects for each property in the profile for the application.</returns>
    /// <exception cref="T:System.Configuration.ConfigurationErrorsException">A property type specified in the  section of the Web.config file could not be created.-or-The <see langword="allowAnonymous" /> attribute for a property in the  section of the Web.config file is set to <see langword="true" /> and the <see langword="enabled" /> attribute of the  element is set to <see langword="false" />.-or-The <see langword="serializeAs" /> attribute for a property in the  section of the Web.config file is set to <see cref="F:System.Configuration.SettingsSerializeAs.Binary" /> and the <see cref="P:System.Type.IsSerializable" /> property of the specified <see langword="type" /> returns <see langword="false" />.-or-The name of a provider specified using the <see langword="provider" /> attribute of a profile property could not be found in the <see cref="P:System.Web.Profile.ProfileManager.Providers" /> collection.-or-The <see langword="type" /> specified for a profile property could not be found.-or-A profile property was specified with a name that matches a property name on the base class specified in the <see langword="inherits" /> attribute of the  section.</exception>
    public new static SettingsPropertyCollection Properties
    {
      get
      {
        InitializeStatic();
        return s_Properties;
      }
    }

    internal static Type InheritsFromType
    {
      get
      {
        if (!ProfileManager.Enabled)
          return typeof (DefaultProfile);
        Type c = !HostingEnvironment.IsHosted ? GetPropType(InheritsFromTypeString) : BuildManager.GetType(InheritsFromTypeString, true, true);
        if (!typeof (ProfileBase).IsAssignableFrom(c))
        {
          ProfileSection profileAppConfig = MTConfigUtil.GetProfileAppConfig();
          throw new ConfigurationErrorsException(SR.GetString("Wrong_profile_base_type"), (Exception) null, profileAppConfig.ElementInformation.Properties["inherits"].Source, profileAppConfig.ElementInformation.Properties["inherit"].LineNumber);
        }
        return c;
      }
    }

    internal static string InheritsFromTypeString
    {
      get
      {
        string inheritsFromTypeString = typeof (ProfileBase).ToString();
        if (!ProfileManager.Enabled)
          return inheritsFromTypeString;
        ProfileSection profileAppConfig = MTConfigUtil.GetProfileAppConfig();
        if (profileAppConfig.Inherits == null)
          return inheritsFromTypeString;
        string typeName = profileAppConfig.Inherits.Trim();
        if (typeName.Length < 1)
          return inheritsFromTypeString;
        Type type = Type.GetType(typeName, false, true);
        if (type == (Type) null)
          return typeName;
        return typeof (ProfileBase).IsAssignableFrom(type) ? type.AssemblyQualifiedName : throw new ConfigurationErrorsException(SR.GetString("Wrong_profile_base_type"), (Exception) null, profileAppConfig.ElementInformation.Properties["inherits"].Source, profileAppConfig.ElementInformation.Properties["inherit"].LineNumber);
      }
    }

    internal static bool InheritsFromCustomType
    {
      get
      {
        if (!ProfileManager.Enabled)
          return false;
        ProfileSection profileAppConfig = MTConfigUtil.GetProfileAppConfig();
        if (profileAppConfig.Inherits == null)
          return false;
        string typeName = profileAppConfig.Inherits.Trim();
        if (typeName == null || typeName.Length < 1)
          return false;
        Type type = Type.GetType(typeName, false, true);
        return type == (Type) null || type != typeof (ProfileBase);
      }
    }

    internal static ProfileBase SingletonInstance => s_SingletonInstance;

    internal static Hashtable GetPropertiesForCompilation()
    {
      if (!ProfileManager.Enabled)
        return (Hashtable) null;
      if (s_PropertiesForCompilation != null)
        return s_PropertiesForCompilation;
      lock (s_InitializeLock)
      {
        if (s_PropertiesForCompilation != null)
          return s_PropertiesForCompilation;
        Hashtable ht = new Hashtable();
        ProfileSection profileAppConfig = MTConfigUtil.GetProfileAppConfig();
        if (profileAppConfig.PropertySettings == null)
        {
          s_PropertiesForCompilation = ht;
          return s_PropertiesForCompilation;
        }
        AddProfilePropertySettingsForCompilation((ProfilePropertySettingsCollection) profileAppConfig.PropertySettings, ht, (string) null);
        foreach (ProfileGroupSettings groupSetting in (ConfigurationElementCollection) profileAppConfig.PropertySettings.GroupSettings)
          AddProfilePropertySettingsForCompilation(groupSetting.PropertySettings, ht, groupSetting.Name);
        ProfileBase.AddProfilePropertySettingsForCompilation(ProfileManager.DynamicProfileProperties, ht, (string) null);
        s_PropertiesForCompilation = ht;
      }
      return s_PropertiesForCompilation;
    }

    internal static string GetProfileClassName()
    {
      Hashtable propertiesForCompilation = GetPropertiesForCompilation();
      return propertiesForCompilation == null || propertiesForCompilation.Count <= 0 && !InheritsFromCustomType ? "System.Web.Profile.DefaultProfile" : "ProfileCommon";
    }

    private static void AddProfilePropertySettingsForCompilation(
      ProfilePropertySettingsCollection propertyCollection,
      Hashtable ht,
      string groupName)
    {
      foreach (ProfilePropertySettings property in (ConfigurationElementCollection) propertyCollection)
      {
        ProfileNameTypeStruct profileNameTypeStruct = new ProfileNameTypeStruct();
        profileNameTypeStruct.Name = groupName == null ? property.Name : groupName + "." + property.Name;
        Type type = property.TypeInternal;
        if (type == (Type) null)
          type = ResolvePropertyTypeForCommonTypes(property.Type.ToLower(CultureInfo.InvariantCulture));
        if (type == (Type) null)
          type = BuildManager.GetType(property.Type, false);
        profileNameTypeStruct.PropertyCodeRefType = !(type == (Type) null) ? new CodeTypeReference(type) : new CodeTypeReference(property.Type);
        profileNameTypeStruct.PropertyType = type;
        property.TypeInternal = type;
        profileNameTypeStruct.IsReadOnly = property.ReadOnly;
        profileNameTypeStruct.LineNumber = property.ElementInformation.Properties["name"].LineNumber;
        profileNameTypeStruct.FileName = property.ElementInformation.Properties["name"].Source;
        ht.Add((object) profileNameTypeStruct.Name, (object) profileNameTypeStruct);
      }
    }

    private static ProfileBase CreateMyInstance(string username, bool isAuthenticated)
    {
      ProfileBase instance = (ProfileBase) Activator.CreateInstance(!HostingEnvironment.IsHosted ? InheritsFromType : BuildManager.GetProfileType());
      instance.Initialize(username, isAuthenticated);
      return instance;
    }

    private static void InitializeStatic()
    {
      if (!ProfileManager.Enabled || s_Initialized)
      {
        if (s_InitializeException != null)
          throw s_InitializeException;
      }
      else
      {
        lock (s_InitializeLock)
        {
          if (s_Initialized)
          {
            if (s_InitializeException == null)
              return;
            throw s_InitializeException;
          }
          try
          {
            ProfileSection profileAppConfig = MTConfigUtil.GetProfileAppConfig();
            bool fAnonEnabled = !HostingEnvironment.IsHosted;//|| AnonymousIdentificationModule.Enabled;
            Type inheritsFromType = InheritsFromType;
            bool hasLowTrust = false;//HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Low);
            s_Properties = new SettingsPropertyCollection();
            ProfileBase.AddPropertySettingsFromConfig(inheritsFromType, fAnonEnabled, hasLowTrust, ProfileManager.DynamicProfileProperties, (string) null);
            if (inheritsFromType != typeof (ProfileBase))
            {
              PropertyInfo[] properties = typeof (ProfileBase).GetProperties();
              NameValueCollection nameValueCollection = new NameValueCollection(properties.Length);
              foreach (PropertyInfo propertyInfo in properties)
                nameValueCollection.Add(propertyInfo.Name, string.Empty);
              foreach (PropertyInfo property1 in inheritsFromType.GetProperties())
              {
                if (nameValueCollection[property1.Name] == null)
                {
                  ProfileProvider provider = hasLowTrust ? ProfileManager.Provider : (ProfileProvider) null;
                  bool isReadOnly = false;
                  SettingsSerializeAs serializeAs = SettingsSerializeAs.ProviderSpecific;
                  string empty = string.Empty;
                  bool flag = false;
                  string str = (string) null;
                  foreach (Attribute customAttribute in Attribute.GetCustomAttributes((MemberInfo) property1, true))
                  {
                    switch (customAttribute)
                    {
                      case SettingsSerializeAsAttribute _:
                        serializeAs = ((SettingsSerializeAsAttribute) customAttribute).SerializeAs;
                        break;
                      case SettingsAllowAnonymousAttribute _:
                        flag = ((SettingsAllowAnonymousAttribute) customAttribute).Allow;
                        if (!fAnonEnabled & flag)
                          throw new ConfigurationErrorsException(SR.GetString("Annoymous_id_module_not_enabled", (object) property1.Name), profileAppConfig.ElementInformation.Properties["inherits"].Source, profileAppConfig.ElementInformation.Properties["inherits"].LineNumber);
                        break;
                      case ReadOnlyAttribute _:
                        isReadOnly = ((ReadOnlyAttribute) customAttribute).IsReadOnly;
                        break;
                      case DefaultSettingValueAttribute _:
                        empty = ((DefaultSettingValueAttribute) customAttribute).Value;
                        break;
                      case CustomProviderDataAttribute _:
                        str = ((CustomProviderDataAttribute) customAttribute).CustomProviderData;
                        break;
                      default:
                        if (hasLowTrust && customAttribute is ProfileProviderAttribute)
                        {
                          provider = ProfileManager.Providers[((ProfileProviderAttribute) customAttribute).ProviderName];
                          if (provider == null)
                            throw new ConfigurationErrorsException(SR.GetString("Profile_provider_not_found", (object) ((ProfileProviderAttribute) customAttribute).ProviderName), profileAppConfig.ElementInformation.Properties["inherits"].Source, profileAppConfig.ElementInformation.Properties["inherits"].LineNumber);
                          break;
                        }
                        break;
                    }
                  }
                  SettingsAttributeDictionary attributes = new SettingsAttributeDictionary();
                  attributes.Add((object) "AllowAnonymous", (object) flag);
                  if (!string.IsNullOrEmpty(str))
                    attributes.Add((object) "CustomProviderData", (object) str);
                  SettingsProperty property2 = new SettingsProperty(property1.Name, property1.PropertyType, (SettingsProvider) provider, isReadOnly, (object) empty, serializeAs, attributes, false, true);
                  s_Properties.Add(property2);
                }
              }
            }
            if (profileAppConfig.PropertySettings != null)
            {
              AddPropertySettingsFromConfig(inheritsFromType, fAnonEnabled, hasLowTrust, (ProfilePropertySettingsCollection) profileAppConfig.PropertySettings, (string) null);
              foreach (ProfileGroupSettings groupSetting in (ConfigurationElementCollection) profileAppConfig.PropertySettings.GroupSettings)
                AddPropertySettingsFromConfig(inheritsFromType, fAnonEnabled, hasLowTrust, groupSetting.PropertySettings, groupSetting.Name);
            }
          }
          catch (Exception ex)
          {
            if (s_InitializeException == null)
              s_InitializeException = ex;
          }
          if (s_Properties == null)
            s_Properties = new SettingsPropertyCollection();
          s_Properties.SetReadOnly();
          s_Initialized = true;
        }
        if (s_InitializeException != null)
          throw s_InitializeException;
      }
    }

    private static void AddPropertySettingsFromConfig(
      Type baseType,
      bool fAnonEnabled,
      bool hasLowTrust,
      ProfilePropertySettingsCollection settingsCollection,
      string groupName)
    {
      foreach (ProfilePropertySettings settings in (ConfigurationElementCollection) settingsCollection)
      {
        string name = groupName != null ? groupName + "." + settings.Name : settings.Name;
        if (baseType != typeof (ProfileBase) && s_Properties[name] != null)
          throw new ConfigurationErrorsException(SR.GetString("Profile_property_already_added"), (Exception) null, settings.ElementInformation.Properties["name"].Source, settings.ElementInformation.Properties["name"].LineNumber);
        try
        {
          if (settings.TypeInternal == (Type) null)
            settings.TypeInternal = ResolvePropertyType(settings.Type);
        }
        catch (Exception ex)
        {
          throw new ConfigurationErrorsException(SR.GetString("Profile_could_not_create_type", (object) ex.Message), ex, settings.ElementInformation.Properties["type"].Source, settings.ElementInformation.Properties["type"].LineNumber);
        }
        if (!fAnonEnabled && settings.AllowAnonymous)
          throw new ConfigurationErrorsException(SR.GetString("Annoymous_id_module_not_enabled", (object) settings.Name), settings.ElementInformation.Properties["allowAnonymous"].Source, settings.ElementInformation.Properties["allowAnonymous"].LineNumber);
        if (hasLowTrust)
          SetProviderForProperty(settings);
        else
          settings.ProviderInternal = (SettingsProvider) null;
        if ((settings.ProviderInternal == null || settings.ProviderInternal.GetType() == typeof (SqlProfileProvider)) && settings.SerializeAs == SerializationMode.Binary && !settings.TypeInternal.IsSerializable)
          throw new ConfigurationErrorsException(SR.GetString("Property_not_serializable", (object) settings.Name), settings.ElementInformation.Properties["serializeAs"].Source, settings.ElementInformation.Properties["serializeAs"].LineNumber);
        SettingsAttributeDictionary attributes = new SettingsAttributeDictionary();
        attributes.Add((object) "AllowAnonymous", (object) settings.AllowAnonymous);
        if (!string.IsNullOrEmpty(settings.CustomProviderData))
          attributes.Add((object) "CustomProviderData", (object) settings.CustomProviderData);
        SettingsProperty property = new SettingsProperty(name, settings.TypeInternal, settings.ProviderInternal, settings.ReadOnly, (object) settings.DefaultValue, (SettingsSerializeAs) settings.SerializeAs, attributes, false, true);
        s_Properties.Add(property);
      }
    }

    private static void SetProviderForProperty(ProfilePropertySettings pps)
    {
      pps.ProviderInternal = pps.Provider == null || pps.Provider.Length < 1 ? (SettingsProvider) ProfileManager.Provider : (SettingsProvider) ProfileManager.Providers[pps.Provider];
      if (pps.ProviderInternal == null)
        throw new ConfigurationErrorsException(SR.GetString("Profile_provider_not_found", (object) pps.Provider), pps.ElementInformation.Properties["provider"].Source, pps.ElementInformation.Properties["provider"].LineNumber);
    }

    private static Type ResolvePropertyTypeForCommonTypes(string typeName)
    {
      switch (typeName)
      {
        case "bool":
        case "boolean":
          return typeof (bool);
        case "byte":
        case "int8":
          return typeof (byte);
        case "char":
          return typeof (char);
        case "date":
        case "datetime":
          return typeof (DateTime);
        case "decimal":
          return typeof (Decimal);
        case "double":
        case "float64":
          return typeof (double);
        case "float":
        case "float32":
          return typeof (float);
        case "int":
        case "int32":
        case "integer":
          return typeof (int);
        case "int16":
        case "short":
          return typeof (short);
        case "int64":
        case "long":
          return typeof (long);
        case "object":
          return typeof (object);
        case "single":
          return typeof (float);
        case "string":
          return typeof (string);
        case "uint":
        case "uint32":
          return typeof (uint);
        case "uint16":
        case "ushort":
          return typeof (ushort);
        case "uint64":
        case "ulong":
          return typeof (ulong);
        default:
          return (Type) null;
      }
    }

    private static Type ResolvePropertyType(string typeName)
    {
      Type type = ResolvePropertyTypeForCommonTypes(typeName.ToLower(CultureInfo.InvariantCulture));
      if (type != (Type) null)
        return type;
      return HostingEnvironment.IsHosted ? BuildManager.GetType(typeName, true, true) : GetPropType(typeName);
    }

    private static Type GetPropType(string typeName) => Type.GetType(typeName, true, true);

    private void RetrieveDates()
    {
      if (this._DatesRetrieved || ProfileManager.Provider == null)
        return;
      IEnumerator enumerator = ProfileManager.Provider.FindProfilesByUserName(ProfileAuthenticationOption.All, this._UserName, 0, 1, out int _).GetEnumerator();
      try
      {
        if (!enumerator.MoveNext())
          return;
        ProfileInfo current = (ProfileInfo) enumerator.Current;
        this._LastActivityDate = current.LastActivityDate.ToUniversalTime();
        this._LastUpdatedDate = current.LastUpdatedDate.ToUniversalTime();
        this._DatesRetrieved = true;
      }
      finally
      {
        if (enumerator is IDisposable disposable)
          disposable.Dispose();
      }
    }
  }
}
