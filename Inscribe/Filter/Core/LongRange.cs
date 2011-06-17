using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Filter.Core
{
    /// <summary>
    /// RangeOfInteger
    /// </summary>
    public class LongRange
    {
        public RangeType RangeType
        {
            get
            {
                if (from != null && to != null)
                {
                    if (from == to)
                        return Core.RangeType.Pivot;
                    else
                        return RangeType.Between;
                }
                else if (from != null)
                    return RangeType.From;
                else
                    return RangeType.To;

            }
        }

        private long? from;
        public long? From
        {
            get { return this.from; }
            set { this.from = value; }
        }

        private long? to;
        public long? To
        {
            get { return this.to; }
            set { this.to = value; }
        }

        public LongRange(long? from = null, long? to = null)
        {
            if (from == null && to == null)
                throw new ArgumentException("Range unexisted.");
            if (from != null && to != null && from > to)
                throw new ArgumentException("Invalid range.");
            this.from = from;
            this.to = to;
        }

        /// <summary>
        /// Range 内にvalueがあるか確認します。
        /// </summary>
        /// <param name="value">確認する値</param>
        public bool Check(long value)
        {
            switch (RangeType)
            {
                case Core.RangeType.Between:
                    return from <= value && to >= value;
                case Core.RangeType.From:
                    return from <= value;
                case Core.RangeType.To:
                    return to >= value;
                case Core.RangeType.Pivot:
                    return from == value;
                default:
                    return false;
            }
        }

        public static LongRange FromPivotValue(long value)
        {
            return new LongRange(value, value);
        }

        public static LongRange FromFromValue(long value)
        {
            return new LongRange(from: value);
        }

        public static LongRange FromToValue(long value)
        {
            return new LongRange(to: value);
        }

        public static LongRange FromBetweenValues(long v1, long v2)
        {
            if (v1 < v2)
                return new LongRange(v1, v2);
            else
                return new LongRange(v2, v1);
        }

        public static bool TryParse(string value, out LongRange range)
        {
            try
            {
                range = LongRange.Parse(value);
                return true;
            }
            catch
            {
                range = null;
                return false;
            }
        }

        public static LongRange Parse(string value)
        {
            var sary = value.Split(new[] { "-" }, StringSplitOptions.None).Select(s => s.Trim()).ToArray();
            if (sary.Length == 1)
            {
                // double じゃない？
                return LongRange.FromPivotValue(long.Parse(sary[0]));
            }
            else if (sary.Length == 2)
            {
                // 二つある
                if (String.IsNullOrEmpty(sary[0]))
                {
                    // To
                    return LongRange.FromToValue(long.Parse(sary[1]));
                }
                else if (String.IsNullOrEmpty(sary[1]))
                {
                    // From
                    return LongRange.FromFromValue(long.Parse(sary[0]));
                }
                else
                {
                    // Between
                    return LongRange.FromBetweenValues(long.Parse(sary[0]), long.Parse(sary[1]));
                }
            }
            else
                throw new ArgumentException("引数が不正です。-が多すぎるか、または無効な文字列です。");

        }

        public override string ToString()
        {
            switch (this.RangeType)
            {
                case Core.RangeType.Between:
                    return this.from.ToString() + "-" + this.to.ToString();
                case Core.RangeType.From:
                    return this.from.ToString() + "-";
                case Core.RangeType.Pivot:
                    return this.from.ToString();
                case Core.RangeType.To:
                    return "-" + this.to.ToString();
                default:
                    return "0";
            }
        }
    }

    public enum RangeType
    {
        From,
        To,
        Between,
        Pivot,
    }
}
