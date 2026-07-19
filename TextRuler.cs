using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace VaderConsulting
{
    public partial class TextRuler : UserControl
    {
        #region Variables

        #region Events declarations

        public delegate void IndentChangedEventHandler(int NewValue);

        public delegate void MarginChangedEventHandler(int NewValue);

        public delegate void MultiIndentChangedEventHandler(int LeftIndent, int HangIndent);

        public delegate void TabChangedEventHandler(TabEventArgs args);

        public event MultiIndentChangedEventHandler BothLeftIndentsChanged;

        public event IndentChangedEventHandler LeftHangingIndentChanging;

        public event IndentChangedEventHandler LeftIndentChanging;

        public event MarginChangedEventHandler LeftMarginChanging;

        public event IndentChangedEventHandler RightIndentChanging;

        public event MarginChangedEventHandler RightMarginChanging;

        public event TabChangedEventHandler TabAdded;

        public event TabChangedEventHandler TabChanged;

        public event TabChangedEventHandler TabRemoved;

        #endregion

        internal enum ControlItems
        {
            LeftIndent,
            LeftHangingIndent,
            RightIndent,
            LeftMargin,
            RightMargin
        }

        //background color
        Color _baseColor = Color.White;

        //border color
        Color _strokeColor = Color.Black;

        //are tabs enabled?
        bool _tabsEnabled = false;

        //index of the captured object
        int capObject = -1, capTab = -1;

        //value which represents dots per millimiter in current system
        readonly float dotsPermm;

        //drawzone area
        RectangleF drawZone = new RectangleF();

        //items of the ruler
        List<RectangleF> items = new List<RectangleF>();

        //margins and indents in pixels (that is why float)
        private int lMargin = 20, rMargin = 15, llIndent = 20, luIndent = 20, rIndent = 15;

        //indicates if mouse button is being pressed and object is captured
        bool mCaptured = false;

        //control dimensions
        RectangleF me = new RectangleF();

        //indicates if margins are used
        bool noMargins = false;

        //pen to draw with
        Pen p = new Pen(Color.Transparent);

        //position
        int pos = -1;

        //tab stops
        List<RectangleF> tabs = new List<RectangleF>();

        //area which is bounded by margins
        RectangleF workArea = new RectangleF();

        #endregion

        #region Constructor

        public TextRuler()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.Font = new Font("Arial", 7.25f);

            tabs.Clear();

            //margins and indents
            items.Add(new RectangleF());
            items.Add(new RectangleF());
            items.Add(new RectangleF());
            items.Add(new RectangleF());
            items.Add(new RectangleF());
            items.Add(new RectangleF());
            items.Add(new RectangleF());

            /*
             * items[0] - left margin
             * items[1] - right margin
             * items[2] - left indent upper mark
             * items[3] - left indent lower mark (picture region)
             * items[4] - right indent mark
             * items[5] - left indent mark (self-moving region)
             * items[6] - left indent mark (all-moving region)
            */

            using (Graphics g = Graphics.FromHwnd(this.Handle))
            {
                dotsPermm = g.DpiX / 25.4f;
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // TextRuler
            // 
            this.Name = "TextRuler";
            this.Size = new System.Drawing.Size(100, 20);
            this.ResumeLayout(false);

        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets color for the background
        /// </summary>
        [DefaultValue(typeof(Color), "White")]
        [Description("Base color for the control.")]
        public Color BaseColor
        {
            get
            {
                return _baseColor;
            }
            set
            {
                _baseColor = value;
            }
        }

        /// <summary>
        /// Gets or sets color for the border
        /// </summary>
        [DefaultValue(typeof(Color), "Black")]
        [Description("Color of the border drawn on the bounds of control.")]
        public Color BorderColor
        {
            get { return _strokeColor; }
            set { _strokeColor = value; this.Refresh(); }
        }

        /// <summary>
        /// Gets or sets indentation of the first line of the paragraph
        /// </summary>
        [Category("Indents")]
        [Description("Gets or sets left hanging indent. This value is in millimeters.")]
        [DefaultValue(20)]
        public int LeftHangingIndent
        {
            get { return llIndent - 1; }
            set
            {
                llIndent = value + 1;
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets indentation from the left of the base text of the paragraph
        /// </summary>
        [Category("Indents")]
        [Description("Gets or sets left indent. This value is in millimeters.")]
        [DefaultValue(20)]
        public int LeftIndent
        {
            get { return luIndent - 1; }
            set
            {
                luIndent = value + 1;
                this.Refresh();
            }
        }

        /// <summary>
        /// Specifies left margin
        /// </summary>
        [Category("Margins")]
        [Description("Gets or sets left margin. This value is in millimeters.")]
        [DefaultValue(20)]
        public int LeftMargin
        {
            get { return lMargin; }
            set
            {
                if (noMargins != true)
                {
                    lMargin = value;
                }
                this.Refresh();
            }
        }

        /// <summary>
        /// Enables or disables usage of the margins. If disabled, margins values are set to 1.
        /// </summary>
        [Category("Margins")]
        [Description("If true Margins are disabled, otherwise, false.")]
        [DefaultValue(false)]
        public bool NoMargins
        {
            get { return noMargins; }
            set
            {
                noMargins = value;
                if (value == true)
                {
                    this.lMargin = 1;
                    this.rMargin = 1;
                }
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets right indentation of the paragraph
        /// </summary>
        [Category("Indents")]
        [Description("Gets or sets right indent. This value is in millimeters.")]
        [DefaultValue(15)]
        public int RightIndent
        {
            get { return rIndent - 1; }
            set
            {
                rIndent = value + 1;
                this.Refresh();
            }
        }

        /// <summary>
        /// Specifies right margin
        /// </summary>
        [Category("Margins")]
        [Description("Gets or sets right margin. This value is in millimeters.")]
        [DefaultValue(15)]
        public int RightMargin
        {
            get { return rMargin; }
            set
            {
                if (noMargins != true)
                {
                    rMargin = value;
                }
                this.Refresh();
            }
        }

        [Category("Tabulation")]
        [Description("True to display tab stops, otherwise, False")]
        [DefaultValue(false)]
        public bool TabsEnabled
        {
            get { return _tabsEnabled; }
            set { _tabsEnabled = value; this.Refresh(); }
        }

        #endregion

        #region Methods

        public float DotsPerMillimeter
        {
            get { return dotsPermm; }
        }

        /// <summary>
        /// Returns List which contains positions of the tabs converted to millimeters.
        /// </summary>
        public List<int> TabPositions
        {
            get
            {
                List<int> lst = new List<int>();
                int i = 0;
                for (i = 0; i <= tabs.Count - 1; i++)
                {
                    lst.Add((int)(tabs[i].X / dotsPermm));
                }
                lst.Sort();
                return lst;
            }
        }

        /// <summary>
        /// Returns List which contains positions of the tabs in pixels.
        /// </summary>
        public List<int> TabPositionsInPixels
        {
            get
            {
                List<int> lst = new List<int>();
                int i = 0;
                for (i = 0; i <= tabs.Count - 1; i++)
                {
                    lst.Add((int)(tabs[i].X));
                }
                lst.Sort();
                return lst;
            }
        }

        private void AddTab(float pos)
        {
            RectangleF rect = new RectangleF(pos, 10f, 8f, 8f);
            tabs.Add(rect);
            if (TabAdded != null)
                TabAdded.Invoke(CreateTabArgs(pos));
        }

        internal int GetValueInPixels(ControlItems item)
        {
            switch (item)
            {
                case ControlItems.LeftIndent:
                    return (int)(luIndent * dotsPermm);

                case ControlItems.LeftHangingIndent:
                    return (int)(llIndent * dotsPermm);

                case ControlItems.RightIndent:
                    return (int)(rIndent * dotsPermm);

                case ControlItems.LeftMargin:
                    return (int)(lMargin * dotsPermm);

                case ControlItems.RightMargin:
                    return (int)(rMargin * dotsPermm);

                default:
                    return 0;

            }
        }

        /// <summary>
        /// Sets positions for tabs. It uses positions represented in millemeters.
        /// </summary>
        /// <param name="positions"></param>
        public void SetTabPositionsInMillimeters(int[] positions)
        {
            if (positions == null)
            {
                tabs.Clear();
            }
            else
            {
                tabs.Clear();
                int i = 0;
                RectangleF rect;
                for (i = 0; i <= positions.Length - 1; i++)
                {
                    if (positions[i] != 0)
                    {
                        rect = new RectangleF(positions[i] * dotsPermm, 10f, 8f, 8f);
                        tabs.Add(rect);
                    }
                }
                this.Refresh();
            }
        }

        /// <summary>
        /// Sets positions for tabs. It uses positions represented in pixels.
        /// </summary>
        /// <param name="positions"></param>
        public void SetTabPositionsInPixels(int[] positions)
        {
            if (positions == null)
            {
                tabs.Clear();
            }
            else
            {
                tabs.Clear();
                int i = 0;
                for (i = 0; i <= positions.Length - 1; i++)
                {
                    RectangleF rect = new RectangleF(Convert.ToSingle(positions[i]), 10f, 8f, 8f);
                    tabs.Add(rect);
                }
            }
            this.Refresh();
        }

        #endregion

        #region Event classes

        public class TabEventArgs : EventArgs
        {
            private int _NewPosition = -1;
            private int _OldPosition = -1;

            internal int NewPosition
            {
                get { return _NewPosition; }
                set { _NewPosition = value; }
            }

            internal int OldPosition
            {
                get { return _OldPosition; }
                set { _OldPosition = value; }
            }
        }

        private TabEventArgs CreateTabArgs(float newPos)
        {
            TabEventArgs tae = new TabEventArgs();
            tae.NewPosition = (int)(newPos / dotsPermm);
            tae.OldPosition = pos;
            return tae;
        }

        #endregion

        #region Overrides

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int i = 0;

            mCaptured = false;

            //process left mouse button only
            if (e.Button != MouseButtons.Left)
                return;

            for (i = 0; i <= 6; i++)
            {
                if (items[i].Contains(e.Location) == true && i != 3) //i must not be equal to 3, as this is region for whole image
                {
                    if (noMargins == true && (i == 0 || i == 1))
                        break;

                    capObject = i;
                    mCaptured = true;
                    break;
                }
            }

            if (mCaptured == true)
                return;

            if (tabs.Count == 0)
                return;

            if (_tabsEnabled == false)
                return;

            i = 0;

            for (i = 0; i <= tabs.Count - 1; i++)
            {
                if (tabs[i].Contains(e.Location) == true)
                {
                    capTab = i;
                    pos = (int)(tabs[i].X / dotsPermm);
                    mCaptured = true;
                    break;
                }
            }

        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mCaptured == true && capObject != -1)
            {
                switch (capObject)
                {
                    case 0:
                        if (noMargins == true)
                            return;
                        if (e.Location.X <= (int)(me.Width - rMargin * dotsPermm - 35f))
                        {
                            this.lMargin = (int)(e.Location.X / dotsPermm);
                            if (this.lMargin < 1)
                                this.lMargin = 1;
                            if (LeftMarginChanging != null)
                                LeftMarginChanging.Invoke(lMargin);
                            this.Refresh();
                        }
                        break;

                    case 1:
                        if (noMargins == true)
                            return;
                        if (e.Location.X >= (int)(lMargin * dotsPermm + 35f))
                        {
                            this.rMargin = (int)((drawZone.Width / dotsPermm) - (int)(e.Location.X / dotsPermm));
                            if (this.rMargin < 1)
                                this.rMargin = 1;
                            if (RightMarginChanging != null)
                                RightMarginChanging.Invoke(rMargin);
                            this.Refresh();
                        }
                        break;

                    case 2:
                        if (e.Location.X <= (int)(me.Width - rIndent * dotsPermm - 35f))
                        {
                            this.luIndent = (int)(e.Location.X / dotsPermm);
                            if (this.luIndent < 1)
                                this.luIndent = 1;
                            if (LeftIndentChanging != null)
                                LeftIndentChanging.Invoke(luIndent - 1);
                            this.Refresh();
                        }
                        break;


                    case 4:
                        if (e.Location.X >= (int)(Math.Max(llIndent, luIndent) * dotsPermm + 35f))
                        {
                            this.rIndent = (int)((me.Width / dotsPermm) - (int)(e.Location.X / dotsPermm));
                            if (this.rIndent < 1)
                                this.rIndent = 1;
                            if (RightIndentChanging != null)
                                RightIndentChanging.Invoke(rIndent - 1);
                            this.Refresh();
                        }
                        break;

                    case 5:
                        if (e.Location.X <= (int)(drawZone.Width - rIndent * dotsPermm - 35f))
                        {
                            this.llIndent = (int)(e.Location.X / dotsPermm);
                            if (this.llIndent < 1)
                                this.llIndent = 1;
                            if (LeftHangingIndentChanging != null)
                                LeftHangingIndentChanging.Invoke(llIndent - 1);
                            this.Refresh();
                        }
                        break;

                    case 6:
                        if (e.Location.X <= (int)(drawZone.Width - rIndent * dotsPermm - 35f))
                        {
                            this.luIndent = luIndent + (int)(e.Location.X / dotsPermm) - llIndent;
                            this.llIndent = (int)(e.Location.X / dotsPermm);
                            if (this.llIndent < 1)
                                this.llIndent = 1;
                            if (this.luIndent < 1)
                                this.luIndent = 1;
                            if (BothLeftIndentsChanged != null)
                                BothLeftIndentsChanged.Invoke(luIndent - 1, llIndent - 1);
                            this.Refresh();
                        }
                        break;

                }
            }
            else if (mCaptured == true && capTab != -1)
            {
                if (workArea.Contains(e.Location) == true)
                {
                    tabs[capTab] = new RectangleF((float)e.Location.X, tabs[capTab].Y, tabs[capTab].Width, tabs[capTab].Height);
                    this.Refresh();
                }
            }
            else
            {
                int i = 0;

                for (i = 0; i <= 4; i++)
                {
                    if (items[i].Contains(e.Location) == true)
                    {
                        switch (i)
                        {
                            case 0:
                                if (noMargins == true) return;
                                Cursor = Cursors.SizeWE;
                                break;

                            case 1:
                                if (noMargins == true) return;
                                Cursor = Cursors.SizeWE;
                                break;
                        }
                        break;
                    }
                    this.Cursor = Cursors.Default;
                }
            }

        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            //process left mouse button only
            if (e.Button != MouseButtons.Left)
                return;

            if (workArea.Contains(e.Location) == false)
            {
                if (mCaptured == true && capTab != -1 && _tabsEnabled == true)
                {
                    try
                    {
                        float pos = tabs[capTab].X * dotsPermm;
                        tabs.RemoveAt(capTab);
                        if (TabRemoved != null)
                            TabRemoved.Invoke(CreateTabArgs(pos));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            else if (workArea.Contains(e.Location) == true)
            {
                if (mCaptured != true && _tabsEnabled == true)
                {
                    AddTab((float)e.Location.X);
                }
                else if (mCaptured == true && capTab != -1)
                {
                    if (TabChanged != null && _tabsEnabled == true)
                        TabChanged.Invoke(CreateTabArgs(e.Location.X));
                }
            }

            capTab = -1;
            mCaptured = false;
            capObject = -1;
            this.Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            //set compositing to high quality because of using images for indents
            e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //make smooth control
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            //this will braw smoother control, without blur and fast ;).
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            //this will braw text with highest quality
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            me = new RectangleF(0f, 0f, (float)this.Width, (float)this.Height);
            drawZone = new RectangleF(1f, 3f, me.Width - 2f, 14f);
            workArea = new RectangleF((float)lMargin * dotsPermm, 3f, drawZone.Width - ((float)rMargin * dotsPermm) - drawZone.X * 2, 14f);

            //firstly, draw background.
            DrawBackground(e.Graphics);

            //then, draw margins
            DrawMargins(e.Graphics);

            //then, draw text (numbers) and marks (vertical lines)
            DrawTextAndMarks(e.Graphics);

            //then, draw indent marks
            DrawIndents(e.Graphics);

            //finally, draw tab stops
            DrawTabs(e.Graphics);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Height = 20;
        }

        #endregion

        #region Painting

        private void DrawBackground(Graphics g)
        {
            //clear background
            p.Color = Color.Transparent;
            g.FillRectangle(p.Brush, me);

            //fill background
            p.Color = _baseColor;
            g.FillRectangle(p.Brush, drawZone);
        }

        private void DrawIndents(Graphics g)
        {
            items[2] = new RectangleF((float)luIndent * dotsPermm - 4.5f, 0f, 9f, 8f);
            items[3] = new RectangleF((float)llIndent * dotsPermm - 4.5f, 8.2f, 9f, 11.8f);
            items[4] = new RectangleF((float)(drawZone.Width - ((float)rIndent * dotsPermm - 4.5f) - 7f), 11f, 9f, 8f);

            //regions for moving left indentation marks
            items[5] = new RectangleF((float)llIndent * dotsPermm - 4.5f, 8.2f, 9f, 5.9f);
            items[6] = new RectangleF((float)llIndent * dotsPermm - 4.5f, 14.1f, 9f, 5.9f);

            g.DrawImage(Properties.Resources.l_indet_pos_upper, items[2]);
            g.DrawImage(Properties.Resources.l_indent_pos_lower, items[3]);
            g.DrawImage(Properties.Resources.r_indent_pos, items[4]);
        }

        private void DrawMargins(Graphics g)
        {
            items[0] = new RectangleF(0f, 3f, lMargin * dotsPermm, 14f);
            items[1] = new RectangleF(drawZone.Width - ((float)rMargin * dotsPermm) + 1f, 3f, rMargin * dotsPermm + 5f, 14f);
            p.Color = Color.DarkGray;
            g.FillRectangle(p.Brush, items[0]);
            g.FillRectangle(p.Brush, items[1]);

            g.PixelOffsetMode = PixelOffsetMode.None;
            //draw border
            p.Color = _strokeColor;
            g.DrawRectangle(p, 0, 3, me.Width - 1, 14);
        }

        private void DrawTabs(Graphics g)
        {
            if (_tabsEnabled == false)
                return;

            int i = 0;

            if (tabs.Count == 0)
                return;

            for (i = 0; i <= tabs.Count - 1; i++)
            {
                g.DrawImage(Properties.Resources.tab_pos, tabs[i]);
            }
        }

        private void DrawTextAndMarks(Graphics g)
        {
            int points = (int)(drawZone.Width / dotsPermm) / 10;
            float range = 5 * dotsPermm;
            int i = 0;
            p.Color = Color.Black;
            SizeF sz;
            for (i = 0; i <= points * 2 + 1; i++)
            {
                if (i % 2 == 0 && i != 0)
                {
                    sz = g.MeasureString((Convert.ToInt32(i / 2)).ToString(), this.Font);
                    g.DrawString((Convert.ToInt32(i / 2)).ToString(), this.Font, p.Brush, new PointF((float)(i * range - (float)(sz.Width / 2)), (float)(me.Height / 2) - (float)(sz.Height / 2)));
                }
                else
                {
                    g.DrawLine(p, (float)(i * range), 7f, (float)(i * range), 12f);
                }
            }
            g.PixelOffsetMode = PixelOffsetMode.Half;
        }

        #endregion

    }
}
