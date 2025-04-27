using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using System.Drawing;
using System.Drawing.Imaging;

using Plotly.NET;
using Plotly.NET.LayoutObjects;
using static Plotly.NET.GenericChart;
using Plotly.NET.ImageExport;
using System.Linq;
using OxyPlot;
using OxyPlot.WindowsForms;
using OxyPlot.Series;
using static System.Windows.Forms.LinkLabel;
using OxyPlot.Axes;

namespace PlantInspector
{

    public class Bars
    {
        // Constraints
        public static double LEFT_MARGIN;       // margins around plot (relative to the image's sizes)
        public static double TOP_MARGIN;
        public static double RIGHT_MARGIN;
        public static double BOTTOM_MARGIN;
        public static double BOTTOM2_MARGIN;
        public static double CLASS_NAMES;
        public static double DAS_START;         // left position of first DAS value


        // Constraints and parameters
        List<System.Drawing.Color> bar_colors = new List<System.Drawing.Color>();


        //------------------------------------------------------------------------------
        public Bars()
        {
            // set constraints
            LEFT_MARGIN = 0.12;
            TOP_MARGIN = 0.0279;
            RIGHT_MARGIN = 0.92;
            BOTTOM_MARGIN = 0.8982;
            BOTTOM2_MARGIN = 0.84;
            CLASS_NAMES = 0.75;
            DAS_START = 0.13;

            // create colors for repetitions (used at all plots)
            bar_colors.Add(System.Drawing.Color.FromArgb(091, 155, 213));
            bar_colors.Add(System.Drawing.Color.FromArgb(237, 125, 049));
            bar_colors.Add(System.Drawing.Color.FromArgb(165, 165, 165));
            bar_colors.Add(System.Drawing.Color.FromArgb(255, 192, 000));
            bar_colors.Add(System.Drawing.Color.FromArgb(068, 114, 196));
            bar_colors.Add(System.Drawing.Color.FromArgb(112, 173, 071));
            bar_colors.Add(System.Drawing.Color.FromArgb(037, 094, 146));
            bar_colors.Add(System.Drawing.Color.FromArgb(158, 072, 014));
            bar_colors.Add(System.Drawing.Color.FromArgb(099, 099, 099));
            bar_colors.Add(System.Drawing.Color.FromArgb(153, 115, 000));
            bar_colors.Add(System.Drawing.Color.FromArgb(038, 068, 120));
            bar_colors.Add(System.Drawing.Color.FromArgb(067, 104, 043));


        }

