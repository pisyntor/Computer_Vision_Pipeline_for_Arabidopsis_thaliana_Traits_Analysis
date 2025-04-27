using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Globalization;
using System.IO;

namespace PlantInspector
{

    public class Excels_EPP
    {


        //------------------------------------------------------------------------------
        public Excels_EPP()
        {

        }

        public static List<List<string> > Read_from_Excel_File(string file_name)
        {
            var res = new List<List<string>>();

            using (var fs = new FileStream(file_name, FileMode.Open, FileAccess.Read))
            using (var package = new ExcelPackage(fs))
            {
                var ws = package.Workbook.Worksheets["Extracted Features"];
                int i_row = 1;
                bool stop_reading = false;
                while (!stop_reading)
                {
                    if (ws.Cells[i_row, 1].Value == null)
                        stop_reading = true;
                    else
                    {
                        List<string> new_line = new List<string>();
                        for (int i_col = 1; i_col < 22; i_col++)
                            new_line.Add(Convert.ToString(ws.Cells[i_row, i_col].Value));
                        res.Add(new_line);
                    }
                    i_row++;
                }
            }

            return res;
        }

        public static List<List<string> > Prepare_Class_Data_for_Export_to_Excel(
            double scaling_factor, int n_digits, string float_format,
            List<string> class_name_per_row, List<string> rep_sn_per_row,
            List<string> date_str, List<string> time_str,
            List<double> rosette_areas, List<double> area_to_perimeter_ratios,
            List<double> bounding_box_areas, List<double> bounding_box_aspect_ratios, List<double> bounding_ellipse_circularities,
            List<double> circumferences, List<double> compactnesses,
            List<double> convex_hull_areas, List<double> convex_hull_elongations, List<double> convex_hull_roundnesses,
            List<double> std_diameters, List<double> eccentricities, List<double> extents,
            List<double> rosette_perimeters, List<double> plant_roundnesses,
            List<double> rotational_mass_asymmetries, List<double> surface_coverages)
        {
            List<List<string>> excel_data = new List<List<string>>();

            for (int i_row = 0; i_row < date_str.Count; i_row++)
            {
                List<string> new_row = new List<string>();
                new_row.Add(date_str[i_row]);
                new_row.Add(time_str[i_row]);
                new_row.Add(System.Math.Round(rosette_areas[i_row] * scaling_factor * scaling_factor, n_digits, MidpointRounding.AwayFromZero).ToString(float_format, CultureInfo.CreateSpecificCulture("en-US")));
                new_row.Add(System.Math.Round(area_to_perimeter_ratios[i_row] * scaling_factor, n_digits, MidpointRounding.AwayFromZero).ToString(float_format, CultureInfo.CreateSpecificCulture("en-US")));
                new_row.Add(System.Math.Round(bounding_box_areas[i_row] * scaling_factor * scaling_factor, n_digits, MidpointRounding.AwayFromZero).ToString(float_format, CultureInfo.CreateSpecificCulture("en-US")));
                new_row.Add(System.Math.Round(bounding_box_aspect_ratios[i_row], n_digits, MidpointRounding.AwayFromZero).ToString(float_format, CultureInfo.CreateSpecificCulture("en-US")));
                new_row.Add(System.Math.Round(bounding_ellipse_circularities[i_row], n_digits, MidpointRounding.AwayFromZero).ToString(float_format, CultureInfo.CreateSpecificCulture("en-US")));
                new_row.Add(System.Math.Round(circumferences[i_row] * scaling_factor, n_digits, MidpointRounding.AwayFromZero).ToString(float_format, CultureInfo.CreateSpecificCulture("en-US")));
                new_row.Add(System.Math.Round(compactnesses[i_row], n_digits, MidpointRounding.AwayFromZero).ToString(float_format, CultureInfo.CreateSpecificCulture("en-US")));
                new_row.Add(System.Math.Round(convex_hull_areas[i_row] * scaling_factor * scaling_factor, n_digits, MidpointRounding.AwayFromZero).ToString(float_format, CultureInfo.CreateSpecificCulture("en-US")));
                new_row.Add(System.Math.Round(convex_hull_elongations[i_row], n_digits, MidpointRounding.AwayFromZero).ToString(float_format, CultureInfo.CreateSpecificCulture("en-US")));
                new_row.Add(System.Math.Round(convex_hull_roundnesses[i_row], n_digits, MidpointRounding.AwayFromZero).ToString(float_format, CultureInfo.CreateSpecificCulture("en-US")));
                new_row.Add(System.Math.Round(std_diameters[i_row] * scaling_factor, n_digits, MidpointRounding.AwayFromZero).ToString(float_format, CultureInfo.CreateSpecificCulture("en-US")));
                new_row.Add(System.Math.Round(eccentricities[i_row], n_digits, MidpointRounding.AwayFromZero).ToString(float_format, CultureInfo.CreateSpecificCulture("en-US")));
                new_row.Add(System.Math.Round(extents[i_row], n_digits, MidpointRounding.AwayFromZero).ToString(float_format, CultureInfo.CreateSpecificCulture("en-US")));
                new_row.Add(System.Math.Round(rosette_perimeters[i_row] * scaling_factor, n_digits, MidpointRounding.AwayFromZero).ToString(float_format, CultureInfo.CreateSpecificCulture("en-US")));
                new_row.Add(System.Math.Round(plant_roundnesses[i_row], n_digits, MidpointRounding.AwayFromZero).ToString(float_format, CultureInfo.CreateSpecificCulture("en-US")));
                new_row.Add(System.Math.Round(rotational_mass_asymmetries[i_row], n_digits, MidpointRounding.AwayFromZero).ToString(float_format, CultureInfo.CreateSpecificCulture("en-US")));
                new_row.Add(System.Math.Round(surface_coverages[i_row], n_digits, MidpointRounding.AwayFromZero).ToString(float_format, CultureInfo.CreateSpecificCulture("en-US")));
                new_row.Add(rep_sn_per_row[i_row]);
                new_row.Add(class_name_per_row[i_row]);

                excel_data.Add(new_row);
            }

            return excel_data;
        }

        public static void Write_to_Excel_File(
            string file_name, List<List<string> > excel_data)
        {
            using (var package = new ExcelPackage())
            {
                //Add a new ws to the empty workbook
                var ws = package.Workbook.Worksheets.Add("Extracted Features");
                //Add the headers
                string[] headers = 
                    {"Date", "Time",
                    "p_area", "p_area_to_perimeter_ratio",
                    "p_bounding_box_area", "p_bounding_box_aspect_ratio", "p_bounding_ellipse_circularity",
                    "p_circumference", "p_compactness",
                    "p_con_hull_area", "p_con_hull_elongation", "p_con_hull_roundness",
                    "p_diameter", "p_eccentricity", "p_extent", "p_perimeter",
                    "p_plant_roundness", "p_rot_mass_asymmetry", "p_surface_coverage",
                    "rep_num", "class"};
                for (int i = 0; i < headers.Length; i++)
                    ws.Cells[1, i + 1].Value = headers[i];
                for (int i_row = 0; i_row < excel_data.Count; i_row++)
                {
                    List<string> curr_row = excel_data[i_row];
                    for (int i_col = 0; i_col < curr_row.Count; i_col++)
                        ws.Cells[i_row + 2, i_col + 1].Value = curr_row[i_col];
                }
                package.SaveAs(new System.IO.FileInfo(file_name));
            }
        }

            //-----------------------------------------------------------------------------------------------------

    }
}
