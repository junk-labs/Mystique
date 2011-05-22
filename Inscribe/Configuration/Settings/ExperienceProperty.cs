using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Inscribe.Configuration.Settings
{
    public class ExperienceProperty
    {
        public ExperienceProperty()
        {
            this.SideViewAllocStrategy = DynamicAllocationStrategy.Passive;
            this.PostFinishShowLength = 3000;
            this.PostAnnotatedFinishShowLength = 8000;
            this.TwitterActionNotifyShowLength = 5000;
            this.PowerUserMode = false;
            this.AuthenticatedUserRelationViewMode = AccountUserRelationMode.ShowRelationControl;
            this.StatusMessageDefaultShowLengthSec = 5;
        }

        #region General experience

        /// <summary>
        /// 上級者向けの解説を使用する
        /// </summary>
        public bool PowerUserMode { get; set; }


        #endregion

        #region Window experience

        public enum DynamicAllocationStrategy
        {
            /// <summary>
            /// より空き領域の多い方へ表示します。
            /// </summary>
            Active,
            /// <summary>
            /// 空き領域が足りなくならない限り、同じ方向へ表示します。
            /// </summary>
            Passive,
            /// <summary>
            /// 同じ方向へ常に表示します。
            /// </summary>
            None
        }

        public DynamicAllocationStrategy SideViewAllocStrategy { get; set; }


        #endregion

        #region Callback show expereience

        public int PostFinishShowLength { get; set; }

        public int PostAnnotatedFinishShowLength { get; set; }

        public int TwitterActionNotifyShowLength { get; set; }

        #endregion

        #region Paticular experience

        public enum AccountUserRelationMode
        {
            ShowTwitter,
            ShowRelationControl
        }

        public AccountUserRelationMode AuthenticatedUserRelationViewMode { get; set; }

        public int StatusMessageDefaultShowLengthSec { get; set; }

        #endregion
    }
}