        public void Create_Growth_Rate_Bar(string classname, string bar_filename, string bar_title, int DAS_min, int DAS_max, double[] avg_values, int barimagewidth, int barimageheight)
        {
            // create bitmap for drawing the bar
            Bitmap bar_bmp = new Bitmap(barimagewidth, barimageheight, PixelFormat.Format24bppRgb);
            if (bar_bmp == null)
                return;
            Graphics g = Graphics.FromImage(bar_bmp);

            List<double> avg_data = new List<double>();
            for (int i = 0; i < avg_values.Length - 1; i++)
            {
                if (avg_values[i] != 0.0 && avg_values[i + 1] != 0.0)
                    avg_data.Add(avg_values[i + 1] / avg_values[i]);
            }

            // fill the whole image with 'white' (background)
            System.Drawing.Color canvas_color = System.Drawing.Color.White;
            SolidBrush canvas_brush = new SolidBrush(canvas_color);
            g.FillRectangle(canvas_brush, 0, 0, barimagewidth, barimageheight);
            canvas_brush.Dispose();

            // draw thin border around the whole bar
            Pen grid_pen = new Pen(System.Drawing.Color.LightGray, 1);
            g.DrawLine(grid_pen, 0, 0, barimagewidth - 1, 0);
            g.DrawLine(grid_pen, barimagewidth - 1, 0, barimagewidth - 1, barimageheight - 1);
            g.DrawLine(grid_pen, barimagewidth - 1, barimageheight - 1, 0, barimageheight - 1);
            g.DrawLine(grid_pen, 0, barimageheight - 1, 0, 0);

            int leftmargin = (int)(LEFT_MARGIN * (double)barimagewidth + 0.5);
            int topmargin = (int)(TOP_MARGIN * (double)barimageheight + 0.5);
            int rightmargin = (int)(RIGHT_MARGIN * (double)barimagewidth + 0.5);
            int bottommargin = (int)(BOTTOM_MARGIN * (double)barimageheight + 0.5);
            int bottom2margin = (int)(BOTTOM2_MARGIN * (double)barimageheight + 0.5);
            SolidBrush title_brush = new SolidBrush(System.Drawing.Color.Black);
            Pen margin_pen = new Pen(System.Drawing.Color.Gray, 2);
            int font_size = (barimageheight - bottommargin) / 3;
            System.Drawing.Font title_font = new System.Drawing.Font(FontFamily.GenericSerif, font_size, FontStyle.Bold, GraphicsUnit.Pixel);

            System.Drawing.Font scale_font = new System.Drawing.Font(FontFamily.GenericSerif, font_size, FontStyle.Bold, GraphicsUnit.Pixel);
            var format = new StringFormat() { Alignment = StringAlignment.Far };

            int DASstart = (int)(DAS_START * (double)barimagewidth + 0.5);
            double DASstep = (double)(rightmargin - DASstart - 10) / (double)(DAS_max - DAS_min);

            // draw bar title at the bottom
            g.DrawString(bar_title, title_font, title_brush, barimagewidth / 4, bottommargin + 2 * font_size - 6);

            // get the max. growth rate
            double max_Growth_Rate = 0.0;
            for (int i = 0; i < avg_data.Count; i++)
            {
                if (avg_data[i] > max_Growth_Rate)
                    max_Growth_Rate = avg_data[i];
            }
            max_Growth_Rate = 100.0 * (max_Growth_Rate - 1.0);

            // draw % scale at the left
            double step_on_graph = (double)(bottom2margin - topmargin) / 12.0;
            double value_step = 0.0;
            if (max_Growth_Rate < 15)
                value_step = 1.5;
            else if (max_Growth_Rate < 20)
                value_step = 2.0;
            else if (max_Growth_Rate < 25)
                value_step = 2.5;
            else if (max_Growth_Rate < 30)
                value_step = 3.0;
            else if (max_Growth_Rate < 35)
                value_step = 3.5;
            else if (max_Growth_Rate < 40)
                value_step = 4.0;
            else if (max_Growth_Rate < 45)
                value_step = 4.5;
            else if (max_Growth_Rate < 50)
                value_step = 5.0;
            else if (max_Growth_Rate < 55)
                value_step = 5.5;
            else if (max_Growth_Rate < 60)
                value_step = 6.0;
            else if (max_Growth_Rate < 65)
                value_step = 6.5;
            else if (max_Growth_Rate < 70)
                value_step = 7.0;
            else
                value_step = 7.5;

            int y = bottom2margin;
            double value = 0.0;
            int step_nb = 0;
            while (y > topmargin)
            {
                g.DrawLine(margin_pen, leftmargin, y, rightmargin, y);
                string value_str = Convert.ToString(value) + " %";
                g.DrawString(value_str, scale_font, title_brush, leftmargin - 25, y - font_size / 2, format);
                value += value_step;
                step_nb++;
                y = (int)(bottom2margin - (double)step_nb * step_on_graph);
            }

            // draw the bars with DAS values on abscissa
            int colorindex = 0;
            for (int i = 0; i < avg_data.Count; i++)
            {
                // draw the bar
                if (colorindex > (bar_colors.Count - 1))
                    colorindex -= bar_colors.Count;
                double current_value = 100.0 * (avg_data[i] - 1.0);
                int bar_height = (int)(current_value * step_on_graph / value_step + 0.5);
                SolidBrush bar_brush = new SolidBrush(bar_colors[colorindex]);
                g.FillRectangle(bar_brush, DASstart + (int)((i + 0.25) * DASstep), bottom2margin - bar_height, (int)(DASstep / 2.0), bar_height);
                bar_brush.Dispose();
                colorindex++;

                // draw the correspondent DAS value
                if ((DASstart + i) == (DASstart + i) / 2 * 2)
                {
                    // only even values are displayed
                    string DAS_str = Convert.ToString(DAS_min + i);
                    g.DrawString(DAS_str, title_font, Brushes.Black, DASstart + (int)((i + 0.5) * DASstep) - 2 * title_font.Size / 3, bottom2margin + 10);
                }
            }

            // draw box for 'Title' for the whole bar (name of class)
            int Bar_title_Width = 140;
            int Bar_title_Height = 40;
            g.DrawLine(margin_pen, barimagewidth / 3, topmargin, barimagewidth / 3 + Bar_title_Width, topmargin);
            g.DrawLine(margin_pen, barimagewidth / 3 + Bar_title_Width, topmargin, barimagewidth / 3 + Bar_title_Width, topmargin + Bar_title_Height);
            g.DrawLine(margin_pen, barimagewidth / 3, topmargin + Bar_title_Height, barimagewidth / 3 + Bar_title_Width, topmargin + Bar_title_Height);
            g.DrawLine(margin_pen, barimagewidth / 3, topmargin + Bar_title_Height, barimagewidth / 3, topmargin);
            g.DrawString(classname, title_font, title_brush, barimagewidth / 3 + Bar_title_Width / 2 - 6 * classname.Length, topmargin + 6);


            // save the bar and finish
            g.Dispose();
            bar_bmp.Save(bar_filename, System.Drawing.Imaging.ImageFormat.Png);
            grid_pen.Dispose();
            title_brush.Dispose();
            margin_pen.Dispose();
            title_font.Dispose();
            scale_font.Dispose();
        }

