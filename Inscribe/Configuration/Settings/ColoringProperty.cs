using System.Windows.Media;
using System.Xml.Serialization;

namespace Inscribe.Configuration.Settings
{
    /// <summary>
    /// タイムラインの色設定
    /// </summary>
    public class ColoringProperty
    {
        public DisablableColorElement MyCurrentTweet =
            new DisablableColorElement(Colors.LightSkyBlue, true);

        public DisablableColorElement MySubTweet =
            new DisablableColorElement(Colors.LightSkyBlue, true);

        public DisablableColorElement InReplyToMeCurrent =
            new DisablableColorElement(255, 190, 100, true);

        public DisablableColorElement InReplyToMeSub =
            new DisablableColorElement(255, 190, 100, true);

        public DisablableColorElement Friend =
            new DisablableColorElement(240, 240, 180, true);

        public DisablableColorElement Following =
            new DisablableColorElement(120, 240, 180, true);

        public DisablableColorElement Follower =
            new DisablableColorElement(240, 120, 180, true);

        public DisablableColorElement FriendAny =
            new DisablableColorElement(210, 210, 150, true);

        public DisablableColorElement FollowingAny =
            new DisablableColorElement(50, 210, 150, true);

        public DisablableColorElement FollowerAny =
            new DisablableColorElement(210, 50, 150, true);

        public ColorElement BaseHighlightColor =
            new ColorElement(Colors.Gray);

        public DisablablePairColorElement Selected =
            new DisablablePairColorElement(
                new DisablableColorElement(Colors.Khaki, true),
                new DisablableColorElement(Colors.DarkOrange, false));

        public DisablablePairColorElement Retweeted =
            new DisablablePairColorElement(
                new DisablableColorElement(200, 255, 200, true),
                new DisablableColorElement(Colors.ForestGreen, true));

        public DisablablePairColorElement DirectMessage =
            new DisablablePairColorElement(
                new DisablableColorElement(Colors.MistyRose, true),
                new DisablableColorElement(Colors.Red, true));

        public DisablablePairColorElement DirectMessageToSub =
            new DisablablePairColorElement(
                new DisablableColorElement(Colors.MistyRose, true),
                new DisablableColorElement(Colors.Salmon, true));

        public PairColorElement BaseColor =
            new PairColorElement(
                new ColorElement(Colors.White),
                new ColorElement(Colors.Black));

        public ColorElement BaseLinkColor =
            new ColorElement(Colors.RoyalBlue);
    }

    public interface IColorElement
    {
        Color GetColor();
    }

    public interface IPairColorElement
    {
        bool IsLightActivated { get; }
        bool IsDarkActivated { get; }

        Brush GetLightBrush();
        
        Brush GetDarkBrush();
        
        Brush GetBrush(bool dark);

        Color GetLightColor();

        Color GetDarkColor();

        Color GetColor(bool dark);
    }

    /// <summary>
    /// 通常色とダーク色があるBrushProperty、ダーク色は省略可能
    /// </summary>
    public class PairColorElement : IPairColorElement
    {
        public PairColorElement()
        {
            LightColor = new ColorElement(Colors.Transparent);
            DarkColor = new DisablableColorElement(Colors.Transparent, false);
        }
        public PairColorElement(ColorElement light, ColorElement dark)
        {
            LightColor = light;
            DarkColor = dark;
        }

        public ColorElement LightColor { get; set; }
        public ColorElement DarkColor { get; set; }

        public Brush GetLightBrush()
        {
            return LightColor.GetBrush();
        }

        public Brush GetDarkBrush()
        {
            return DarkColor.GetBrush();
        }

        public Brush GetBrush(bool dark)
        {
            if (dark)
                return GetDarkBrush();
            else
                return GetLightBrush();
        }

        public Color GetLightColor()
        {
            return LightColor.GetColor();
        }

        public Color GetDarkColor()
        {
            return DarkColor.GetColor();
        }

        public Color GetColor(bool dark)
        {
            if (dark)
                return GetDarkColor();
            else
                return GetLightColor();
        }

