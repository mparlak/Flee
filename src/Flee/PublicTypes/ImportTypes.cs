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
    public abstract class ImportBase : IEnumerable<ImportBase>, IEquatable<ImportBase>
    {
        private ExpressionContext _myContext;

        internal ImportBase()
        {
        }

        #region "Methods - Non Public"
        internal virtual void SetContext(ExpressionContext context)
        {
            _myContext = context;
            this.Validate();
        }

        internal abstract void Validate();

        protected abstract void AddMembers(string memberName, MemberTypes memberType, ICollection<MemberInfo> dest);
        protected abstract void AddMembers(MemberTypes memberType, ICollection<MemberInfo> dest);

        internal ImportBase Clone()
        {
            return (ImportBase)this.MemberwiseClone();
        }

        protected static void AddImportMembers(ImportBase import, string memberName, MemberTypes memberType, ICollection<MemberInfo> dest)
        {
            import.AddMembers(memberName, memberType, dest);
        }

        protected static void AddImportMembers(ImportBase import, MemberTypes memberType, ICollection<MemberInfo> dest)
        {
            import.AddMembers(memberType, dest);
        }

        protected static void AddMemberRange(ICollection<MemberInfo> members, ICollection<MemberInfo> dest)
        {
            foreach (MemberInfo mi in members)
            {
                dest.Add(mi);
            }
        }

        protected bool AlwaysMemberFilter(MemberInfo member, object criteria)
        {
            return true;
        }

        internal abstract bool IsMatch(string name);
        internal abstract Type FindType(string typename);

        internal virtual ImportBase FindImport(string name)
        {
            return null;
        }

        internal MemberInfo[] FindMembers(string memberName, MemberTypes memberType)
        {
            List<MemberInfo> found = new List<MemberInfo>();
            this.AddMembers(memberName, memberType, found);
            return found.ToArray();
        }
        #endregion

        #region "Methods - Public"
        public MemberInfo[] GetMembers(MemberTypes memberType)
        {
            List<MemberInfo> found = new List<MemberInfo>();
            this.AddMembers(memberType, found);
            return found.ToArray();
        }
        #endregion

        #region "IEnumerable Implementation"
        public virtual System.Collections.Generic.IEnumerator<ImportBase> GetEnumerator()
        {
            List<ImportBase> coll = new List<ImportBase>();
            return coll.GetEnumerator();
        }

        private System.Collections.IEnumerator GetEnumerator1()
        {
            return this.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator1();
        }
        #endregion

        #region "IEquatable Implementation"
        public bool Equals(ImportBase other)
        {
            return this.EqualsInternal(other);
        }

        protected abstract bool EqualsInternal(ImportBase import);
        #endregion

        #region "Properties - Protected"
        protected ExpressionContext Context => _myContext;

        #endregion

        #region "Properties - Public"
        public abstract string Name { get; }

        public virtual bool IsContainer => false;

        #endregion
    }

    public sealed class TypeImport : ImportBase
    {
        private readonly Type _myType;
        private readonly BindingFlags _myBindFlags;
        private readonly bool _myUseTypeNameAsNamespace;
        public TypeImport(Type importType) : this(importType, false)
        {
        }

        public TypeImport(Type importType, bool useTypeNameAsNamespace) : this(importType, BindingFlags.Public | BindingFlags.Static, useTypeNameAsNamespace)
        {
        }

        #region "Methods - Non Public"
        internal TypeImport(Type t, BindingFlags flags, bool useTypeNameAsNamespace)
        {
            Utility.AssertNotNull(t, "t");
            _myType = t;
            _myBindFlags = flags;
            _myUseTypeNameAsNamespace = useTypeNameAsNamespace;
        }

        internal override void Validate()
        {
            this.Context.AssertTypeIsAccessible(_myType);
        }

        protected override void AddMembers(string memberName, MemberTypes memberType, ICollection<MemberInfo> dest)
        {
            MemberInfo[] members = _myType.FindMembers(memberType, _myBindFlags, this.Context.Options.MemberFilter, memberName);
            ImportBase.AddMemberRange(members, dest);
        }

        protected override void AddMembers(MemberTypes memberType, ICollection<MemberInfo> dest)
        {
            if (_myUseTypeNameAsNamespace == false)
            {
                MemberInfo[] members = _myType.FindMembers(memberType, _myBindFlags, this.AlwaysMemberFilter, null);
                ImportBase.AddMemberRange(members, dest);
            }
        }

        internal override bool IsMatch(string name)
        {
            if (_myUseTypeNameAsNamespace == true)
            {
                return string.Equals(_myType.Name, name, this.Context.Options.MemberStringComparison);
            }
            else
            {
                return false;
            }
        }

        internal override Type FindType(string typeName)
        {
            if (string.Equals(typeName, _myType.Name, this.Context.Options.MemberStringComparison) == true)
            {
                return _myType;
            }
            else
            {
                return null;
            }
        }

        protected override bool EqualsInternal(ImportBase import)
        {
            TypeImport otherSameType = import as TypeImport;
            return (otherSameType != null) && object.ReferenceEquals(_myType, otherSameType._myType);
        }
        #endregion

        #region "Methods - Public"
        public override IEnumerator<ImportBase> GetEnumerator()
        {
            if (_myUseTypeNameAsNamespace == true)
            {
                List<ImportBase> coll = new List<ImportBase>();
                coll.Add(new TypeImport(_myType, false));
                return coll.GetEnumerator();
            }
            else
            {
                return base.GetEnumerator();
            }
        }
        #endregion

        #region "Properties - Public"
        public override bool IsContainer => _myUseTypeNameAsNamespace;

        public override string Name => _myType.Name;

        public Type Target => _myType;

        #endregion
    }

    public sealed class MethodImport : ImportBase
    {

        private readonly MethodInfo _myMethod;
        public MethodImport(MethodInfo importMethod)
        {
            Utility.AssertNotNull(importMethod, "importMethod");
            _myMethod = importMethod;
        }

        internal override void Validate()
        {
            this.Context.AssertTypeIsAccessible(_myMethod.ReflectedType);
        }

        protected override void AddMembers(string memberName, MemberTypes memberType, ICollection<MemberInfo> dest)
        {
            if (string.Equals(memberName, _myMethod.Name, this.Context.Options.MemberStringComparison) == true && (memberType & MemberTypes.Method) != 0)
            {
                dest.Add(_myMethod);
            }
        }

        protected override void AddMembers(MemberTypes memberType, ICollection<MemberInfo> dest)
        {
            if ((memberType & MemberTypes.Method) != 0)
            {
                dest.Add(_myMethod);
            }
        }

        internal override bool IsMatch(string name)
        {
            return string.Equals(_myMethod.Name, name, this.Context.Options.MemberStringComparison);
        }

        internal override Type FindType(string typeName)
        {
            return null;
        }

        protected override bool EqualsInternal(ImportBase import)
        {
            MethodImport otherSameType = import as MethodImport;
            return (otherSameType != null) && _myMethod.MethodHandle.Equals(otherSameType._myMethod.MethodHandle);
        }

        public override string Name => _myMethod.Name;

        public MethodInfo Target => _myMethod;
    }

    public sealed class NamespaceImport : ImportBase, ICollection<ImportBase>
    {
        private readonly string _myNamespace;
        private readonly List<ImportBase> _myImports;
        public NamespaceImport(string importNamespace)
        {
            Utility.AssertNotNull(importNamespace, "importNamespace");
            if (importNamespace.Length == 0)
            {
                string msg = Utility.GetGeneralErrorMessage(GeneralErrorResourceKeys.InvalidNamespaceName);
                throw new ArgumentException(msg);
            }

            _myNamespace = importNamespace;
            _myImports = new List<ImportBase>();
        }

        internal override void SetContext(ExpressionContext context)
        {
            base.SetContext(context);

            foreach (ImportBase import in _myImports)
            {
                import.SetContext(context);
            }
        }

        internal override void Validate()
        {
        }

        protected override void AddMembers(string memberName, MemberTypes memberType, ICollection<MemberInfo> dest)
        {
            foreach (ImportBase import in this.NonContainerImports)
            {
                AddImportMembers(import, memberName, memberType, dest);
            }
        }

        protected override void AddMembers(MemberTypes memberType, ICollection<MemberInfo> dest)
        {
        }

        internal override Type FindType(string typeName)
        {
            foreach (ImportBase import in this.NonContainerImports)
            {
                Type t = import.FindType(typeName);

                if ((t != null))
                {
                    return t;
                }
            }

            return null;
        }

        internal override ImportBase FindImport(string name)
        {
            foreach (ImportBase import in _myImports)
            {
                if (import.IsMatch(name) == true)
                {
                    return import;
                }
            }
            return null;
        }

        internal override bool IsMatch(string name)
        {
            return string.Equals(_myNamespace, name, this.Context.Options.MemberStringComparison);
        }

        private ICollection<ImportBase> NonContainerImports
        {
            get
            {
                List<ImportBase> found = new List<ImportBase>();

                foreach (ImportBase import in _myImports)
                {
                    if (import.IsContainer == false)
                    {
                        found.Add(import);
                    }
                }

                return found;
            }
        }

        protected override bool EqualsInternal(ImportBase import)
        {
            NamespaceImport otherSameType = import as NamespaceImport;
            return (otherSameType != null) && _myNamespace.Equals(otherSameType._myNamespace, this.Context.Options.MemberStringComparison);
        }

        public override bool IsContainer => true;

        public override string Name => _myNamespace;

        #region "ICollection implementation"
        public void Add(ImportBase item)
        {
            Utility.AssertNotNull(item, "item");

            if ((this.Context != null))
            {
                item.SetContext(this.Context);
            }

            _myImports.Add(item);
        }

        public void Clear()
        {
            _myImports.Clear();
        }

        public bool Contains(ImportBase item)
        {
            return _myImports.Contains(item);
        }

        public void CopyTo(ImportBase[] array, int arrayIndex)
        {
            _myImports.CopyTo(array, arrayIndex);
        }

        public bool Remove(ImportBase item)
        {
            return _myImports.Remove(item);
        }

        public override System.Collections.Generic.IEnumerator<ImportBase> GetEnumerator()
        {
            return _myImports.GetEnumerator();
        }

        public int Count => _myImports.Count;

        public bool IsReadOnly => false;

        #endregion
    }
}
