using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Inscribe.Filter;
using Inscribe.Filter.Core;
using Livet;
using Livet.Commands;
using Livet.Messaging.Windows;
using Inscribe.Configuration;

namespace Inscribe.ViewModels.Common.Filter
{
    public class FilterElementEditorViewModel : ViewModel
    {
        public bool Success { get; private set; }

        private bool _isNegate = false;
        public bool IsNegate
        {
            get { return _isNegate; }
            set
            {
                this._isNegate = value;
                RaisePropertyChanged(() => IsNegate);
                if (this._cofiguredFilterBase != null)
                    this._cofiguredFilterBase.Negate = value;
            }
        }

        public FilterElementEditorViewModel(FilterBase filter = null)
        {
            if (filter != null)
            {
                this._currentSelectedItem = filter.Identifier + ": " + filter.Description;
                this._prevDescString = this._currentSelectedItem;
                ConfiguredFilterBase = QueryCompiler.ToFilter(filter.ToQuery()).Filters.First() as FilterBase;
                this.IsNegate = filter.Negate;
            }
            else
            {
                this.CurrentSelectedItem = GetFilterDescriptions().First();
            }
            this._filterDescriptions = GetFilterDescriptions().ToArray();
        }

        private IEnumerable<string> _filterDescriptions;
        public IEnumerable<string> FilterDescriptions
        {
            get { return this._filterDescriptions; }
        }

        private IEnumerable<string> GetFilterDescriptions()
        {
            foreach (var s in FilterRegistrant.FilterKeys.OrderBy(s => s))
            {
                int localcount = 0;
                foreach (var type in FilterRegistrant.GetFilter(s))
                {
                    if (type != null)
                    {
                        FilterBase inst = null;
                        var ci = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)[0];
                        if (ci != null)
                            inst = ci.Invoke(new object[0]) as FilterBase;
                        if (inst != null)
                        {
                            if (inst.IsOnlyForTranscender &&
                                !Setting.Instance.ExperienceProperty.IsTranscender)
                                continue;
                            if (inst.Identifier != s) // hit by alias
                                continue;
                            if (localcount > 0)
                                yield return s + "(" + localcount + "): " + inst.Description;
                            else
                                yield return s + ": " + inst.Description;
                        }
                    }
                }
            }
        }

