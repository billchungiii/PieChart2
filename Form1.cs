using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PieChart2
{
    public partial class Form1 : Form
    {
        private List<PieItem> _pieItems;
        public Form1()
        {
            InitializeComponent();
            //// 啟用雙緩衝繪圖，減少閃爍和殘影
            //this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
            //              ControlStyles.AllPaintingInWmPaint |
            //              ControlStyles.UserPaint, true);

            //// 也為panel2啟用雙緩衝
            //panel2.SetStyle(ControlStyles.OptimizedDoubleBuffer |
            //              ControlStyles.AllPaintingInWmPaint |
            //              ControlStyles.UserPaint, true);
            this.Resize += Form1_Resize;
            this.panel2.Paint += Panel2_Paint;
            // 初始化示例數據
            InitializeData();
        }

        private void Panel2_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(panel2.BackColor);
            DrawPieChart(e.Graphics);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            panel2.Invalidate();
        }

        private void InitializeData()
        {
            // 示例數據，與您提供的圖片類似
            _pieItems = new List<PieItem>
            {
                new PieItem { Name = "元大", Value = 28.31, Color = Color.Purple },
                new PieItem { Name = "第一金", Value = 12.93, Color = Color.LightGreen },
                new PieItem { Name = "中國信託", Value = 1.82, Color = Color.Cyan },
                new PieItem { Name = "國泰", Value = 2.35, Color = Color.DarkCyan },
                new PieItem { Name = "富邦", Value = 4.72, Color = Color.Coral },
                new PieItem { Name = "國票", Value = 5.67, Color = Color.Gray },
                new PieItem { Name = "統一", Value = 7.65, Color = Color.Brown },
                new PieItem { Name = "凱基", Value = 10.48, Color = Color.Red },
                new PieItem { Name = "永豐", Value = 7.87, Color = Color.Orange },
                new PieItem { Name = "富邦金", Value = 4.2, Color = Color.Pink },
                new PieItem { Name = "兆豐", Value = 15.07, Color = Color.DarkBlue }
            };

            // 初次繪製圖表
            panel2.Invalidate();
        }

        private void DrawPieChart(Graphics g)
        {
            if (_pieItems == null || _pieItems.Count == 0)
                return;

            // 設置高品質繪圖模式
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            // 為標籤和連接線預留足夠的邊距
            int margin = 80;

            // 計算圓餅圖的有效繪製區域大小
            int effectiveWidth = panel2.Width - 2 * margin;
            int effectiveHeight = panel2.Height - 2 * margin;

            // 選擇較小的尺寸作為圓餅圖的直徑
            int diameter = Math.Min(effectiveWidth, effectiveHeight);

            if (diameter < 100) // 如果尺寸太小，不繪製
                return;

            // 計算圓餅圖的半徑
            int radius = diameter / 2;

            // 計算圓餅圖的中心位置
            int centerX = panel2.Width / 2;
            int centerY = panel2.Height / 2;

            // 計算圓餅圖的矩形邊界
            Rectangle pieRect = new Rectangle(
                centerX - radius,
                centerY - radius,
                diameter,
                diameter);

            // 計算總量
            double total = _pieItems.Sum(item => item.Value);

            // 準備左右兩側的標籤列表
            List<LabelInfo> leftLabels = new List<LabelInfo>();
            List<LabelInfo> rightLabels = new List<LabelInfo>();

            // 繪製圓餅圖
            float startAngle = 0;
            float sweepAngle = 0;

            foreach (var item in _pieItems)
            {
                sweepAngle = (float)(item.Value / total * 360);
                using (SolidBrush brush = new SolidBrush(item.Color))
                {
                    g.FillPie(brush, pieRect, startAngle, sweepAngle);
                    using (Pen whitePen = new Pen(Color.White, 1.5f))
                    {
                        g.DrawPie(whitePen, pieRect, startAngle, sweepAngle);
                    }
                }

                // 計算扇形區域的中心角
                double midAngle = Math.PI * (startAngle + sweepAngle / 2) / 180.0;

                // 計算連接線的起點（精確到圓餅圖邊緣）
                float lineStartX = centerX + (float)(Math.Cos(midAngle) * radius);
                float lineStartY = centerY + (float)(Math.Sin(midAngle) * radius);

                // 創建標籤信息對象
                LabelInfo labelInfo = new LabelInfo
                {
                    Item = item,
                    StartAngle = startAngle,
                    SweepAngle = sweepAngle,
                    MidAngle = midAngle,
                    LineStartX = lineStartX,
                    LineStartY = lineStartY,
                    Percentage = (item.Value / total) * 100
                };

                // 根據角度決定標籤放在左側還是右側
                if (midAngle > Math.PI / 2 && midAngle < Math.PI * 3 / 2)
                {
                    leftLabels.Add(labelInfo);
                }
                else
                {
                    rightLabels.Add(labelInfo);
                }

                startAngle += sweepAngle;
            }

            // 繪製左側標籤
            DrawSideLabels(g, leftLabels, centerX, centerY, radius, margin, true);

            // 繪製右側標籤
            DrawSideLabels(g, rightLabels, centerX, centerY, radius, margin, false);
        }

        // 繪製一側的標籤與連接線
        private void DrawSideLabels(Graphics g, List<LabelInfo> labels, int centerX, int centerY,
            int radius, int margin, bool isLeftSide)
        {
            if (labels.Count == 0)
                return;

            // 排序標籤，從上到下
            labels = labels.OrderBy(l => l.LineStartY).ToList();

            // 最小標籤間距
            float minLabelGap = 25;

            // 標籤區域的邊界
            float labelAreaStart;
            float labelAreaEnd;

            // 設定標籤區域
            if (isLeftSide)
            {
                labelAreaStart = margin;
                labelAreaEnd = centerX - radius - 20; // 左側標籤區域結束位置
            }
            else
            {
                labelAreaStart = centerX + radius + 20; // 右側標籤區域開始位置
                labelAreaEnd = panel2.Width - margin;
            }

            // 為每個標籤找到合適的位置
            for (int i = 0; i < labels.Count; i++)
            {
                LabelInfo label = labels[i];

                // 使用線的起始Y座標（圓餅圖邊緣點）作為標籤Y座標
                float labelY = label.LineStartY;

                // 確保標籤不會超出面板上下邊界
                labelY = Math.Max(margin + 10, labelY);
                labelY = Math.Min(panel2.Height - margin - 10, labelY);

                // 檢查與前面標籤的間距
                if (i > 0)
                {
                    float prevLabelY = labels[i - 1].LabelY; // 前一個標籤的Y坐標
                    if (labelY - prevLabelY < minLabelGap) // 如果間距太小
                    {
                        labelY = prevLabelY + minLabelGap; // 增加間距
                    }
                }

                // 保存實際使用的Y座標
                label.LabelY = labelY;

                // 繪製標籤
                using (Font labelFont = new Font("微軟正黑體", 11f, FontStyle.Regular, GraphicsUnit.Point, 0))
                {
                    string labelText = $"{label.Item.Name}({label.Percentage:N2}%)";
                    SizeF labelSize = g.MeasureString(labelText, labelFont);

                    // 確定文字水平位置
                    float textX;

                    if (isLeftSide)
                    {
                        // 左側標籤靠左對齊
                        textX = labelAreaStart;
                    }
                    else
                    {
                        // 右側標籤靠右對齊
                        textX = labelAreaEnd - labelSize.Width;
                    }

                    // 確保文字垂直居中
                    float textY = labelY - labelSize.Height / 2;

                    // 計算線條終點 - 調整為符合要求
                    float lineEndX;
                    if (isLeftSide)
                    {
                        // 左側標籤：線條延伸到標籤文字的最後一個字的右邊
                        lineEndX = textX + labelSize.Width + 3; // 加3像素的間距
                    }
                    else
                    {
                        // 右側標籤：線條延伸到標籤文字的第一個字的左邊
                        lineEndX = textX - 3; // 減3像素的間距
                    }

                    // 繪製標籤文字（先繪製文字，以便線條覆蓋在文字上方）
                    g.DrawString(labelText, labelFont, Brushes.Black, textX, textY);

                    // 繪製標籤線條
                    using (Pen linePen = new Pen(label.Item.Color, 1.2f))
                    {
                        if (Math.Abs(labelY - label.LineStartY) < 2)
                        {
                            // 如果標籤Y座標與線起點Y座標非常接近，直接畫一條水平線
                            g.DrawLine(linePen, label.LineStartX, label.LineStartY, lineEndX, labelY);
                        }
                        else
                        {
                            // 否則畫一條折線
                            // 第一段：從圓餅圖邊緣水平延伸
                            float horizontalExtend;

                            if (isLeftSide)
                            {
                                horizontalExtend = Math.Min(label.LineStartX - 15, (label.LineStartX + lineEndX) / 2);
                            }
                            else
                            {
                                horizontalExtend = Math.Max(label.LineStartX + 15, (label.LineStartX + lineEndX) / 2);
                            }

                            // 第一段：從圓餅圖邊緣向外水平延伸
                            g.DrawLine(linePen, label.LineStartX, label.LineStartY, horizontalExtend, label.LineStartY);

                            // 第二段：垂直線段
                            g.DrawLine(linePen, horizontalExtend, label.LineStartY, horizontalExtend, labelY);

                            // 第三段：水平延伸到標籤附近
                            g.DrawLine(linePen, horizontalExtend, labelY, lineEndX, labelY);
                        }
                    }
                }
            }
        }
    }

    // 標籤信息類
    public class LabelInfo
    {
        public PieItem Item { get; set; }
        public float StartAngle { get; set; }
        public float SweepAngle { get; set; }
        public double MidAngle { get; set; }
        public float LineStartX { get; set; }
        public float LineStartY { get; set; }
        public double Percentage { get; set; }
        public float LabelY { get; set; } // 用於保存實際使用的Y座標
    }

    public class PieItem
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public Color Color { get; set; }
    }
}
