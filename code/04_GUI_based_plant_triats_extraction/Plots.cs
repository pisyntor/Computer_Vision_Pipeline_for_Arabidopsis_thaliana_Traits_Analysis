using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using System.Drawing;
using System.Drawing.Imaging;

using Plotly.NET;
using Plotly.NET.LayoutObjects;
using static Plotly.NET.GenericChart;
using System.Linq;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using static Plotly.NET.StyleParam;
using OxyPlot.Axes;

namespace PlantInspector
{

    public class Plots
    {
        // Constraints
        public static double LEFT_MARGIN;       // margins around plot (relative to the image's sizes)
        public static double TOP_MARGIN;
        public static double RIGHT_MARGIN;
        public static double BOTTOM_MARGIN;
        public static double DAS_START;         // left position of first DAS value
        public static double NULL_START;        // bottom (=0) position of plotted data values
        public static double REP_COLORS_START;  // left edge of box listing the rep's colors
        public static int REP_COLOR_ITEM_HEIGHT;     // height of one row for rep-color items

        //------------------------------------------------------------------------------
        public Plots()
        {
            // constraints
            LEFT_MARGIN = 0.12;  //  0.16758;
            TOP_MARGIN = 0.0279;
            RIGHT_MARGIN = 0.87;
            BOTTOM_MARGIN  = 0.8982;
            DAS_START = 0.13;   //  0.1896;
            NULL_START = 0.8577;
            REP_COLORS_START = 0.8970;
            REP_COLOR_ITEM_HEIGHT = 20;
        }