        public void Create_Growth_Rate_Bar2(string classname, string bar_filename, string x_axis_label, string y_axis_label, int DAS_min, int DAS_max, double[] growth_rate_values, int barimagewidth, int barimageheight)
        {
            // create bitmap for drawing the bar
            Bitmap bar_bmp = new Bitmap(barimagewidth, barimageheight, PixelFormat.Format24bppRgb);
            if (bar_bmp == null)
                return;
            Graphics g = Graphics.FromImage(bar_bmp);

            List<double> growth_data_list = new List<double>();
            for ( int i=0; i< growth_rate_values.Length-1; i++)
            {
                growth_data_list.Add(growth_rate_values[i]);
            }

            // fill the whole image with 'white' (background)
            System.Drawing.Color canvas_color = System.Drawing.Color.White;
            SolidBrush canvas_brush = new SolidBrush(canvas_color);
            g.FillRectangle(canvas_brush, 0, 0, barimagewidth, barimageheight);
            canvas_brush.Dispose();

            // draw thin border around the whole bar
            Pen grid_pen = new Pen(System.Drawing.Color.LightGray, 1);
            g.DrawLine(grid_pen, 0, 0, barimagewidth - 1, 0);
            g.DrawLine(grid_pen, barimagewidth - 1, 0, barimagewidth - 1, barimageheight - 1);
            g.DrawLine(grid_pen, barimagewidth - 1, barimageheight - 1, 0, barimageheight - 1);
            g.DrawLine(grid_pen, 0, barimageheight - 1, 0, 0);

            int leftmargin = (int)(LEFT_MARGIN * (double)barimagewidth + 0.5);
            int topmargin = (int)(TOP_MARGIN * (double)barimageheight + 0.5);
            int rightmargin = (int)(RIGHT_MARGIN * (double)barimagewidth + 0.5);
            int bottommargin = (int)(BOTTOM_MARGIN * (double)barimageheight + 0.5);
            int bottom2margin = (int)(BOTTOM2_MARGIN * (double)barimageheight + 0.5);
            SolidBrush title_brush = new SolidBrush(System.Drawing.Color.Black);
            Pen margin_pen = new Pen(System.Drawing.Color.Gray, 2);
            int font_size = (barimageheight - bottommargin) / 3;
            System.Drawing.Font title_font = new System.Drawing.Font(FontFamily.GenericSerif, font_size, FontStyle.Bold, GraphicsUnit.Pixel);

            System.Drawing.Font scale_font = new System.Drawing.Font(FontFamily.GenericSerif, font_size, FontStyle.Bold, GraphicsUnit.Pixel);
            var format = new StringFormat() { Alignment = StringAlignment.Far };

            int DASstart = (int)(DAS_START * (double)barimagewidth + 0.5);
            double DASstep = (double)(rightmargin - DASstart - 10) / (double)(DAS_max - DAS_min);

            // draw Y axis label at the left; we need to do some translations and a 180 degrees rotation
            StringFormat titleformat = new StringFormat();
            titleformat.Alignment = StringAlignment.Center;
            SizeF vis_win_sz = g.VisibleClipBounds.Size;
            g.TranslateTransform(vis_win_sz.Width, 0);
            g.RotateTransform(270);
            g.DrawString(y_axis_label, title_font, title_brush, new RectangleF(-vis_win_sz.Height, -vis_win_sz.Width + 7, vis_win_sz.Height, vis_win_sz.Width), titleformat);
            g.ResetTransform();

            // draw X axis label at the bottom
            g.DrawString(x_axis_label, title_font, title_brush, new RectangleF(0, vis_win_sz.Height - 50, vis_win_sz.Width, 50), titleformat);

            // get the max. growth rate
            double max_Growth_Rate = 0.0;
            for (int i = 0; i < growth_data_list.Count; i++)
            {
                if (growth_data_list[i] > max_Growth_Rate)
                    max_Growth_Rate = growth_data_list[i];
            }
            max_Growth_Rate *= 100.0;

            // draw % scale at the left
            double step_on_graph = (double)(bottom2margin - topmargin) / 12.0;
            double value_step = 0.0;
            if (max_Growth_Rate < 15)
                value_step = 1.5;
            else if (max_Growth_Rate < 20)
                value_step = 2.0;
            else if (max_Growth_Rate < 25)
                value_step = 2.5;
            else if (max_Growth_Rate < 30)
                value_step = 3.0;
            else if (max_Growth_Rate < 35)
                value_step = 3.5;
            else if (max_Growth_Rate < 40)
                value_step = 4.0;
            else if (max_Growth_Rate < 45)
                value_step = 4.5;
            else if (max_Growth_Rate < 50)
                value_step = 5.0;
            else if (max_Growth_Rate < 55)
                value_step = 5.5;
            else if (max_Growth_Rate < 60)
                value_step = 6.0;
            else if (max_Growth_Rate < 65)
                value_step = 6.5;
            else if (max_Growth_Rate < 70)
                value_step = 7.0;
            else
                value_step = 7.5;

            int y = bottom2margin;
            double value = 0.0;
            int step_nb = 0;
            while (y > topmargin)
            {
                g.DrawLine(margin_pen, leftmargin, y, rightmargin, y);
                string value_str = Convert.ToString(value) + " %";
                g.DrawString(value_str, scale_font, title_brush, leftmargin - 15, y - font_size / 2, format);
                value += value_step;
                step_nb++;
                y = (int)(bottom2margin - (double)step_nb * step_on_graph);
            }

            // draw the bars with DAS values on abscissa
            int colorindex = 0;
            for (int i = 0; i < growth_data_list.Count; i++)
            {
                // draw the bar
                if (colorindex > (bar_colors.Count - 1))
                    colorindex -= bar_colors.Count;
                double current_value = 100.0 * growth_data_list[i];
                int bar_height = (int)(current_value * step_on_graph / value_step + 0.5);
                SolidBrush bar_brush = new SolidBrush(bar_colors[colorindex]);
                g.FillRectangle(bar_brush, DASstart + (int)((i + 0.25) * DASstep), bottom2margin - bar_height, (int)(DASstep / 2.0), bar_height);
                bar_brush.Dispose();
                colorindex++;

                // draw the correspondent DAS value
                if ((DASstart+i) == (DASstart + i) / 2 * 2)
                {
                    // only even values are displayed
                    string DAS_str = Convert.ToString(DAS_min + i);
                    g.DrawString(DAS_str, title_font, Brushes.Black, DASstart + (int)((i + 0.5) * DASstep) - 2*title_font.Size/3, bottom2margin + 10);
                }
            }

            // draw box for 'Title' for the whole bar (name of class)
            SizeF txt_sz = g.MeasureString(classname, title_font);
            RectangleF title_rectangle = new RectangleF(
                (vis_win_sz.Width - txt_sz.Width) / 2.0F,
                5.0F,
                txt_sz.Width,
                txt_sz.Height);
            g.DrawString(classname, title_font, title_brush, title_rectangle, titleformat);
            g.DrawRectangle(margin_pen, Rectangle.Round(title_rectangle));


            // save the bar and finish
            g.Dispose();
            bar_bmp.Save(bar_filename, System.Drawing.Imaging.ImageFormat.Png);
            grid_pen.Dispose();
            title_brush.Dispose();
            margin_pen.Dispose();
            title_font.Dispose();
            scale_font.Dispose();
        }

