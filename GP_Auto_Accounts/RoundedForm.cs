using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Windows.Forms;

public class SmoothRoundedForm : Form
{
    private const int CornerRadius = 25;
    private bool _isDragging;
    private Point _dragStartPosition;

    public SmoothRoundedForm()
    {
        // Only apply custom styles at runtime
        if (!IsDesignMode)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.AllPaintingInWmPaint, true);
        }

        // Always apply these (designer-friendly)
        this.BackColor = Color.FromArgb(240, 240, 240);
        this.Padding = new Padding(10);
    }

    // ========================
    // 1. DESIGNER COMPATIBILITY
    // ========================
    private bool IsDesignMode
    {
        get
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
                   DesignMode ||
                   System.Diagnostics.Process.GetCurrentProcess().ProcessName == "devenv";
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (IsDesignMode)
        {
            // Show design-time visualization
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                Color.DarkGray, ButtonBorderStyle.Dashed);
            return;
        }

        // ========================
        // 2. RUNTIME RENDERING
        // ========================
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;


        using (var path = GetRoundedPath(ClientRectangle, CornerRadius))
        {
            // Fill background
            using (var brush = new SolidBrush(BackColor))
                e.Graphics.FillPath(brush, path);

            // Draw smooth border
            using (var pen = new Pen(Color.FromArgb(120, 120, 120), 5f))  // Thicker pen for a more solid border
            {
                pen.Alignment = PenAlignment.Center;  // Change PenAlignment to 'Center' for a smoother border
                e.Graphics.DrawPath(pen, path);
            }
        }
    }

    private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        radius = Math.Min(radius, Math.Min(rect.Width, rect.Height));

        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();

        return path;
    }

    // ========================
    // 3. FORM FUNCTIONALITY
    // ========================
    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (!IsDesignMode) UpdateRegion();
    }

    private void UpdateRegion()
    {
        using (var path = GetRoundedPath(ClientRectangle, CornerRadius))
        {
            var newRegion = new Region(path);
            this.Region?.Dispose();
            this.Region = newRegion;
        }
    }

    // Dragging implementation
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _isDragging = true;
            _dragStartPosition = e.Location;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _isDragging = false;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_isDragging)
        {
            Point diff = Point.Subtract(e.Location, new Size(_dragStartPosition));
            this.Location = Point.Add(this.Location, new Size(diff));
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && this.Region != null)
        {
            this.Region.Dispose();
            this.Region = null;
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
            this.SuspendLayout();
            // 
            // SmoothRoundedForm
            // 
            this.ClientSize = new System.Drawing.Size(555, 449);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SmoothRoundedForm";
            this.Load += new System.EventHandler(this.SmoothRoundedForm_Load);
            this.ResumeLayout(false);

    }

    private void SmoothRoundedForm_Load(object sender, EventArgs e)
    {

    }
}