        public void Create_Plot(string classname, string plot_filename, string plot_title, List<int> rep_serialnumbers, List<int> rep_lengths, int DAS_min, int DAS_max,
            List<double> DAS_s, List<double> plot_data, List<System.Drawing.Color> plot_colors, double scaling_pixel2mm, int plotimagewidth, int plotimageheight, Boolean bSplineInterpolation)
        {
            // create bitmap for drawing the plot
            Bitmap plot_bmp = new Bitmap(plotimagewidth, plotimageheight, PixelFormat.Format24bppRgb);
            if (plot_bmp == null)
                return;
            Graphics g = Graphics.FromImage(plot_bmp);

            // fill the whole image with 'white' (background)
            System.Drawing.Color canvas_color = System.Drawing.Color.White;
            SolidBrush canvas_brush = new SolidBrush(canvas_color);
            g.FillRectangle(canvas_brush, 0, 0, plotimagewidth, plotimageheight);
            canvas_brush.Dispose();

            // draw thin border around the whole graph
            Pen grid_pen = new Pen(System.Drawing.Color.LightGray, 1);
            g.DrawLine(grid_pen, 0, 0, plotimagewidth - 1, 0);
            g.DrawLine(grid_pen, plotimagewidth - 1, 0, plotimagewidth - 1, plotimageheight - 1);
            g.DrawLine(grid_pen, plotimagewidth - 1, plotimageheight - 1, 0, plotimageheight - 1);
            g.DrawLine(grid_pen, 0, plotimageheight - 1, 0, 0);

            // draw the borders around graph, leaving margin to the scale, titles and list of rep colors
            int leftmargin = (int)(LEFT_MARGIN * (double)plotimagewidth + 0.5);
            int topmargin = (int)(TOP_MARGIN * (double)plotimageheight + 0.5);
            int rightmargin = (int)(RIGHT_MARGIN * (double)plotimagewidth + 0.5);
            int bottommargin = (int)(BOTTOM_MARGIN * (double)plotimageheight + 0.5);
            Pen margin_pen = new Pen(System.Drawing.Color.Black, 3);
            g.DrawLine(margin_pen, rightmargin, bottommargin, leftmargin, bottommargin);
            g.DrawLine(margin_pen, leftmargin, bottommargin, leftmargin, topmargin);

            // draw title for abscissa
            string abscissa_title = "Days After Sowing [DAS]";
            SolidBrush title_brush = new SolidBrush(System.Drawing.Color.Black);
            int font_size = (plotimageheight - bottommargin) / 3;
            System.Drawing.Font title_font = new System.Drawing.Font(System.Drawing.FontFamily.GenericSerif, font_size, FontStyle.Bold, GraphicsUnit.Pixel);
            g.DrawString(abscissa_title, title_font, title_brush, plotimagewidth / 3, bottommargin + 2 * font_size - 6);

            // draw title for ordinata
            StringFormat titleformat = new StringFormat();
            titleformat.Alignment = StringAlignment.Center;
            SizeF txt = g.MeasureString(plot_title, title_font);
            SizeF sz = g.VisibleClipBounds.Size;
            g.TranslateTransform(sz.Width, 0);
            g.RotateTransform(270);
            g.DrawString(plot_title, title_font, Brushes.Black, new RectangleF(-sz.Height, -sz.Width + 15, sz.Height, sz.Width), titleformat);
            g.ResetTransform();

            // set misc. vars for drawing
            int repcolors_left = (int)(REP_COLORS_START * (double)plotimagewidth + 0.5);
            System.Drawing.Font scale_font = new System.Drawing.Font(System.Drawing.FontFamily.GenericSerif, 2 * font_size / 3, FontStyle.Bold, GraphicsUnit.Pixel);
            var format = new StringFormat() { Alignment = StringAlignment.Far };
            int DASstart = (int)(DAS_START * (double)plotimagewidth + 0.5);
            double DASstep = (double)(rightmargin - DASstart - 10) / (double)(DAS_max - DAS_min);

            // draw vertical scale with horizontal strips of grid
            int nullstart = (int)(NULL_START * (double)plotimageheight + 0.5);
            double value_min = 1000000;
            double value_max = 0;
            for (int i = 0; i < plot_data.Count; i++)
            {
                value_min = System.Math.Min(value_min, plot_data[i]);
                value_max = System.Math.Max(value_max, plot_data[i]);
            }
            value_min *= scaling_pixel2mm;   // convert from pixelnumber to real size or area
            value_max *= scaling_pixel2mm;
            double value_range = value_max - value_min;
            double value_step = 0.0;
            double step_on_graph = (double)(nullstart - topmargin) / 11.0;

            double mult = 1;
            while (value_step == 0)
            {
                if (value_range < 0.1) value_step = 0.01 * mult;
                else if (value_range < 0.2) value_step = 0.02 * mult;
                else if (value_range < 0.3) value_step = 0.03 * mult;
                else if (value_range < 0.4) value_step = 0.04 * mult;
                else if (value_range < 0.5) value_step = 0.05 * mult;
                else if (value_range < 0.6) value_step = 0.06 * mult;
                else if (value_range < 0.7) value_step = 0.07 * mult;
                else if (value_range < 0.8) value_step = 0.08 * mult;
                else if (value_range < 0.9) value_step = 0.09 * mult;
                value_range /= 10.0;
                mult *= 10.0;
            }

            value_min = value_step * (double)(int)(value_min / value_step);

            int y = nullstart;
            double value = value_min;   // 0.0;
            int step_nb = 0;
            while (y > topmargin)
            {
                g.DrawLine(grid_pen, leftmargin, y, rightmargin, y);
                g.DrawLine(margin_pen, leftmargin, y, leftmargin - 15, y);
                string value_str = Convert.ToString(value);
                g.DrawString(value_str, scale_font, title_brush, leftmargin - 25, y - font_size / 3, format);
                value += value_step;
                step_nb++;
                y = (int)(nullstart - step_nb * step_on_graph);
            }

            // draw reps list
            // Alek comment start: draw legend not skipping the unchecked 
            int start_index = 0;
            int reps_idx = 0;
            for (int i = 0; i < rep_serialnumbers.Count; i++)
            {
                int rep_sn = rep_serialnumbers[i];
                if (rep_sn == -1)
                {
                    continue;
                }
                int color_index = rep_sn - 1;
                while (color_index >= 10)
                    color_index -= 10;
                Pen plot_pen = new Pen(plot_colors[color_index], 3);

                int item_height = topmargin + (1 + reps_idx) * REP_COLOR_ITEM_HEIGHT + 10;
                g.DrawLine(plot_pen, repcolors_left + 15, item_height + 10, repcolors_left + 50, item_height + 10);
                string repcolor_item_sn = (rep_sn < 10) ? "0" : "";
                repcolor_item_sn += Convert.ToString(rep_sn);
                g.DrawString(repcolor_item_sn, scale_font, title_brush, plotimagewidth - 15, item_height, format);
                plot_pen.Dispose();

                reps_idx++;
            }
            // Alek comment end: draw legend not skipping the unchecked 

            // draw the plots of current data vectors
            start_index = 0;
            for (int i = 0; i < rep_serialnumbers.Count; i++)
            {
                int rep_sn = rep_serialnumbers[i];
                if (rep_sn == -1)
                {
                    continue;
                }
                int color_index = rep_sn - 1;
                while (color_index >= 10)
                    color_index -= 10;
                Pen plot_pen = new Pen(plot_colors[color_index], 3);
                int rep_length = rep_lengths[i];

                double[] x_i = new double[rep_length];
                double[] y_i = new double[rep_length];
                for (int j = start_index; j < (start_index + rep_length); j++)
                {
                    double current_DAS = DAS_s[j] - DAS_min;
                    if (j < (start_index + rep_length - 1))
                    {
                        double next_DAS = DAS_s[j + 1] - DAS_min;
                        if (next_DAS == 0 || current_DAS == next_DAS)
                            continue;
                    }
                    if (current_DAS >= 0)
                    {
                        double current_value = scaling_pixel2mm * plot_data[j] - value_min;
                        x_i[j - start_index] = (int)(DASstart + current_DAS * DASstep + 0.5);
                        y_i[j - start_index] = (int)(nullstart - current_value * step_on_graph / value_step);
                    }
                }
                if (bSplineInterpolation)
                {
                    CubicSpline spline = new CubicSpline();
                    int count = (int)(x_i[rep_length - 1] - x_i[0]);
                    double[] x_o = new double[count];
                    double[] y_o = new double[count];
                    spline.InterpolateXY(x_i, y_i, count, ref x_o, ref y_o);
                    for (int j = 0; j < (count - 1); j++)
                    {
                        g.DrawLine(plot_pen, (int)(x_o[j] + 0.5), (int)(y_o[j] + 0.5), (int)(x_o[j + 1] + 0.5), (int)(y_o[j + 1] + 0.5));
                    }
                }
                else
                {
                    for (int j = 0; j < (rep_length - 1); j++)
                    {
                        g.DrawLine(plot_pen, (int)x_i[j], (int)y_i[j], (int)x_i[j + 1], (int)y_i[j + 1]);
                    }
                }

                start_index += rep_length;
            }

            // draw box for 'Reps colors' list
            g.DrawLine(margin_pen, repcolors_left, topmargin, plotimagewidth - 5, topmargin);
            int list_height = (2 + rep_serialnumbers.Count) * REP_COLOR_ITEM_HEIGHT;
            g.DrawLine(margin_pen, repcolors_left, topmargin, repcolors_left, topmargin + list_height);
            g.DrawLine(margin_pen, repcolors_left, topmargin + list_height, plotimagewidth - 5, topmargin + list_height);
            g.DrawLine(margin_pen, plotimagewidth - 5, topmargin, plotimagewidth - 5, topmargin + list_height);
            string repcolor_title = "Reps.:";
            //Move the string left
            g.DrawString(repcolor_title, scale_font, title_brush, plotimagewidth - 47, topmargin + 5, format);

            // draw horizontal scale with vertical strips of grid
            for (int i = 0; i <= (DAS_max - DAS_min); i++)
            {
                int x = (int)(DASstart + i * DASstep + 0.5);
                g.DrawLine(grid_pen, x, topmargin, x, bottommargin);
                if (i == i / 2 * 2)
                {
                    g.DrawLine(margin_pen, x, bottommargin, x, bottommargin + 15);
                    string DAS_value = Convert.ToString(DAS_min + i);
                    g.DrawString(DAS_value, scale_font, title_brush, x - font_size / 2, bottommargin + 20);
                }
                else if ((DAS_max - DAS_min) <= 20)
                    g.DrawLine(margin_pen, x, bottommargin, x, bottommargin + 15);
            }

            // draw box for 'Title' for the whole bar (name of class)
            int Bar_title_Width = 140;
            int Bar_title_Height = 40;
            g.DrawLine(margin_pen, plotimagewidth / 3, topmargin, plotimagewidth / 3 + Bar_title_Width, topmargin);
            g.DrawLine(margin_pen, plotimagewidth / 3 + Bar_title_Width, topmargin, plotimagewidth / 3 + Bar_title_Width, topmargin + Bar_title_Height);
            g.DrawLine(margin_pen, plotimagewidth / 3, topmargin + Bar_title_Height, plotimagewidth / 3 + Bar_title_Width, topmargin + Bar_title_Height);
            g.DrawLine(margin_pen, plotimagewidth / 3, topmargin + Bar_title_Height, plotimagewidth / 3, topmargin);
            g.DrawString(classname, title_font, title_brush, plotimagewidth / 3 + Bar_title_Width / 2 - 6 * classname.Length, topmargin + 6);

            // save the plot and finish
            g.Dispose();
            plot_bmp.Save(plot_filename, System.Drawing.Imaging.ImageFormat.Png);
            grid_pen.Dispose();
            title_font.Dispose();
            title_brush.Dispose();
            margin_pen.Dispose();
            scale_font.Dispose();
        }