        public void Create_Growth_Rate_Bar__Oxyplot(string classname, string bar_filename, string x_axis_label, string y_axis_label,
            int DAS_min, int DAS_max, double[] growth_rate_values, int barimagewidth, int barimageheight)
        {
            var bar_model = new PlotModel()
            {
                Title = classname,
                TitleFont = System.Drawing.FontFamily.GenericSerif.Name,
                TitleFontWeight = FontWeights.Bold,
                Background = OxyColor.FromRgb(255, 255, 255),
            };

            // specify key and position
            var categoryAxis = new OxyPlot.Axes.CategoryAxis()
            {
                Position = AxisPosition.Bottom,
                //Key = "Category",
                Title = x_axis_label,
                TitleFont = System.Drawing.FontFamily.GenericSerif.Name,
                TitleFontSize = 18,
                TitleFontWeight = FontWeights.Bold,
                AxisTitleDistance = 24.0,
                AxislineStyle = LineStyle.Solid,
                MajorGridlineStyle = LineStyle.Dash,
                GapWidth = 0.5,
            };

            // specify key and position
            var valueAxis = new OxyPlot.Axes.LinearAxis()
            {
                Position = AxisPosition.Left,
                //Key = "Value",
                Title = y_axis_label,
                TitleFont = System.Drawing.FontFamily.GenericSerif.Name,
                TitleFontSize = 18,
                TitleFontWeight = FontWeights.Bold,
                StringFormat = "0%",
                AxisTitleDistance = 24.0,
                AxislineStyle = LineStyle.Solid,
                MajorGridlineStyle = LineStyle.Dash,
                MajorStep = 0.05,
                MinimumMajorStep = 0.05,
            };

            var bar_series = new OxyPlot.Series.ColumnSeries();

            for (int i = 0; i < growth_rate_values.Length; i++)
            {
                if (growth_rate_values[i] >= 0)
                    bar_series.Items.Add(new ColumnItem()
                    {
                        Value = growth_rate_values[i],
                        Color = OxyColor.FromRgb(0, 146, 255),
                    });
                else
                    bar_series.Items.Add(new ColumnItem()
                    {
                        Value = growth_rate_values[i],
                        Color = OxyColor.FromRgb(255, 146, 0),
                    });
                categoryAxis.Labels.Add(Convert.ToString(DAS_min + i));
            }
            bar_model.Series.Add(bar_series);
            bar_model.Axes.Add(categoryAxis);
            bar_model.Axes.Add(valueAxis);

            PngExporter.Export(bar_model, bar_filename + ".png", barimagewidth, barimageheight, new System.Drawing.SolidBrush(System.Drawing.Color.White));
        }

