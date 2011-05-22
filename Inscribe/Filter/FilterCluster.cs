﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dulcet.Twitter;

namespace Inscribe.Filter
{
    /// <summary>
    /// フィルタクラスタ
    /// </summary>
    public class FilterCluster : IFilter
    {
        /// <summary>
        /// 登録されているフィルタ
        /// </summary>
        private IEnumerable<IFilter> _filters = null;

        /// <summary>
        /// このクラスタが含むフィルタ
        /// </summary>
        public IEnumerable<IFilter> Filters
        {
            get
            {
                if (_filters == null)
                    return new IFilter[0];
                else
                    return _filters;
            }
            set
            {
                UpdateChain(false);
                _filters = value;
                UpdateChain(true);
            }
        }

        #region Reacception chain

        private void UpdateChain(bool attach)
        {
            if (_filters != null)
            {
                foreach (var f in _filters)
                {
                    if (attach)
                        f.RequireReaccept += new Action(filter_RequireReaccept);
                    else
                        f.RequireReaccept -= new Action(filter_RequireReaccept);
                }
            }
        }

        void filter_RequireReaccept()
        {
            // 再適用の伝播
            this.RequireReaccept();
        }

        #endregion

        /// <summary>
        /// このクラスタの真偽値がクラスタに登録されているフィルタ全体のOR結合で判定されるかを設定します。
        /// </summary>
        public bool ConcatenateOR { get; set; }

        /// <summary>
        /// このフィルタクラスタに所属するフィルタについて、フィルタ条件に合致するかの判定を行います。<para />
        /// Negateは内部で考慮されます。<para />
        /// フィルタが一つもない場合は、U=∅のときの∀(AND)、∃(OR)のBoolean値に準じます。<para />
        /// (ANDのとき:TRUE ORのとき:FALSE)
        /// </summary>
        /// <param name="status">フィルタテストするステータス</param>
        /// <returns>ステータスがフィルタに合致するか</returns>
        public bool Filter(TwitterStatusBase status)
        {
            if (_filters != null)
            {
                foreach (var f in _filters)
                {
                    // ANDのとき => FALSEに出会ったらFALSEを返す
                    // ORのとき => TRUEに出会ったらTRUEを返す　
                    if (f.Filter(status) == ConcatenateOR)
                        return ConcatenateOR == !Negate;
                }
            }
            // ANDのとき => FALSEに出会っていないのでTRUEを返す
            // ORのとき => TRUEに出会っていないのでFALSEを返す
            return !ConcatenateOR == !Negate;
        }

        public event Action RequireReaccept = () => { };

        /// <summary>
        /// 否定フィルタであるか
        /// </summary>
        public bool Negate { get; set; }

        /// <summary>
        /// このフィルタをクエリ化します。
        /// </summary>
        public string ToQuery()
        {
            if (_filters == null || _filters.Count() == 0)
            {
                if (this.ConcatenateOR)
                    return this.Negate ? "()" : "!()";
                else
                    return this.Negate ? "!()" : "()";
            }
            else
            {
                var strs = from f in _filters
                           select f.ToQuery();
                return (this.Negate ? "!(" : "(") + String.Join(
                    ConcatenateOR ? " || " : " && ",
                    strs) + ")";
            }
        }
    }
}