        public void Create_Plot_Of_DAS_Avg(string classname, string plot_filename, string plot_title, List<int> rep_serialnumbers, List<int> rep_lengths, int[] rep_day_shifts, int DAS_min, int DAS_max,
            SortedList<string, SortedList<int, List<double>>> feature_value_per_rep_per_int_DAS, List<System.Drawing.Color> plot_colors, double scaling_pixel2mm, int plotimagewidth, int plotimageheight, Boolean bSplineInterpolation)
        {
            // create bitmap for drawing the plot
            Bitmap plot_bmp = new Bitmap(plotimagewidth, plotimageheight, PixelFormat.Format24bppRgb);
            if (plot_bmp == null)
                return;
            Graphics g = Graphics.FromImage(plot_bmp);

            // fill the whole image with 'white' (background)
            System.Drawing.Color canvas_color = System.Drawing.Color.White;
            SolidBrush canvas_brush = new SolidBrush(canvas_color);
            g.FillRectangle(canvas_brush, 0, 0, plotimagewidth, plotimageheight);
            canvas_brush.Dispose();

            // draw thin border around the whole graph
            Pen grid_pen = new Pen(System.Drawing.Color.LightGray, 1);
            g.DrawLine(grid_pen, 0, 0, plotimagewidth - 1, 0);
            g.DrawLine(grid_pen, plotimagewidth - 1, 0, plotimagewidth - 1, plotimageheight - 1);
            g.DrawLine(grid_pen, plotimagewidth - 1, plotimageheight - 1, 0, plotimageheight - 1);
            g.DrawLine(grid_pen, 0, plotimageheight - 1, 0, 0);

            // draw the borders around graph, leaving margin to the scale, titles and list of rep colors
            int leftmargin = (int)(LEFT_MARGIN * (double)plotimagewidth + 0.5);
            int topmargin = (int)(TOP_MARGIN * (double)plotimageheight + 0.5);
            int rightmargin = (int)(RIGHT_MARGIN * (double)plotimagewidth + 0.5);
            int bottommargin = (int)(BOTTOM_MARGIN * (double)plotimageheight + 0.5);
            Pen margin_pen = new Pen(System.Drawing.Color.Black, 3);
            g.DrawLine(margin_pen, rightmargin, bottommargin, leftmargin, bottommargin);
            g.DrawLine(margin_pen, leftmargin, bottommargin, leftmargin, topmargin);

            // draw title for abscissa
            string abscissa_title = "Days After Sowing [DAS]";
            SolidBrush title_brush = new SolidBrush(System.Drawing.Color.Black);
            int font_size = (plotimageheight - bottommargin) / 3;
            System.Drawing.Font title_font = new System.Drawing.Font(System.Drawing.FontFamily.GenericSerif, font_size, FontStyle.Bold, GraphicsUnit.Pixel);
            g.DrawString(abscissa_title, title_font, title_brush, plotimagewidth / 3, bottommargin + 2 * font_size - 6);

            // draw title for ordinata
            StringFormat titleformat = new StringFormat();
            titleformat.Alignment = StringAlignment.Center;
            SizeF txt = g.MeasureString(plot_title, title_font);
            SizeF sz = g.VisibleClipBounds.Size;
            g.TranslateTransform(sz.Width, 0);
            g.RotateTransform(270);
            g.DrawString(plot_title, title_font, Brushes.Black, new RectangleF(-sz.Height, -sz.Width + 15, sz.Height, sz.Width), titleformat);
            g.ResetTransform();

            // set misc. vars for drawing
            int repcolors_left = (int)(REP_COLORS_START * (double)plotimagewidth + 0.5);
            System.Drawing.Font scale_font = new System.Drawing.Font(System.Drawing.FontFamily.GenericSerif, 2 * font_size / 3, FontStyle.Bold, GraphicsUnit.Pixel);
            var format = new StringFormat() { Alignment = StringAlignment.Far };
            int DASstart = (int)(DAS_START * (double)plotimagewidth + 0.5);
            double DASstep = (double)(rightmargin - DASstart - 10) / (double)(DAS_max - DAS_min);

            // Compute averages per rep per day
            double value_min = 1000000;
            double value_max = 0;
            SortedList<string, SortedList<int, double>> feature_average_per_DAS = new SortedList<string, SortedList<int, double>>();
            foreach (string rep_name in feature_value_per_rep_per_int_DAS.Keys)
            {
                feature_average_per_DAS[rep_name] = new SortedList<int, double>();
                foreach (int DAS_int in feature_value_per_rep_per_int_DAS[rep_name].Keys)
                    feature_average_per_DAS[rep_name][DAS_int] = feature_value_per_rep_per_int_DAS[rep_name][DAS_int].Count > 0 ? feature_value_per_rep_per_int_DAS[rep_name][DAS_int].Average() : 0;
                value_min = System.Math.Min(value_min, feature_average_per_DAS[rep_name].Values.Min());
                value_max = System.Math.Max(value_max, feature_average_per_DAS[rep_name].Values.Max());
            }

            // draw vertical scale with horizontal strips of grid
            int nullstart = (int)(NULL_START * (double)plotimageheight + 0.5);
            value_min *= scaling_pixel2mm;   // convert from pixelnumber to real size or area
            value_max *= scaling_pixel2mm;
            double value_range = value_max - value_min;
            double value_step = 0.0;
            double step_on_graph = (double)(nullstart - topmargin) / 11.0;

            double mult = 1;
            while (value_step == 0)
            {
                if (value_range < 0.1) value_step = 0.01 * mult;
                else if (value_range < 0.2) value_step = 0.02 * mult;
                else if (value_range < 0.3) value_step = 0.03 * mult;
                else if (value_range < 0.4) value_step = 0.04 * mult;
                else if (value_range < 0.5) value_step = 0.05 * mult;
                else if (value_range < 0.6) value_step = 0.06 * mult;
                else if (value_range < 0.7) value_step = 0.07 * mult;
                else if (value_range < 0.8) value_step = 0.08 * mult;
                else if (value_range < 0.9) value_step = 0.09 * mult;
                value_range /= 10.0;
                mult *= 10.0;
            }

            value_min = value_step * (double)(int)(value_min / value_step);

            int y = nullstart;
            double value = value_min;   // 0.0;
            int step_nb = 0;
            while (y > topmargin)
            {
                g.DrawLine(grid_pen, leftmargin, y, rightmargin, y);
                g.DrawLine(margin_pen, leftmargin, y, leftmargin - 15, y);
                string value_str = Convert.ToString(value);
                g.DrawString(value_str, scale_font, title_brush, leftmargin - 25, y - font_size / 3, format);
                value += value_step;
                step_nb++;
                y = (int)(nullstart - step_nb * step_on_graph);
            }

            // draw reps list
            // Alek comment start: draw legend not skipping the unchecked 
            int reps_idx = 0;
            for (int i = 0; i < rep_serialnumbers.Count; i++)
            {
                int rep_sn = rep_serialnumbers[i];
                if (rep_sn == -1)
                {
                    continue;
                }
                int color_index = rep_sn - 1;
                while (color_index >= 10)
                    color_index -= 10;
                Pen plot_pen = new Pen(plot_colors[color_index], 3);

                int item_height = topmargin + (1 + reps_idx) * REP_COLOR_ITEM_HEIGHT + 10;
                g.DrawLine(plot_pen, repcolors_left + 15, item_height + 10, repcolors_left + 50, item_height + 10);
                string repcolor_item_sn = (rep_sn < 10) ? "0" : "";
                repcolor_item_sn += Convert.ToString(rep_sn);
                g.DrawString(repcolor_item_sn, scale_font, title_brush, plotimagewidth - 15, item_height, format);
                plot_pen.Dispose();

                reps_idx++;
            }
            // Alek comment end: draw legend not skipping the unchecked 

            // draw the plots of current data vectors
            for (int i = 0; i < rep_serialnumbers.Count; i++)
            {
                int rep_sn = rep_serialnumbers[i];
                if (rep_sn == -1)
                {
                    continue;
                }
                string rep_name = "rep_" + rep_sn.ToString("D2");
                int color_index = rep_sn - 1;
                while (color_index >= 10)
                    color_index -= 10;
                Pen plot_pen = new Pen(plot_colors[color_index], 3);

                double[] x_i = new double[feature_average_per_DAS[rep_name].Count];
                double[] y_i = new double[feature_average_per_DAS[rep_name].Count];
                for (int j = 0; j < feature_average_per_DAS[rep_name].Count; j++)
                {
                    double current_DAS = feature_average_per_DAS[rep_name].Keys[j] - DAS_min - rep_day_shifts[i];
                    if (j < feature_average_per_DAS[rep_name].Count - 1)
                    {
                        double next_DAS = feature_average_per_DAS[rep_name].Keys[j + 1] - DAS_min - rep_day_shifts[i];
                        if (next_DAS == 0 || current_DAS == next_DAS)
                            continue;
                    }
                    if (current_DAS >= 0)
                    {
                        double current_value = scaling_pixel2mm * feature_average_per_DAS[rep_name].Values[j] - value_min;
                        x_i[j] = (int)(DASstart + current_DAS * DASstep + 0.5);
                        y_i[j] = (int)(nullstart - current_value * step_on_graph / value_step);
                    }
                }
                if (bSplineInterpolation)
                {
                    CubicSpline spline = new CubicSpline();
                    int count = (int)(x_i[feature_average_per_DAS[rep_name].Count - 1] - x_i[0]);
                    double[] x_o = new double[count];
                    double[] y_o = new double[count];
                    spline.InterpolateXY(x_i, y_i, count, ref x_o, ref y_o);
                    for (int j = 0; j < (count - 1); j++)
                    {
                        g.DrawLine(plot_pen, (int)(x_o[j] + 0.5), (int)(y_o[j] + 0.5), (int)(x_o[j + 1] + 0.5), (int)(y_o[j + 1] + 0.5));
                    }
                }
                else
                {
                    for (int j = 0; j < (feature_average_per_DAS[rep_name].Count - 1); j++)
                    {
                        g.DrawLine(plot_pen, (int)x_i[j], (int)y_i[j], (int)x_i[j + 1], (int)y_i[j + 1]);
                    }
                }
            }

            // draw box for 'Reps colors' list
            g.DrawLine(margin_pen, repcolors_left, topmargin, plotimagewidth - 5, topmargin);
            int list_height = (2 + rep_serialnumbers.Count) * REP_COLOR_ITEM_HEIGHT;
            g.DrawLine(margin_pen, repcolors_left, topmargin, repcolors_left, topmargin + list_height);
            g.DrawLine(margin_pen, repcolors_left, topmargin + list_height, plotimagewidth - 5, topmargin + list_height);
            g.DrawLine(margin_pen, plotimagewidth - 5, topmargin, plotimagewidth - 5, topmargin + list_height);
            string repcolor_title = "Reps.:";
            //Move the string left
            g.DrawString(repcolor_title, scale_font, title_brush, plotimagewidth - 47, topmargin + 5, format);

            // draw horizontal scale with vertical strips of grid
            for (int i = 0; i <= (DAS_max - DAS_min); i++)
            {
                int x = (int)(DASstart + i * DASstep + 0.5);
                g.DrawLine(grid_pen, x, topmargin, x, bottommargin);
                if (i == i / 2 * 2)
                {
                    g.DrawLine(margin_pen, x, bottommargin, x, bottommargin + 15);
                    string DAS_value = Convert.ToString(DAS_min + i);
                    g.DrawString(DAS_value, scale_font, title_brush, x - font_size / 2, bottommargin + 20);
                }
                else if ((DAS_max - DAS_min) <= 20)
                    g.DrawLine(margin_pen, x, bottommargin, x, bottommargin + 15);
            }

            // draw box for 'Title' for the whole bar (name of class)
            int Bar_title_Width = 140;
            int Bar_title_Height = 40;
            g.DrawLine(margin_pen, plotimagewidth / 3, topmargin, plotimagewidth / 3 + Bar_title_Width, topmargin);
            g.DrawLine(margin_pen, plotimagewidth / 3 + Bar_title_Width, topmargin, plotimagewidth / 3 + Bar_title_Width, topmargin + Bar_title_Height);
            g.DrawLine(margin_pen, plotimagewidth / 3, topmargin + Bar_title_Height, plotimagewidth / 3 + Bar_title_Width, topmargin + Bar_title_Height);
            g.DrawLine(margin_pen, plotimagewidth / 3, topmargin + Bar_title_Height, plotimagewidth / 3, topmargin);
            g.DrawString(classname, title_font, title_brush, plotimagewidth / 3 + Bar_title_Width / 2 - 6 * classname.Length, topmargin + 6);

            // save the plot and finish
            g.Dispose();
            plot_bmp.Save(plot_filename + "_old.png", System.Drawing.Imaging.ImageFormat.Png);
            grid_pen.Dispose();
            title_font.Dispose();
            title_brush.Dispose();
            margin_pen.Dispose();
            scale_font.Dispose();
        }


