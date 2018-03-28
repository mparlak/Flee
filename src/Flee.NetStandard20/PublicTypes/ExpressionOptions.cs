using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;
using System.Globalization;
using Flee.InternalTypes;


namespace Flee.PublicTypes
{
    public sealed class ExpressionOptions
    {

        private PropertyDictionary _myProperties;
        private Type _myOwnerType;
        private readonly ExpressionContext _myOwner;
        internal event EventHandler CaseSensitiveChanged;

        internal ExpressionOptions(ExpressionContext owner)
        {
            _myOwner = owner;
            _myProperties = new PropertyDictionary();

            this.InitializeProperties();
        }

        #region "Methods - Private"

        private void InitializeProperties()
        {
            this.StringComparison = System.StringComparison.Ordinal;
            this.OwnerMemberAccess = BindingFlags.Public;

            _myProperties.SetToDefault<bool>("CaseSensitive");
            _myProperties.SetToDefault<bool>("Checked");
            _myProperties.SetToDefault<bool>("EmitToAssembly");
            _myProperties.SetToDefault<Type>("ResultType");
            _myProperties.SetToDefault<bool>("IsGeneric");
            _myProperties.SetToDefault<bool>("IntegersAsDoubles");
            _myProperties.SetValue("ParseCulture", CultureInfo.CurrentCulture);
            this.SetParseCulture(this.ParseCulture);
            _myProperties.SetValue("RealLiteralDataType", RealLiteralDataType.Double);
        }

        private void SetParseCulture(CultureInfo ci)
        {
            ExpressionParserOptions po = _myOwner.ParserOptions;
            po.DecimalSeparator = Convert.ToChar(ci.NumberFormat.NumberDecimalSeparator);
            po.FunctionArgumentSeparator = Convert.ToChar(ci.TextInfo.ListSeparator);
            po.DateTimeFormat = ci.DateTimeFormat.ShortDatePattern;
        }

        #endregion

        #region "Methods - Internal"

        internal ExpressionOptions Clone()
        {
            ExpressionOptions clonedOptions = (ExpressionOptions)this.MemberwiseClone();
            clonedOptions._myProperties = _myProperties.Clone();
            return clonedOptions;
        }

        internal bool IsOwnerType(Type t)
        {
            return this._myOwnerType.IsAssignableFrom(t);
        }

        internal void SetOwnerType(Type ownerType)
        {
            _myOwnerType = ownerType;
        }

        #endregion

        #region "Properties - Public"
        public Type ResultType
        {
            get { return _myProperties.GetValue<Type>("ResultType"); }
            set
            {
                Utility.AssertNotNull(value, "value");
                _myProperties.SetValue("ResultType", value);
            }
        }

        public bool Checked
        {
            get { return _myProperties.GetValue<bool>("Checked"); }
            set { _myProperties.SetValue("Checked", value); }
        }

        public StringComparison StringComparison
        {
            get { return _myProperties.GetValue<StringComparison>("StringComparison"); }
            set { _myProperties.SetValue("StringComparison", value); }
        }

        public bool EmitToAssembly
        {
            get { return _myProperties.GetValue<bool>("EmitToAssembly"); }
            set { _myProperties.SetValue("EmitToAssembly", value); }
        }

        public BindingFlags OwnerMemberAccess
        {
            get { return _myProperties.GetValue<BindingFlags>("OwnerMemberAccess"); }
            set { _myProperties.SetValue("OwnerMemberAccess", value); }
        }

        public bool CaseSensitive
        {
            get { return _myProperties.GetValue<bool>("CaseSensitive"); }
            set
            {
                if (this.CaseSensitive != value)
                {
                    _myProperties.SetValue("CaseSensitive", value);
                    if (CaseSensitiveChanged != null)
                    {
                        CaseSensitiveChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public bool IntegersAsDoubles
        {
            get { return _myProperties.GetValue<bool>("IntegersAsDoubles"); }
            set { _myProperties.SetValue("IntegersAsDoubles", value); }
        }

        public CultureInfo ParseCulture
        {
            get { return _myProperties.GetValue<CultureInfo>("ParseCulture"); }
            set
            {
                Utility.AssertNotNull(value, "ParseCulture");
                if ((value.LCID != this.ParseCulture.LCID))
                {
                    _myProperties.SetValue("ParseCulture", value);
                    this.SetParseCulture(value);
                    _myOwner.ParserOptions.RecreateParser();
                }
            }
        }

        public RealLiteralDataType RealLiteralDataType
        {
            get { return _myProperties.GetValue<RealLiteralDataType>("RealLiteralDataType"); }
            set { _myProperties.SetValue("RealLiteralDataType", value); }
        }
        #endregion

        #region "Properties - Non Public"
        internal IEqualityComparer<string> StringComparer
        {
            get
            {
                if (this.CaseSensitive == true)
                {
                    return System.StringComparer.Ordinal;
                }
                else
                {
                    return System.StringComparer.OrdinalIgnoreCase;
                }
            }
        }

        internal MemberFilter MemberFilter
        {
            get
            {
                if (this.CaseSensitive == true)
                {
                    return Type.FilterName;
                }
                else
                {
                    return Type.FilterNameIgnoreCase;
                }
            }
        }

        internal StringComparison MemberStringComparison
        {
            get
            {
                if (this.CaseSensitive == true)
                {
                    return System.StringComparison.Ordinal;
                }
                else
                {
                    return System.StringComparison.OrdinalIgnoreCase;
                }
            }
        }

        internal Type OwnerType => _myOwnerType;

        internal bool IsGeneric
        {
            get { return _myProperties.GetValue<bool>("IsGeneric"); }
            set { _myProperties.SetValue("IsGeneric", value); }
        }
        #endregion
    }
}
