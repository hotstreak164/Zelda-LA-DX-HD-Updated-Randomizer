using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using LADXHD_Launcher.Properties;

namespace LADXHD_Launcher
{
    public static class ResourceHelper
    {
        public static Dictionary<string, object> GetAllResources()
        {
            Dictionary<string, object> resources = new Dictionary<string, object>();

            ResourceSet resourceSet = Resources.ResourceManager.GetResourceSet
            (
                CultureInfo.CurrentUICulture,
                true,
                true
            );
            foreach (DictionaryEntry entry in resourceSet)
            {
                resources[(string)entry.Key] = entry.Value;
            }
            return resources;
        }

        public static T GetResource<T>(string name)
        {
            object resource = Resources.ResourceManager.GetObject(name);
            if (resource is T typed)
            {
                return typed;
            }
            throw new InvalidOperationException
            (
                $"Resource '{name}' is not of type {typeof(T).Name}."
            );
        }
    }
}