        public void Create_AllClass_Growth_Rate_Bar(string bar_filename, string bar_title, List<string> classnames, List<double> avg_data, int barimagewidth, int barimageheight)
        {
            // create bitmap for drawing the bar
            Bitmap bar_bmp = new Bitmap(barimagewidth, barimageheight, PixelFormat.Format24bppRgb);
            if (bar_bmp == null)
                return;
            Graphics g = Graphics.FromImage(bar_bmp);

            // fill the whole image with 'white' (background)
            System.Drawing.Color canvas_color = System.Drawing.Color.White;
            SolidBrush canvas_brush = new SolidBrush(canvas_color);
            g.FillRectangle(canvas_brush, 0, 0, barimagewidth, barimageheight);
            canvas_brush.Dispose();

            // draw thin border around the whole bar
            Pen grid_pen = new Pen(System.Drawing.Color.LightGray, 1);
            g.DrawLine(grid_pen, 0, 0, barimagewidth - 1, 0);
            g.DrawLine(grid_pen, barimagewidth - 1, 0, barimagewidth - 1, barimageheight - 1);
            g.DrawLine(grid_pen, barimagewidth - 1, barimageheight - 1, 0, barimageheight - 1);
            g.DrawLine(grid_pen, 0, barimageheight - 1, 0, 0);

            int leftmargin = (int)(LEFT_MARGIN * (double)barimagewidth + 0.5);
            int topmargin = (int)(TOP_MARGIN * (double)barimageheight + 0.5);
            int rightmargin = (int)(RIGHT_MARGIN * (double)barimagewidth + 0.5);
            int bottommargin = (int)(BOTTOM_MARGIN * (double)barimageheight + 0.5);
            int classnamesmargin = (int)(CLASS_NAMES * (double)barimageheight + 0.5);
            Pen margin_pen = new Pen(System.Drawing.Color.Gray, 2);

            SolidBrush title_brush = new SolidBrush(System.Drawing.Color.Black);
            int font_size = (barimageheight - bottommargin) / 3;
            System.Drawing.Font title_font = new System.Drawing.Font(FontFamily.GenericSerif, font_size, FontStyle.Bold, GraphicsUnit.Pixel);
            System.Drawing.Font scale_font = new System.Drawing.Font(FontFamily.GenericSerif, font_size, FontStyle.Bold, GraphicsUnit.Pixel);
            var format = new StringFormat() { Alignment = StringAlignment.Far };

            // draw bar title at the bottom
            g.DrawString(bar_title, title_font, title_brush, barimagewidth / 3, bottommargin + 2 * font_size - 6);

            // get the max. growth rate
            double max_Growth_Rate = 0.0;
            for (int i = 0; i < avg_data.Count; i++)
            {
                if (avg_data[i] > max_Growth_Rate)
                    max_Growth_Rate = avg_data[i];
            }
            max_Growth_Rate = 100.0 * (max_Growth_Rate - 1.0);

            // draw % scale at the left
            double step_on_graph = (double)(classnamesmargin - topmargin) / 12.0;
            double value_step = 0.0;
            if (max_Growth_Rate < 15)
                value_step = 1.5;
            else if (max_Growth_Rate < 20)
                value_step = 2.0;
            else if (max_Growth_Rate < 25)
                value_step = 2.5;
            else if (max_Growth_Rate < 30)
                value_step = 3.0;
            else if (max_Growth_Rate < 35)
                value_step = 3.5;
            else
                value_step = 5.0;

            int y = classnamesmargin;
            double value = 0.0;
            int step_nb = 0;
            while (y > topmargin)
            {
                g.DrawLine(margin_pen, leftmargin, y, rightmargin, y);
                string value_str = Convert.ToString(value) + " %";
                g.DrawString(value_str, scale_font, title_brush, leftmargin - 25, y - font_size / 2, format);
                value += value_step;
                step_nb++;
                y = (int)(classnamesmargin - (double)step_nb * step_on_graph);
            }

            // draw vertical bars and the class  names at the bottom
            double horz_step = (double)(rightmargin - leftmargin) / (double)avg_data.Count;
            int colorindex = 0;
            int left_start = leftmargin;
            for (int i = 0; i < avg_data.Count; i++)
            {
                // draw the bar
                if (colorindex > (bar_colors.Count - 1))
                    colorindex -= bar_colors.Count;
                double current_value = 100.0 * (avg_data[i] - 1.0);
                int bar_height = (int)(current_value * step_on_graph / value_step + 0.5);
                SolidBrush bar_brush = new SolidBrush(bar_colors[colorindex]);
                g.FillRectangle(bar_brush, leftmargin + (int)((i + 0.25) * horz_step), classnamesmargin - bar_height, (int)(horz_step / 2.0), bar_height);
                bar_brush.Dispose();
                colorindex++;

                // draw the correspondent class name
                StringFormat titleformat = new StringFormat();
                titleformat.Alignment = StringAlignment.Center;
                SizeF txt = g.MeasureString(classnames[i], title_font);
                SizeF sz = g.VisibleClipBounds.Size;
                g.TranslateTransform(sz.Width, 0);
                g.RotateTransform(270);
                g.DrawString(classnames[i], title_font, Brushes.Black, new RectangleF(-sz.Height - 230, -sz.Width + leftmargin + (int)((i + 0.5) * horz_step) - font_size / 2, sz.Height, sz.Width), titleformat);
                g.ResetTransform();
            }


            // save the bar and finish
            g.Dispose();
            bar_bmp.Save(bar_filename, System.Drawing.Imaging.ImageFormat.Png);
            grid_pen.Dispose();
            title_brush.Dispose();
            title_font.Dispose();
            margin_pen.Dispose();
            scale_font.Dispose();
        }