        public void Create_Plot_Of_DAS_Avg__Oxyplot_2(string classname, string plot_filename, string plot_title, List<int> rep_serialnumbers, List<int> rep_lengths, int[] rep_day_shifts,
            List<double> DAS_s, List<double> plot_data, List<System.Drawing.Color> plot_colors, double scaling_pixel2mm, int plotimagewidth, int plotimageheight, Boolean bSplineInterpolation)
        {
            List<OxyPlot.OxyColor> oxyplot_colors = new List<OxyPlot.OxyColor>();
            for (int i = 0; i < plot_colors.Count; i++)
                oxyplot_colors.Add(OxyColor.FromArgb(plot_colors[i].A, plot_colors[i].R, plot_colors[i].G, plot_colors[i].B));

            var line_plot_model = new PlotModel()
            {
                Title = classname,
                TitleFont = System.Drawing.FontFamily.GenericSerif.Name,
                TitleFontWeight = FontWeights.Bold,
                Background = OxyColor.FromRgb(255, 255, 255),
            };

            // specify key and position
            var x_axis = new OxyPlot.Axes.LinearAxis()
            {
                Position = AxisPosition.Bottom,
                //Key = "Value",
                TitleFont = System.Drawing.FontFamily.GenericSerif.Name,
                Title = "Days After Sowing [DAS]",
                TitleFontSize = 18,
                TitleFontWeight = FontWeights.Bold,
                AxisTitleDistance = 24.0,
                AxislineStyle = LineStyle.Solid,
                MajorGridlineStyle = LineStyle.Dash,
                MinimumMajorStep = 1.0,
            };

            // specify key and position
            var y_axis = new OxyPlot.Axes.LinearAxis()
            {
                Position = AxisPosition.Left,
                //Key = "Value",
                Title = plot_title,
                TitleFont = System.Drawing.FontFamily.GenericSerif.Name,
                TitleFontSize = 18,
                TitleFontWeight = FontWeights.Bold,
                AxisTitleDistance = 24.0,
                AxislineStyle = LineStyle.Solid,
                MajorGridlineStyle = LineStyle.Dash,
            };

            line_plot_model.Axes.Add(x_axis);
            line_plot_model.Axes.Add(y_axis);
            line_plot_model.LegendPlacement = LegendPlacement.Outside;
            line_plot_model.LegendPosition = LegendPosition.RightTop;
            line_plot_model.LegendFontSize = 13;
            line_plot_model.LegendTitle = "Reps:\n ";
            line_plot_model.LegendFont = System.Drawing.FontFamily.GenericSerif.Name;
            line_plot_model.LegendTitleFontSize = 14;
            line_plot_model.LegendTitleFontWeight = FontWeights.Bold;

            // Scale data
            List<double> scaled_plot_data = new List<double>();
            for (int i = 0; i < plot_data.Count; i++)
                scaled_plot_data.Add(scaling_pixel2mm * plot_data[i]);

            // draw the plots of current data vectors
            int rep_start = 0;
            for (int i = 0; i < rep_serialnumbers.Count; i++)
            {
                int rep_sn = rep_serialnumbers[i];
                if (rep_sn == -1)
                {
                    continue;
                }
                string rep_name = "rep_" + rep_sn.ToString("D2");
                int color_index = rep_sn - 1;
                while (color_index >= 10)
                    color_index -= 10;
                Pen plot_pen = new Pen(plot_colors[color_index], 3);

                // create lines and fill them with data points
                var curr_line_series = new OxyPlot.Series.LineSeries()
                {
                    Title = rep_sn.ToString("D2"),
                    Font = System.Drawing.FontFamily.GenericSerif.Name,
                    Color = oxyplot_colors[i % oxyplot_colors.Count],
                    StrokeThickness = 3,
                };

                List<double> x_i_list = DAS_s.GetRange(rep_start, rep_lengths[i]).Select(day => day - rep_day_shifts[i]).ToList();
                List<double> y_i_list = scaled_plot_data.GetRange(rep_start, rep_lengths[i]);
                Dictionary<int, double> y_total_per_day = new Dictionary<int, double>();
                Dictionary<int, double> n_vals_per_day = new Dictionary<int, double>();
                // Average values per day
                for (int j = 0; j < rep_lengths[i]; j++)
                {
                    int int_DAS = (int)x_i_list[j];
                    if (!y_total_per_day.ContainsKey(int_DAS))
                    {
                        y_total_per_day[int_DAS] = 0;
                        n_vals_per_day[int_DAS] = 0;
                    }
                    y_total_per_day[int_DAS] += y_i_list[j];
                    n_vals_per_day[int_DAS] += 1;
                }
                foreach (int int_DAS in y_total_per_day.Keys)
                    curr_line_series.Points.Add(new DataPoint(int_DAS, y_total_per_day[int_DAS] / n_vals_per_day[int_DAS]));
                line_plot_model.Series.Add(curr_line_series);
                rep_start += rep_lengths[i];
            }

            PngExporter.Export(line_plot_model, plot_filename, plotimagewidth, plotimageheight, new System.Drawing.SolidBrush(System.Drawing.Color.White));
        }