        public bool IsLightActivated
        {
            get { return true; }
        }

        public bool IsDarkActivated
        {
            get { return true; }
        }
    }

    public class DisablablePairColorElement : IPairColorElement
    {
        public DisablablePairColorElement()
        {
            LightColor = new DisablableColorElement(Colors.Transparent, false);
            DarkColor = new DisablableColorElement(Colors.Transparent, false);
        }

        public DisablablePairColorElement(DisablableColorElement light, DisablableColorElement dark)
        {
            LightColor = light;
            DarkColor = dark;
        }

        public DisablableColorElement LightColor { get; set; }
        public DisablableColorElement DarkColor { get; set; }
        public bool Activated
        {
            get { return LightColor.IsActivated; }
            set { LightColor.IsActivated = value; }
        }
        public bool ActivatedSub
        {
            get { return DarkColor.IsActivated; }
            set { DarkColor.IsActivated = value; }
        }

        public Brush GetLightBrush()
        {
            return LightColor.GetBrush();
        }

        public Brush GetDarkBrush()
        {
            return DarkColor.GetBrush();
        }

        public Brush GetBrush(bool dark)
        {
            if (dark)
                return GetDarkBrush();
            else
                return GetLightBrush();
        }

        public Color GetLightColor()
        {
            return LightColor.GetColor();
        }

        public Color GetDarkColor()
        {
            return DarkColor.GetColor();
        }

        public Color GetColor(bool dark)
        {
            if (dark)
                return GetDarkColor();
            else
                return GetLightColor();
        }

        public bool IsLightActivated
        {
            get { return LightColor.IsActivated; }
        }

        public bool IsDarkActivated
        {
            get { return DarkColor.IsActivated; }
        }
    }

    /// <summary>
    /// ブラシ色指定プロパティ
    /// </summary>
    public class ColorElement
    {
        public ColorElement() : this(0, 0, 0) { }
        public ColorElement(byte or, byte og, byte ob)
        {
            defaults = new[] { or, og, ob };
            R = or;
            G = og;
            B = ob;
        }

        public ColorElement(Color c)
            : this(c.R, c.G, c.B) { }

        [XmlIgnore()]
        public byte[] defaults = null;

        [XmlAttribute("R")]
        public byte R = 0;
        [XmlAttribute("G")]
        public byte G = 0;
        [XmlAttribute("B")]
        public byte B = 0;

        public virtual Brush GetBrush()
        {
            return new SolidColorBrush(GetColor());
        }

        public virtual Color GetColor()
        {
            return Color.FromRgb(R, G, B);
        }

        public void SetDefault()
        {
            R = defaults[0];
            G = defaults[1];
            B = defaults[2];
        }

        public Color GetDefaultColor()
        {
            return Color.FromRgb(defaults[0], defaults[1], defaults[2]);
        }
    }

    /// <summary>
    /// 無効にすることが可能なブラシ色指定プロパティ
    /// </summary>
    public class DisablableColorElement : ColorElement
    {
        public DisablableColorElement() : this(0, 0, 0, false) { }
        public DisablableColorElement(byte or, byte og, byte ob, bool act)
            : base(or, og, ob)
        {
            this.OrigActivated = act;
            this.IsActivated = act;
        }

        public DisablableColorElement(Color c, bool act)
            : this(c.R, c.G, c.B, act) { }

        [XmlIgnore()]
        public bool OrigActivated = false;

        [XmlAttribute("IsActive")]
        public bool IsActivated = false;

        /// <summary>
        /// ブラシ、またはNULLが返ります。
        /// </summary>
        public override Brush GetBrush()
        {
            if (IsActivated)
                return base.GetBrush();
            else
                return null;
        }

        /// <summary>
        /// カラーが返ります。<para />
        /// アクティベートされていない場合はTransparentが返ります。
        /// </summary>
        public override Color GetColor()
        {
            if (IsActivated)
                return base.GetColor();
            else
                return Colors.Transparent;
        }
    }
}
