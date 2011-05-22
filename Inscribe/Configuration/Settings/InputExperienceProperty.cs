﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Inscribe.Storage;

namespace Inscribe.Configuration.Settings
{
    public class InputExperienceProperty
    {
        public InputExperienceProperty()
        {
            TrimBeginSpace = true;
            ReplyAccountAutoDecision = true;
            ClosePostBoxAfterPost = false;
            UseInputSuggesting = true;
            AutoBindInputtedHashtag = false;
        }

        public bool TrimBeginSpace { get; set; }

        public bool ReplyAccountAutoDecision { get; set; }

        public bool ClosePostBoxAfterPost { get; set; }

        public bool UseInputSuggesting { get; set; }

        private HashtagAutoBindDescription[] _hashtagAutoBindDescriptions = null;
        public HashtagAutoBindDescription[] HashtagAutoBindDescriptions
        {
            get { return _hashtagAutoBindDescriptions ?? new HashtagAutoBindDescription[0]; }
            set { _hashtagAutoBindDescriptions = value; }
        }

        public bool AutoBindInputtedHashtag { get; set; }
    }

    public class HashtagAutoBindDescription
    {
        public enum AutoBindStrategy
        {
            Contains = 0,
            StartsWith = 1,
            EndsWith = 2,
            Regex = 3
        }

        public AutoBindStrategy Strategy { get; set; }

        public string ConditionText { get; set; }

        public bool IsNegateCondition { get; set; }

        public string TagText { get; set; }

        private bool _warnedRegexError = false;
        public bool CheckCondition(string text)
        {
            switch (Strategy)
            {
                case AutoBindStrategy.Contains:
                    return text.IndexOf(ConditionText, StringComparison.CurrentCultureIgnoreCase) >= 0 == !IsNegateCondition;
                case AutoBindStrategy.StartsWith:
                    return text.StartsWith(ConditionText, StringComparison.CurrentCultureIgnoreCase) == !IsNegateCondition;
                case AutoBindStrategy.EndsWith:
                    return text.EndsWith(ConditionText, StringComparison.CurrentCultureIgnoreCase) == !IsNegateCondition;
                case AutoBindStrategy.Regex:
                    try
                    {
                        return Regex.IsMatch(text, ConditionText) == !IsNegateCondition;
                    }
                    catch(Exception e)
                    {
                        if (!_warnedRegexError)
                        {
                            NotifyStorage.Notify("ハッシュタグ自動バインド:正規表現エラー " + e.Message);
                            _warnedRegexError = true;
                        }
                        return false;
                    }
                default:
                    return false;
            }
        }

        public override string ToString()
        {
            return "#" + TagText + " : " + (IsNegateCondition ? "!" : "") +
                "\"" + ConditionText + "\" - " + Strategy.ToString();
        }
    }
}