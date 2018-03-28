using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;

namespace Flee.Resources
{
    internal class FleeResourceManager
    {

        private Dictionary<string, ResourceManager> MyResourceManagers;

        private static FleeResourceManager OurInstance = new FleeResourceManager();
        private FleeResourceManager()
        {
            MyResourceManagers = new Dictionary<string, ResourceManager>(StringComparer.OrdinalIgnoreCase);
        }

        private ResourceManager GetResourceManager(string resourceFile)
        {
            lock (this)
            {
                ResourceManager rm = null;
                if (MyResourceManagers.TryGetValue(resourceFile, out rm) == false)
                {
                    Type t = typeof(FleeResourceManager);
                    rm = new ResourceManager(string.Format("{0}.{1}", t.Namespace, resourceFile), t.Assembly);
                    MyResourceManagers.Add(resourceFile, rm);
                }
                return rm;
            }
        }

        private string GetResourceString(string resourceFile, string key)
        {
            ResourceManager rm = this.GetResourceManager(resourceFile);
            return rm.GetString(key);
        }

        public string GetCompileErrorString(string key)
        {
            return this.GetResourceString("CompileErrors", key);
        }

        public string GetElementNameString(string key)
        {
            return this.GetResourceString("ElementNames", key);
        }

        public string GetGeneralErrorString(string key)
        {
            return this.GetResourceString("GeneralErrors", key);
        }

        public static FleeResourceManager Instance
        {
            get { return OurInstance; }
        }
    }
}
