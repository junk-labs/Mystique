using System.Windows.Media;
using System.Xml.Serialization;

namespace Inscribe.Configuration.Settings
{
    /// <summary>
    /// タイムラインの色設定
    /// </summary>
    public class ColoringProperty
    {
        #region NameBackColor

        public DisablableColorElement MyColor =
            new DisablableColorElement(0x99, 0xcd, 0xff);

        public DisablableColorElement FriendColor =
            new DisablableColorElement(0xff, 0xe0, 0x99);

        public DisablableColorElement FollowingColor =
            new DisablableColorElement(0xaa, 0xff, 0x99);

        public DisablableColorElement FollowerColor =
            new DisablableColorElement(0xff, 0x99, 0x99);

        public DisablableColorElement DirectMessageNameColor =
            new DisablableColorElement(0xff, 0x99, 0x99);

        public ColorElement DefaultNameColor =
            new ColorElement(Colors.White);

        #endregion

        #region TextBackColor

        public DisablablePairColorElement RetweetedColor =
            new DisablablePairColorElement(
                new DisablableColorElement(0xc3, 0xfe, 0xd5),
                new DisablableColorElement(0x99, 0xff, 0xb7));

        public DisablablePairColorElement DirectMessageColor =
            new DisablablePairColorElement(
                new DisablableColorElement(0xff, 0xd0, 0xd0),
                new DisablableColorElement(0xff, 0xc0, 0xc0));

        public DisablablePairColorElement MentionColor =
            new DisablablePairColorElement(
                new DisablableColorElement(0xff, 0xee, 0xb0),
                new DisablableColorElement(0xff, 0xe4, 0xa0));

        public DisablablePairColorElement SelectedColor =
            new DisablablePairColorElement(
                new DisablableColorElement(0xff, 0xff, 0x8c),
                new DisablableColorElement(0xff, 0xff, 0x78));

        public PairColorElement DefaultColor =
            new PairColorElement(
                new ColorElement(Colors.White),
                new DisablableColorElement(0xee, 0xff, 0xff));

        #endregion

        #region TextColor

        public DisablableColorElement RetweetedTextColor =
            new DisablableColorElement(0x00, 0x30, 0x0e);

        public DisablableColorElement DirectMessageTextColor =
            new DisablableColorElement(0x30, 0x00, 0x00);

        public DisablableColorElement MentionTextColor =
            new DisablableColorElement(0x30, 0x27, 0x00);

        public DisablableColorElement SelectedTextColor =
            new DisablableColorElement(0x70, 0x70, 0x00, false);

        public ColorElement DefaultTextColor =
            new ColorElement(Colors.Black);

        public ColorElement DefaultLinkColor =
            new ColorElement(Colors.RoyalBlue);

        #endregion

        #region CommonColor

        public ColorElement PostBoxOpenForeground = new ColorElement(Colors.Black);

        public ColorElement PostBoxOpenBackground = new ColorElement(Colors.White);

        public ColorElement PostBoxCloseForeground = new ColorElement(Colors.Gray);

        public ColorElement PostBoxCloseBackground = new ColorElement(Colors.Transparent);

        public ColorElement PostBoxBorder = new ColorElement(Colors.Gray);

        public ColorElement StatusBarBackground = new ColorElement(Colors.WhiteSmoke);

        public ColorElement TweetWorkerNotifierBackground = new ColorElement(Colors.Transparent);

        public ColorElement TabSelectedBackground = new ColorElement(Colors.White);

        public ColorElement TabSelectedHighlight = new ColorElement(0xA0, 0x87, 0xCE, 0xFA);

        public ColorElement TabBackground = new ColorElement(Colors.Silver);

        public ColorElement TabHighlight = new ColorElement(Colors.White);

        public ColorElement UserProfileBackground = new ColorElement(Colors.White);

        public ColorElement UserProfileDarkBackground = new ColorElement(Colors.WhiteSmoke);

        public ColorElement SearchBackground = new ColorElement(Colors.White);

        public ColorElement SearchForeground = new ColorElement(Colors.Black);

        public ColorElement SearchTextBackground = new ColorElement(Colors.White);

        public ColorElement SearchInactiveForeground = new ColorElement(Colors.Gray);

        public ColorElement SearchBorder = new ColorElement(Colors.Silver);

        #endregion

        public bool SetInputCaretColorWhite = false;

        public bool SetSearchCaretColorWhite = false;
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
        public PairColorElement(ColorElement light, DisablableColorElement dark)
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
    public class ColorElement : IColorElement
    {
        public ColorElement() : this(0, 0, 0, 0) { }
        public ColorElement(byte oa, byte or, byte og, byte ob)
        {
            A = oa;
            R = or;
            G = og;
            B = ob;
        }

        public ColorElement(Color c)
            : this(c.A, c.R, c.G, c.B) { }

        [XmlAttribute("A")]
        public byte A = 0xFF;
        [XmlAttribute("R")]
        public byte R = 0xFF;
        [XmlAttribute("G")]
        public byte G = 0xFF;
        [XmlAttribute("B")]
        public byte B = 0xFF;

        public virtual Brush GetBrush()
        {
            return new SolidColorBrush(GetColor());
        }

        public virtual Color GetColor()
        {
            return Color.FromArgb(A, R, G, B);
        }
    }

    /// <summary>
    /// 無効にすることが可能なブラシ色指定プロパティ
    /// </summary>
    public class DisablableColorElement : ColorElement
    {
        public DisablableColorElement() : this(0, 0, 0, 0, false) { }
        public DisablableColorElement(byte or, byte og, byte ob) : this(0xff, or, og, ob, true) { }
        public DisablableColorElement(byte oa, byte or, byte og, byte ob) : this(oa, or, og, ob, true) { }
        public DisablableColorElement(byte or, byte og, byte ob, bool act) : this(0xff, or, og, ob, act) { }
        public DisablableColorElement(byte oa, byte or, byte og, byte ob, bool act)
            : base(oa, or, og, ob)
        {
            this.IsActivated = act;
        }

        public DisablableColorElement(Color c) : this(c, true) { }
        public DisablableColorElement(Color c, bool act)
            : this(c.A, c.R, c.G, c.B, act) { }

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