        private string _currentSelectedItem = null;
        public string CurrentSelectedItem
        {
            get { return this._currentSelectedItem; }
            set
            {
                this._currentSelectedItem = value;
                RaisePropertyChanged(() => _currentSelectedItem);
                if (String.IsNullOrWhiteSpace(_currentSelectedItem) ||
                    !_currentSelectedItem.Contains(":"))
                    return;
                var id = _currentSelectedItem.Substring(0, _currentSelectedItem.IndexOf(":"));
                int count = 0;
                if (id.EndsWith(")"))
                {
                    count = int.Parse(id.Substring(id.IndexOf("("), 1));
                    id = id.Substring(0, id.IndexOf("("));
                }
                var ftype = FilterRegistrant.GetFilter(id).Skip(count).FirstOrDefault();
                if (ftype == null)
                    return;
                var ci = ftype.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)[0];
                if (ci != null)
                    ConfiguredFilterBase = ci.Invoke(new object[0]) as FilterBase;
            }
        }

        private string _prevDescString = null;
        private IEnumerable<ViewModel> _currentHostedArgumentVMs = null;
        public IEnumerable<ViewModel> ArgumentViewModels
        {
            get
            {
                return this._currentHostedArgumentVMs;
            }
        }

        private FilterBase _cofiguredFilterBase = null;
        public FilterBase ConfiguredFilterBase
        {
            get { return this._cofiguredFilterBase; }
            set
            {
                this._cofiguredFilterBase = value;
                RaisePropertyChanged(() => ConfiguredFilterBase);
                if (value != null)
                {
                    _cofiguredFilterBase.Negate = this.IsNegate;
                    this._currentHostedArgumentVMs =
                        from p in value.GetType().GetProperties()
                        where p.CanRead && p.CanWrite
                        let attr = Attribute.GetCustomAttributes(p, typeof(GuiVisibleAttribute)).OfType<GuiVisibleAttribute>().FirstOrDefault()
                        where attr != null
                        select GenerateViewModel(p.GetGetMethod().ReturnType, attr.Description, attr.Hint, p.GetValue(value, null), o => p.SetValue(value, o, null));
                    RaisePropertyChanged(() => ArgumentViewModels);
                }
            }
        }

        private ViewModel GenerateViewModel(Type t, string desc, string hint, object value, Action<object> setValueHandler)
        {
            if (t.Equals(typeof(String)))
                return new FilterTextArgumentViewModel(desc, hint, (string)value, setValueHandler);
            else if (t.Equals(typeof(Boolean)))
                return new FilterBooleanArgumentViewModel(desc, hint, (bool)value, setValueHandler);
            else if (t.Equals(typeof(LongRange)))
                return new FilterLongRangeArgumentViewModel(desc, hint, (LongRange)value, setValueHandler);
            else if (t.Equals(typeof(long)))
                return new FilterNumericArgumentViewModel(desc, hint, (long)value, setValueHandler);
            else
                return new FilterUneditableArgumentViewModel(desc, t);
        }

        #region OKCommand
        ViewModelCommand _OKCommand;

        public ViewModelCommand OKCommand
        {
            get
            {
                if (_OKCommand == null)
                    _OKCommand = new ViewModelCommand(OK);
                return _OKCommand;
            }
        }

        private void OK()
        {
            this.Success = true;
            Close();
        }
        #endregion

        #region CancelCommand
        ViewModelCommand _CancelCommand;

        public ViewModelCommand CancelCommand
        {
            get
            {
                if (_CancelCommand == null)
                    _CancelCommand = new ViewModelCommand(Cancel);
                return _CancelCommand;
            }
        }

        private void Cancel()
        {
            this.Success = false;
            Close();
        }
        #endregion

        private void Close()
        {
            Messenger.Raise(new WindowActionMessage("Close", WindowAction.Close));
        }
    }

    public class FilterTextArgumentViewModel : ViewModel
    {
        public FilterTextArgumentViewModel(string desc, string hint, string value, Action<string> setValueHandler)
        {
            this._value = value;
            this.Description = desc;
            this.Hint = hint;
            this.handler = setValueHandler;
        }

        private String _value;

        public String Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged(() => Value);
                if (this.handler != null)
                    handler(value);
            }
        }

        private Action<string> handler;

        public String Description { get; private set; }

        public bool IsHintVisible
        {
            get { return !String.IsNullOrEmpty(Hint); }
        }

        public String Hint { get; private set; }

    }

    public class FilterBooleanArgumentViewModel : ViewModel
    {
        public FilterBooleanArgumentViewModel(string desc, string hint, bool value, Action<object> setValueHandler)
        {
            this._value = value;
            this.Description = desc;
            this.Hint = hint;
            this.handler = setValueHandler;
        }

        private bool _value;

        public bool Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged(() => Value);
                if (this.handler != null)
                    handler(value);
            }
        }

        private Action<object> handler;

        public string Description { get; private set; }

        public bool IsHintVisible
        {
            get { return !String.IsNullOrEmpty(Hint); }
        }

        public String Hint { get; private set; }
    }

    public class FilterNumericArgumentViewModel : ViewModel
    {

        public FilterNumericArgumentViewModel(string desc, string hint, long value, Action<object> setValueHandler)
        {
            this._value = value;
            this.Description = desc;
            this.Hint = hint;
            this.handler = setValueHandler;
        }

        private long _value;

        public long Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged(() => Value);
                if (this.handler != null)
                    handler(value);
            }
        }

        private Action<object> handler;

        public string Description { get; private set; }

        public bool IsHintVisible
        {
            get { return !String.IsNullOrEmpty(Hint); }
        }

        public String Hint { get; private set; }
    }

    public class FilterLongRangeArgumentViewModel : ViewModel
    {
        public FilterLongRangeArgumentViewModel(string desc, string hint, LongRange value, Action<LongRange> setValueHandler)
        {
            this._value = value;
            this.Description = desc;
            this.Hint = hint;
            this.handler = setValueHandler;
        }

        private Action<LongRange> handler;

        private LongRange _value;
        public LongRange Value
        {
            get { return this._value ?? LongRange.FromPivotValue(0); }
            set
            {
                this._value = value;
                RaisePropertyChanged(() => Value);
                if (handler != null)
                    handler(value);
            }
        }

        public string Description { get; private set; }

        public bool IsHintVisible
        {
            get { return !String.IsNullOrEmpty(Hint); }
        }

        public String Hint { get; private set; }
    }

    public class FilterUneditableArgumentViewModel : ViewModel
    {
        public FilterUneditableArgumentViewModel(string desc, Type type)
        {
            this.Description = desc;
            this.Type = type.ToString();
        }

        public string Type { get; private set; }

        public string Description { get; private set; }
    }
}