        public void Create_PlotHtml(string classname, string plot_filename, string plot_title, List<int> rep_serialnumbers, List<int> rep_lengths, int[] rep_day_shifts,
            List<double> DAS_s, List<double> plot_data, List<System.Drawing.Color> plot_colors, double scaling_pixel2mm, int plotimagewidth, int plotimageheight, Boolean bSplineInterpolation)
        {
            Plotly.NET.LayoutObjects.LinearAxis xAxis = new Plotly.NET.LayoutObjects.LinearAxis();
            xAxis.SetValue("title", "<b>Days After Sowing [DAS]</b>");
            xAxis.SetValue("gridcolor", "lightgray");
            xAxis.SetValue("showgrid", true);
            xAxis.SetValue("linecolor", "black");
            xAxis.SetValue("showline", true);

            Plotly.NET.LayoutObjects.LinearAxis yAxis = new Plotly.NET.LayoutObjects.LinearAxis();
            yAxis.SetValue("title", String.Format("<b>{0}</b>", plot_title));
            yAxis.SetValue("gridcolor", "lightgray");
            yAxis.SetValue("showgrid", true);
            yAxis.SetValue("linecolor", "black");
            yAxis.SetValue("showline", true);

            Title title = new Title();
            title.SetValue("text", String.Format("<b>{0}</b>", classname));
            title.SetValue("y", 0.9);
            title.SetValue("x", 0.5);
            title.SetValue("yanchor", "top");
            title.SetValue("xanchor", "center");

            Layout layout = new Layout();
            layout.SetValue("title", title);
            layout.SetValue("xaxis", xAxis);
            layout.SetValue("yaxis", yAxis);
            layout.SetValue("width", plotimagewidth);
            layout.SetValue("height", plotimageheight);
            layout.SetValue("showlegend", true);
            layout.SetValue("plot_bgcolor", "white");

            // Scale data
            List<double> scaled_plot_data = new List<double>();
            for (int i = 0; i < plot_data.Count; i++)
                scaled_plot_data.Add(scaling_pixel2mm * plot_data[i]);

            // Make rep plots
            List<string> layout_colors = new List<string>();
            int n_colors = plot_colors.Count;
            bool[] selected_colors = new bool[n_colors];
            int n_sel_reps = 0;
            for (int i = 0; i < rep_serialnumbers.Count; i++)
                if (rep_serialnumbers[i] != -1)
                    n_sel_reps++;
            int curr_n_reps = 0;
            if (n_sel_reps > 0)
            {
                Trace[] trace_list = new Trace[n_sel_reps];
                int rep_start = 0;
                for (int i = 0; i < rep_serialnumbers.Count; i++)
                {
                    int rep_sn = rep_serialnumbers[i];
                    int i_color = i % n_colors;
                    if (rep_sn == -1)
                    {
                        selected_colors[i_color] = false;
                        continue;
                    }
                    selected_colors[i_color] = true;
                    layout_colors.Add("rgba(" + plot_colors[i_color].R.ToString() + ", " + plot_colors[i_color].G.ToString() + ", " + plot_colors[i_color].B.ToString() + ", 1.0)");

                    Trace trace = new Trace("scatter");
                    trace.SetValue("x", DAS_s.GetRange(rep_start, rep_lengths[i]).Select(day => day - rep_day_shifts[i]).ToList());
                    trace.SetValue("y", scaled_plot_data.GetRange(rep_start, rep_lengths[i]));
                    trace.SetValue("mode", "lines+markers");
                    trace.SetValue("name", String.Format("<b>Rep. {0:D2}</b>", rep_sn));

                    trace_list[curr_n_reps] = trace;
                    curr_n_reps++;
                    rep_start += rep_lengths[i];
                }

                for (int i_color = 0; i_color < plot_colors.Count; i_color++)
                    if (!selected_colors[i_color])
                        layout_colors.Add("rgba(" + plot_colors[i_color].R.ToString() + ", " + plot_colors[i_color].G.ToString() + ", " + plot_colors[i_color].B.ToString() + ", 1.0)");
                layout.SetValue("colorway", layout_colors);

                Plotly.NET.GenericChart.ofTraceObjects(true, Microsoft.FSharp.Collections.ListModule.OfSeq(trace_list)).WithLayout(layout).SaveHtml(plot_filename);
            }
        }

        //-----------------------------------------------------------------------------------------------------

    }
}
