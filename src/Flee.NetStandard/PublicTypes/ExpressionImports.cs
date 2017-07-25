using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Flee.InternalTypes;
using Flee.PublicTypes;
using Flee.Resources;

namespace Flee.PublicTypes
{
    public sealed class ExpressionImports
    {

        private static Dictionary<string, Type> OurBuiltinTypeMap = CreateBuiltinTypeMap();
        private NamespaceImport MyRootImport;
        private TypeImport MyOwnerImport;

        private ExpressionContext MyContext;
        internal ExpressionImports()
        {
            MyRootImport = new NamespaceImport("true");
        }

        private static Dictionary<string, Type> CreateBuiltinTypeMap()
        {
            Dictionary<string, Type> map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            map.Add("boolean", typeof(bool));
            map.Add("byte", typeof(byte));
            map.Add("sbyte", typeof(sbyte));
            map.Add("short", typeof(short));
            map.Add("ushort", typeof(UInt16));
            map.Add("int", typeof(Int32));
            map.Add("uint", typeof(UInt32));
            map.Add("long", typeof(long));
            map.Add("ulong", typeof(ulong));
            map.Add("single", typeof(float));
            map.Add("double", typeof(double));
            map.Add("decimal", typeof(decimal));
            map.Add("char", typeof(char));
            map.Add("object", typeof(object));
            map.Add("string", typeof(string));

            return map;
        }

        #region "Methods - Non public"
        internal void SetContext(ExpressionContext context)
        {
            MyContext = context;
            MyRootImport.SetContext(context);
        }

        internal ExpressionImports Clone()
        {
            ExpressionImports copy = new ExpressionImports();

            copy.MyRootImport = (NamespaceImport)MyRootImport.Clone();
            copy.MyOwnerImport = MyOwnerImport;

            return copy;
        }

        internal void ImportOwner(Type ownerType)
        {
            MyOwnerImport = new TypeImport(ownerType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, false);
            MyOwnerImport.SetContext(MyContext);
        }

        internal bool HasNamespace(string ns)
        {
            NamespaceImport import = MyRootImport.FindImport(ns) as NamespaceImport;
            return (import != null);
        }

        internal NamespaceImport GetImport(string ns)
        {
            if (ns.Length == 0)
            {
                return MyRootImport;
            }

            NamespaceImport import = MyRootImport.FindImport(ns) as NamespaceImport;

            if (import == null)
            {
                import = new NamespaceImport(ns);
                MyRootImport.Add(import);
            }

            return import;
        }

        internal MemberInfo[] FindOwnerMembers(string memberName, System.Reflection.MemberTypes memberType)
        {
            return MyOwnerImport.FindMembers(memberName, memberType);
        }

        internal Type FindType(string[] typeNameParts)
        {
            string[] namespaces = new string[typeNameParts.Length - 1];
            string typeName = typeNameParts[typeNameParts.Length - 1];

            System.Array.Copy(typeNameParts, namespaces, namespaces.Length);
            ImportBase currentImport = MyRootImport;

            foreach (string ns in namespaces)
            {
                currentImport = currentImport.FindImport(ns);
                if (currentImport == null)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }

            return currentImport?.FindType(typeName);
        }

        static internal Type GetBuiltinType(string name)
        {
            Type t = null;

            if (OurBuiltinTypeMap.TryGetValue(name, out t) == true)
            {
                return t;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region "Methods - Public"
        public void AddType(Type t, string ns)
        {
            Utility.AssertNotNull(t, "t");
            Utility.AssertNotNull(ns, "namespace");

            MyContext.AssertTypeIsAccessible(t);

            NamespaceImport import = this.GetImport(ns);
            import.Add(new TypeImport(t, BindingFlags.Public | BindingFlags.Static, false));
        }

        public void AddType(Type t)
        {
            this.AddType(t, string.Empty);
        }

        public void AddMethod(string methodName, Type t, string ns)
        {
            Utility.AssertNotNull(methodName, "methodName");
            Utility.AssertNotNull(t, "t");
            Utility.AssertNotNull(ns, "namespace");

            MethodInfo mi = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);

            if (mi == null)
            {
                string msg = Utility.GetGeneralErrorMessage(GeneralErrorResourceKeys.CouldNotFindPublicStaticMethodOnType, methodName, t.Name);
                throw new ArgumentException(msg);
            }

            this.AddMethod(mi, ns);
        }

        public void AddMethod(MethodInfo mi, string ns)
        {
            Utility.AssertNotNull(mi, "mi");
            Utility.AssertNotNull(ns, "namespace");

            MyContext.AssertTypeIsAccessible(mi.ReflectedType);

            if (mi.IsStatic == false | mi.IsPublic == false)
            {
                string msg = Utility.GetGeneralErrorMessage(GeneralErrorResourceKeys.OnlyPublicStaticMethodsCanBeImported);
                throw new ArgumentException(msg);
            }

            NamespaceImport import = this.GetImport(ns);
            import.Add(new MethodImport(mi));
        }

        public void ImportBuiltinTypes()
        {
            foreach (KeyValuePair<string, Type> pair in OurBuiltinTypeMap)
            {
                this.AddType(pair.Value, pair.Key);
            }
        }
        #endregion

        #region "Properties - Public"
        public NamespaceImport RootImport => MyRootImport;

        #endregion
    }
}