        public void Create_AllClass_Growth_Rate_Bar2(string bar_filename, string x_axis_label, string y_axis_label, List<string> classnames, List<double> growth_data, int barimagewidth, int barimageheight)
        {
            // create bitmap for drawing the bar
            Bitmap bar_bmp = new Bitmap(barimagewidth, barimageheight, PixelFormat.Format24bppRgb);
            if (bar_bmp == null)
                return;
            Graphics g = Graphics.FromImage(bar_bmp);

            // fill the whole image with 'white' (background)
            System.Drawing.Color canvas_color = System.Drawing.Color.White;
            SolidBrush canvas_brush = new SolidBrush(canvas_color);
            g.FillRectangle(canvas_brush, 0, 0, barimagewidth, barimageheight);
            canvas_brush.Dispose();

            // draw thin border around the whole bar
            Pen grid_pen = new Pen(System.Drawing.Color.LightGray, 1);
            g.DrawLine(grid_pen, 0, 0, barimagewidth - 1, 0);
            g.DrawLine(grid_pen, barimagewidth - 1, 0, barimagewidth - 1, barimageheight - 1);
            g.DrawLine(grid_pen, barimagewidth - 1, barimageheight - 1, 0, barimageheight - 1);
            g.DrawLine(grid_pen, 0, barimageheight - 1, 0, 0);

            int leftmargin = (int)(LEFT_MARGIN * (double)barimagewidth + 0.5);
            int topmargin = (int)(TOP_MARGIN * (double)barimageheight + 0.5);
            int rightmargin = (int)(RIGHT_MARGIN * (double)barimagewidth + 0.5);
            int bottommargin = (int)(BOTTOM_MARGIN * (double)barimageheight + 0.5);
            int classnamesmargin = (int)(CLASS_NAMES * (double)barimageheight + 0.5);
            Pen margin_pen = new Pen(System.Drawing.Color.Gray, 2);

            SolidBrush title_brush = new SolidBrush(System.Drawing.Color.Black);
            int font_size = (barimageheight - bottommargin) / 3;
            System.Drawing.Font title_font = new System.Drawing.Font(FontFamily.GenericSerif, font_size, FontStyle.Bold, GraphicsUnit.Pixel);
            System.Drawing.Font scale_font = new System.Drawing.Font(FontFamily.GenericSerif, font_size, FontStyle.Bold, GraphicsUnit.Pixel);
            var format = new StringFormat() { Alignment = StringAlignment.Far };

            // draw Y axis label at the left
            System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            g.DrawString(y_axis_label, title_font, title_brush, leftmargin + 2 * font_size - 2, barimageheight / 3, drawFormat);

            // draw X axis label at the bottom
            g.DrawString(x_axis_label, title_font, title_brush, barimagewidth / 3, bottommargin + 2 * font_size - 2);

            // get the max. growth rate
            double max_Growth_Rate = 0.0;
            for ( int i=0; i< growth_data.Count; i++)
            {
                if (growth_data[i] > max_Growth_Rate)
                    max_Growth_Rate = growth_data[i];
            }
            max_Growth_Rate = 100.0 * max_Growth_Rate;

            // draw % scale at the left
            double step_on_graph = (double)(classnamesmargin - topmargin) / 12.0;
            double value_step = 0.0;
            if (max_Growth_Rate < 15)
                value_step = 1.5;
            else if (max_Growth_Rate < 20)
                value_step = 2.0;
            else if (max_Growth_Rate < 25)
                value_step = 2.5;
            else if (max_Growth_Rate < 30)
                value_step = 3.0;
            else if (max_Growth_Rate < 35)
                value_step = 3.5;
            else
                value_step = 5.0;

            int y = classnamesmargin;
            double value = 0.0;
            int step_nb = 0;
            while (y > topmargin)
            {
                g.DrawLine(margin_pen, leftmargin, y, rightmargin, y);
                string value_str = Convert.ToString(value) + " %";
                g.DrawString(value_str, scale_font, title_brush, leftmargin - 25, y - font_size / 2, format);
                value += value_step;
                step_nb++;
                y = (int)(classnamesmargin - (double)step_nb * step_on_graph);
            }

            // draw vertical bars and the class  names at the bottom
            double horz_step = (double)(rightmargin - leftmargin) / (double)growth_data.Count;
            int colorindex = 0;
            int left_start = leftmargin;
            for ( int i=0; i< growth_data.Count; i++)
            {
                // draw the bar
                if (colorindex > (bar_colors.Count - 1))
                    colorindex -= bar_colors.Count;
                double current_value = 100.0 * growth_data[i];
                int bar_height = (int)(current_value * step_on_graph / value_step + 0.5);
                SolidBrush bar_brush = new SolidBrush(bar_colors[colorindex]);
                g.FillRectangle(bar_brush, leftmargin + (int)((i+0.25) * horz_step), classnamesmargin - bar_height, (int)(horz_step/2.0), bar_height);
                bar_brush.Dispose();
                colorindex++;

                // draw the correspondent class name
                StringFormat titleformat = new StringFormat();
                titleformat.Alignment = StringAlignment.Center;
                SizeF sz = g.VisibleClipBounds.Size;
                g.TranslateTransform(sz.Width, 0);
                g.RotateTransform(270);
                g.DrawString(classnames[i], title_font, Brushes.Black, new RectangleF(-sz.Height-230, -sz.Width + leftmargin + (int)((i + 0.5) * horz_step)- font_size/2, sz.Height, sz.Width), titleformat);
                g.ResetTransform();
            }


            // save the bar and finish
            g.Dispose();
            bar_bmp.Save(bar_filename, System.Drawing.Imaging.ImageFormat.Png);
            grid_pen.Dispose();
            title_brush.Dispose();
            title_font.Dispose();
            margin_pen.Dispose();
            scale_font.Dispose();
        }

        //-----------------------------------------------------------------------------------------------------

    }
}
