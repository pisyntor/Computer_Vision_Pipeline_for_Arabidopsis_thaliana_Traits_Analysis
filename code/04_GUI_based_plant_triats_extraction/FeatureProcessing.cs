using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using System.Drawing;
using System.Drawing.Imaging;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System.Linq;
using MathNet.Numerics.Providers.LinearAlgebra;
using static System.Net.Mime.MediaTypeNames;

namespace PlantInspector
{


    public class FeatureProcessing
    {
        //------------------------------------------------------------------------------
        public double GetAreaBasedOnContour(List<Point> contour)
        {
            // get the area covered by polygon using the "shoelace" formula
            double area = 0.0;

            if (contour == null || contour.Count == 0)
                return area;

            int nbofpoints = contour.Count;

            for (int i = 0; i < nbofpoints - 1; i++)
            {
                area += contour[i].X * contour[i + 1].Y - contour[i + 1].X * contour[i].Y;
            }
            area += contour[nbofpoints - 1].X * contour[0].Y - contour[0].X * contour[nbofpoints - 1].Y;
            area /= 2.0;
            if (area < 0)
                area = -area;

            return area;
        }

        public double GetPerimeter(List<Point> contour)
        {
            double perimeter = 0.0;


            if (contour == null || contour.Count == 0)
                return perimeter;

            int nbofpoints = contour.Count;
            for (int i = 0; i < nbofpoints - 1; i++)
            {
                perimeter += System.Math.Sqrt(System.Math.Pow((double)(contour[i + 1].X - contour[i].X), 2.0) + System.Math.Pow((double)(contour[i + 1].Y - contour[i].Y), 2.0));
            }
            perimeter += System.Math.Sqrt(System.Math.Pow((double)(contour[0].X - contour[nbofpoints - 1].X), 2.0) + System.Math.Pow((double)(contour[0].Y - contour[nbofpoints - 1].Y), 2.0));

            return perimeter;
        }

        private double cross(Point O, Point A, Point B)
        {
            return (A.X - O.X) * (B.Y - O.Y) - (A.Y - O.Y) * (B.X - O.X);
        }

        public List<Point> GetConvexHull(List<Point> contour)
        {
            if (contour == null)
                return null;

            if (contour.Count <= 1)
                return contour;

            int n = contour.Count, k = 0;
            List<Point> H = new List<Point>(new Point[2 * n]);

            contour.Sort((a, b) =>
                 a.X == b.X ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));

            // Build lower hull
            for (int i = 0; i < n; ++i)
            {
                while (k >= 2 && cross(H[k - 2], H[k - 1], contour[i]) <= 0)
                    k--;
                H[k++] = contour[i];
            }

            // Build upper hull
            for (int i = n - 2, t = k + 1; i >= 0; i--)
            {
                while (k >= t && cross(H[k - 2], H[k - 1], contour[i]) <= 0)
                    k--;
                H[k++] = contour[i];
            }
            H = H.GetRange(0, k - 1);   //  H.Take(k - 1).ToList();
            return H;

        }

        public Point[] GetStandardDiameter(List<Point> contour)
        {
            Point[] endpoints = new Point[2];

            if (contour == null || contour.Count == 0)
                return endpoints;

            // (Brut Force)
            double max_distance2 = 0.0;
            int nbofpoints = contour.Count;
            for (int i = 0; i < nbofpoints - 1; i++)
                for (int j = i + 1; j < nbofpoints; j++)
                {
                    double distance2 = System.Math.Pow((double)(contour[j].X - contour[i].X), 2.0) + System.Math.Pow((double)(contour[j].Y - contour[i].Y), 2.0);
                    if (distance2 > max_distance2)
                    {
                        max_distance2 = distance2;
                        endpoints[0].X = contour[i].X;
                        endpoints[0].Y = contour[i].Y;
                        endpoints[1].X = contour[j].X;
                        endpoints[1].Y = contour[j].Y;
                    }
                }

            return endpoints;
        }

        public List<Point> GetBoundingBox(List<Point> contour)
        {
            List<Point> cornerpoints = new List<Point>();
            Point p1 = new Point(0, 0);
            Point p2 = new Point(0, 0);
            Point p3 = new Point(0, 0);
            Point p4 = new Point(0, 0);

            if (contour == null || contour.Count == 0)
            {
                // invalid contour
                cornerpoints.Add(p1);
                cornerpoints.Add(p2);
                cornerpoints.Add(p3);
                cornerpoints.Add(p4);
                return cornerpoints;
            }

            // Brut force method for getting the minimal area bounding box
            double min_area = 1000000;   // init with impossible
            int nbofpoints = contour.Count;
            for (int angle_degree = 0; angle_degree < 180; angle_degree++)
            {
                double angle = System.Math.PI * (double)angle_degree / 180.0;  // convert from degree to radian
                double cos_angle = System.Math.Cos(angle);
                double sin_angle = System.Math.Sin(angle);

                double min_x = 10000;
                double max_x = -10000;
                double min_y = 10000;
                double max_y = -10000;

                for (int i = 0; i < nbofpoints; i++)
                {
                    Point pt = new Point(contour[i].X, contour[i].Y);
                    double rotated_X = cos_angle * pt.X + sin_angle * pt.Y;
                    min_x = System.Math.Min(min_x, rotated_X);
                    max_x = System.Math.Max(max_x, rotated_X);
                    double rotated_Y = -sin_angle * pt.X + cos_angle * pt.Y;
                    min_y = System.Math.Min(min_y, rotated_Y);
                    max_y = System.Math.Max(max_y, rotated_Y);
                }

                double area = (max_x - min_x) * (max_y - min_y);
                if (area < min_area)
                {
                    min_area = area;
                    double cos_angle2 = System.Math.Cos(-angle);
                    double sin_angle2 = System.Math.Sin(-angle);
                    p1.X = (int)(cos_angle2 * min_x + sin_angle2 * min_y + 0.5);
                    p1.Y = (int)(-sin_angle2 * min_x + cos_angle2 * min_y + 0.5);
                    p2.X = (int)(cos_angle2 * max_x + sin_angle2 * min_y + 0.5);
                    p2.Y = (int)(-sin_angle2 * max_x + cos_angle2 * min_y + 0.5);
                    p3.X = (int)(cos_angle2 * max_x + sin_angle2 * max_y + 0.5);
                    p3.Y = (int)(-sin_angle2 * max_x + cos_angle2 * max_y + 0.5);
                    p4.X = (int)(cos_angle2 * min_x + sin_angle2 * max_y + 0.5);
                    p4.Y = (int)(-sin_angle2 * min_x + cos_angle2 * max_y + 0.5);

                }
            }

            cornerpoints.Add(p1);
            cornerpoints.Add(p2);
            cornerpoints.Add(p3);
            cornerpoints.Add(p4);
            return cornerpoints;
        }

        public List<double> GetBoundingCircle(List<Point> contour)
        {
            List<double> circledata = new List<double>();

            if (contour == null || contour.Count == 0)
            {
                // invalid contour
                circledata.Add(0.0);
                circledata.Add(0.0);
                circledata.Add(-1.0);
                return circledata;
            }

            List<PointC> pointC_list = new List<PointC>();
            for (int i = 0; i < contour.Count; i++)
            {
                PointC pc = new PointC((double)contour[i].X, (double)contour[i].Y);
                pointC_list.Add((PointC)pc);
            }

            Circle bounding_circle = SmallestEnclosingCircle.MakeCircle(pointC_list);
            circledata.Add(bounding_circle.c.x);
            circledata.Add(bounding_circle.c.y);
            circledata.Add(bounding_circle.r);
            return circledata;
        }

        public List<double> GetEquivalentCircleFromListOfPoints(List<Point> mask_points)
        {
            List<double> circledata = new List<double>();

            if (mask_points == null || mask_points.Count == 0)
            {
                // invalid mask
                circledata.Add(0.0);
                circledata.Add(0.0);
                circledata.Add(-1.0);
                return circledata;
            }

            double sum_x = 0;
            double sum_y = 0;
            for (int i = 0; i < mask_points.Count; i++)
            {
                sum_x += (double)mask_points[i].X + 0.5;  // Add 0.5 because there lies the mass centre of the pixel
                sum_y += (double)mask_points[i].Y + 0.5;  // Add 0.5 because there lies the mass centre of the pixel
            }

            circledata.Add(sum_x / mask_points.Count);
            circledata.Add(sum_y / mask_points.Count);
            circledata.Add(System.Math.Sqrt(mask_points.Count / System.Math.PI));

            return circledata;
        }

        public List<double> GetBoundingEllipse(List<Point> contour)
        {
            List<double> ellipsedata = new List<double>();

            if (contour == null || contour.Count == 0)
            {
                // invalid contour
                ellipsedata.Add(0.0); // Center.X
                ellipsedata.Add(0.0); // Center.Y
                ellipsedata.Add(-1.0); // Major radius
                ellipsedata.Add(-1.0); // Minor radius
                ellipsedata.Add(0.0); // Angle major radius
                return ellipsedata;
            }

            if (contour.Count == 1)
            {
                // Degenerate ellipse based on one point
                ellipsedata.Add((double)contour[0].X); // Center.X
                ellipsedata.Add((double)contour[0].Y); // Center.Y
                ellipsedata.Add(0.5); // Major radius
                ellipsedata.Add(0.5); // Minor radius
                ellipsedata.Add(0.0); // Angle major radius
                return ellipsedata;
            }

            if (contour.Count == 2)
            {
                // Degenerate ellipse based on two points
                ellipsedata.Add(((double)contour[0].X + (double)contour[1].X) / 2.0); // Center.X
                ellipsedata.Add(((double)contour[0].Y + (double)contour[1].Y) / 2.0); // Center.Y
                ellipsedata.Add(System.Math.Sqrt((double)
                    (System.Math.Pow(contour[1].X - contour[0].X, 2)
                    + System.Math.Pow(contour[1].Y - contour[0].Y, 2))) / 2.0); // Major radius
                ellipsedata.Add(0.5); // Minor radius
                ellipsedata.Add(System.Math.Atan2((double)contour[0].Y - ellipsedata[1], (double)contour[0].X - ellipsedata[0])); // Angle major radius
                return ellipsedata;
            }

            double tolerance = 0.001; // 0.2  // The tolerance for error in fitting the ellipse
            int n = contour.Count; // Number of points
            int d = 2; // Dimension of the points

            Matrix<double> Q = Matrix<double>.Build.Dense(3, n);
            for (int i = 0; i < n; i++)
            {
                Q[0, i] = (double)contour[i].X;
                Q[1, i] = (double)contour[i].Y;
                Q[2, i] = 1.0;
            }
            Matrix<double> P = Q.SubMatrix(0, 2, 0, Q.ColumnCount);

            int count = 1;
            double err = 1;

            Vector<double> u = Vector<double>.Build.Dense(n, (i) => 1.0 / (float)n);

            while (err > tolerance)
            {
                Matrix<double> uDiag = Matrix<double>.Build.DenseDiagonal(n, n, (i) => u[i]);

                Matrix<double> X = Q * uDiag * Q.Transpose();
                Matrix<double> Tmp = Q.Transpose() * X.Inverse() * Q;
                Matrix<double> M = Matrix<double>.Build.DenseDiagonal(n, n, (i) => Tmp[i, i]);

                int iMax = 0;
                double maxM = M[0, 0]; // Double.MinValue;
                for (int i = 1; i < n; i++)
                    if (M[i, i] > maxM)
                    {
                        iMax = i;
                        maxM = M[i, i];
                    }

                double step_size = (maxM - d - 1) / ((d + 1) * (maxM + 1));

                Vector<double> new_u = (1 - step_size) * u;
                new_u[iMax] += step_size;

                Vector<double> u_diff = new_u - u;
                err = u_diff.L2Norm();

                // Increment count and replace u
                count = count + 1;
                u = new_u;
            }

            Matrix<double> bigU = Matrix<double>.Build.DenseDiagonal(n, n, (i) => u[i]);
            Matrix<double> c = P * Matrix<double>.Build.DenseOfColumnVectors(Enumerable.Repeat(u, 1));
            Matrix<double> A = (1.0 / (double)d) * (P * bigU * P.Transpose() - c * c.Transpose()).Inverse();

            Svd<double> svd = A.Svd(true);

            double r_a = 1.0 / System.Math.Sqrt(svd.S[0]);
            double r_b = 1.0 / System.Math.Sqrt(svd.S[1]);
            double angle = System.Math.Atan2(svd.U[1, 0], svd.U[0, 0]) * 180 / System.Math.PI;
            if (r_a < r_b)
                angle = System.Math.Atan2(svd.U[0, 0], svd.U[1, 0]) * 180 / System.Math.PI;
            // Correct angle to be in the -90...90 range
            angle = (angle + 90) % 180 - 90;

            ellipsedata.Add(c[0, 0]);
            ellipsedata.Add(c[1, 0]);
            ellipsedata.Add(System.Math.Max(r_a, r_b));
            ellipsedata.Add(System.Math.Min(r_a, r_b));
            ellipsedata.Add(angle);

            return ellipsedata;
        }

        public List<double> GetEllipseBasedOn2ndCentralMoment(List<Point> mask_points)
        {
            List<double> ellipsedata = new List<double>();

            if (mask_points == null || mask_points.Count == 0)
            {
                // invalid contour
                ellipsedata.Add(0.0); // Center.X
                ellipsedata.Add(0.0); // Center.Y
                ellipsedata.Add(-1.0); // Major radius
                ellipsedata.Add(-1.0); // Minor radius
                ellipsedata.Add(-1.0); // Angle major radius
                ellipsedata.Add(-1000.0); // Area
                return ellipsedata;
            }

            if (mask_points.Count == 1)
            {
                // Degenerate ellipse based on one point
                ellipsedata.Add((double)mask_points[0].X + 0.5); // Center.X
                ellipsedata.Add((double)mask_points[0].Y + 0.5); // Center.Y
                ellipsedata.Add(0.5); // Major radius
                ellipsedata.Add(0.5); // Minor radius
                ellipsedata.Add(0.0); // Angle major radius
                ellipsedata.Add((double)mask_points.Count); // Area
                return ellipsedata;
            }

            double M00 = (double)mask_points.Count;
            double M01 = 0.0;
            double M10 = 0.0;
            double M02 = 0.0;
            double M20 = 0.0;
            double M11 = 0.0;
            for (int i = 0; i < mask_points.Count; i++)
            {
                M01 += mask_points[i].Y;
                M10 += mask_points[i].X;
                M02 += mask_points[i].Y * mask_points[i].Y;
                M20 += mask_points[i].X * mask_points[i].X;
                M11 += mask_points[i].X * mask_points[i].Y;
            }
            ellipsedata.Add(M10 / M00);  // Center.X
            ellipsedata.Add(M01 / M00);  // Center.Y

            double mu_20 = M20 / M00 - ellipsedata[0] * ellipsedata[0];
            double mu_02 = M02 / M00 - ellipsedata[1] * ellipsedata[1];
            double mu_11 = M11 / M00 - ellipsedata[0] * ellipsedata[1];

            double delta = System.Math.Sqrt(4 * mu_11 * mu_11 + (mu_20 - mu_02) * (mu_20 - mu_02));
            double lambda_1 = ((mu_20 + mu_02) + delta) / 2.0;
            double lambda_2 = ((mu_20 + mu_02) - delta) / 2.0;

            ellipsedata.Add(System.Math.Sqrt(2 * System.Math.Abs(lambda_1)));  // Major radius
            ellipsedata.Add(System.Math.Sqrt(2 * System.Math.Abs(lambda_2)));  // Minor radius
            double angle = 0.0;
            if (mu_20 != mu_02)
                angle = 0.5 * System.Math.Atan((2.0 * mu_11) / (System.Math.Sign(mu_20 - mu_02) * (mu_20 - mu_02))) * 180.0 / System.Math.PI;
            angle = (angle + 90) % 180;  // - 90;  // Correct angle to be in the -90...90 range
            ellipsedata.Add(angle);  // Angle major radius
            ellipsedata.Add(M00);  // Area

            return ellipsedata;
        }


        // corner_points are the corner points of the bounding box
        public Point[] GetPlantWidth(List<Point> corner_points)
        {
            Point[] endpoints = new Point[2];

            if (corner_points == null || corner_points.Count == 0)
                return endpoints;

            double dist1 = System.Math.Sqrt(System.Math.Pow((double)(corner_points[0].X - corner_points[1].X), 2.0) +
                System.Math.Pow((double)(corner_points[0].Y - corner_points[1].Y), 2.0));
            double dist2 = System.Math.Sqrt(System.Math.Pow((double)(corner_points[0].X - corner_points[3].X), 2.0) +
                System.Math.Pow((double)(corner_points[0].Y - corner_points[3].Y), 2.0));
            // Create a line between the midpoints of the sides with the smallest size
            if (dist1 < dist2)
            {
                endpoints[0].X = (corner_points[0].X + corner_points[1].X) / 2;
                endpoints[0].Y = (corner_points[0].Y + corner_points[1].Y) / 2;
                endpoints[1].X = (corner_points[2].X + corner_points[3].X) / 2;
                endpoints[1].Y = (corner_points[2].Y + corner_points[3].Y) / 2;
            }
            else
            {
                endpoints[0].X = (corner_points[0].X + corner_points[3].X) / 2;
                endpoints[0].Y = (corner_points[0].Y + corner_points[3].Y) / 2;
                endpoints[1].X = (corner_points[1].X + corner_points[2].X) / 2;
                endpoints[1].Y = (corner_points[1].Y + corner_points[2].Y) / 2;
            }

            return endpoints;
        }

        public double GetAreaInsideCircle(List<Point> mask_points, double center_x, double center_y, double radius)
        {
            double area_inside_circle = 0;

            for (int i = 0; i < mask_points.Count; i++)
            {
                double distance_to_circle_center = 
                    System.Math.Sqrt(
                        System.Math.Pow((double)mask_points[i].X + 0.5 - center_x, 2) 
                        + System.Math.Pow((double)mask_points[i].Y + 0.5 - center_y, 2));
                if (distance_to_circle_center <= radius)
                    area_inside_circle += 1.0;
            }

            return area_inside_circle;
        }

        public List<double> AdjustFeature(List<double> feature, List<int> rep_lengths)
        {
            List<double> adjusted_feature = new List<double>();

            int start_index = 0;
            for (int i = 0; i < rep_lengths.Count; i++)
            {
                int rep_length = rep_lengths[i];
                for (int j = start_index; j < (start_index + rep_length); j++)
                {
                    if (j == start_index)
                    {
                        // first item is accepted anyway
                        adjusted_feature.Add(feature[j]);
                    }
                    else if (j == (start_index + rep_length - 1))
                    {
                        if (j > 2 && feature[j] < adjusted_feature[j - 1])
                            adjusted_feature.Add(adjusted_feature[j - 1] + (adjusted_feature[j - 1] - adjusted_feature[j - 2]));
                        else
                            adjusted_feature.Add(feature[j]);
                    }
                    else
                    {
                        // middle item is replaced, if smaller than the previous one
                        if (feature[j] < adjusted_feature[j - 1])
                        {
                            double previous_value = adjusted_feature[j - 1];
                            int k = j + 1;
                            while (k < (start_index + rep_length - 1) && (feature[k] < previous_value))
                                k++;
                            if (k == (start_index + rep_length - 1))
                            {
                                if (j < (start_index + 2))
                                    adjusted_feature.Add(feature[j]);
                                else
                                    adjusted_feature.Add(adjusted_feature[j - 1] + (adjusted_feature[j - 1] - adjusted_feature[j - 2]));
                            }
                            else
                            {
                                double next_value = feature[k];
                                adjusted_feature.Add(((double)(k - j) * previous_value + next_value) / (double)(k - j + 1));
                            }
                        }
                        else
                            adjusted_feature.Add(feature[j]);
                    }
                }
                start_index += rep_length;
            }

            return adjusted_feature;
        }

        public List<double> SmoothFeature(List<double> feature, List<int> rep_lengths)
        {
            List<double> smoothed_feature = new List<double>();
            int start_index = 0;
            for (int i = 0; i < rep_lengths.Count; i++)
            {
                int rep_length = rep_lengths[i];
                for (int j = start_index; j < (start_index + rep_length); j++)
                {
                    if (j == start_index || j == (start_index + rep_length - 1))
                        smoothed_feature.Add(feature[j]);
                    else
                        smoothed_feature.Add((feature[j-1]+ feature[j]+ feature[j+1])/3.0);
                }
                start_index += rep_length;
            }

            return smoothed_feature;
        }


        public List<Point> MaskToListOfPoints(Bitmap mask)
        {
            List<Point> mask_points = new List<Point>();

            BitmapData mask_data = mask.LockBits(
                new Rectangle(0, 0, mask.Width, mask.Height),
                ImageLockMode.ReadOnly,
                mask.PixelFormat);

            IntPtr ptr1 = mask_data.Scan0;
            int bytespp = 4;    // fixed (32bpp)
            int inputstride = (mask.Width * bytespp + 3) / 4 * 4;
            int nbofinputbytes = inputstride * mask.Height;
            byte[] inp = new byte[nbofinputbytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr1, inp, 0, nbofinputbytes);

            for (int y = 0; y < mask.Height; y++)
                for (int x = 0; x < mask.Width; x++)
                    if (inp[y * inputstride + x * bytespp] > 0)
                        mask_points.Add(new Point(x, y));

            mask.UnlockBits(mask_data);

            return mask_points;
        }


        public List<Point> FillContour(List<Point> contour, int img_width, int img_height)
        {
            // Create Bitmap class object
            Bitmap filled_contour_bitmap = new Bitmap(img_width, img_height, PixelFormat.Format32bppPArgb);

            // Initialize a Graphics class instance
            Graphics graphics = Graphics.FromImage(filled_contour_bitmap);

            // Create a brush while specifying its color
            Brush brush = new SolidBrush(Color.FromKnownColor(KnownColor.White));

            // Create the filled polygon
            graphics.FillPolygon(brush, contour.ToArray());

            return MaskToListOfPoints(filled_contour_bitmap);
        }
    }

    //-------------------------------------------------------------------------
    public class SmallestEnclosingCircle // public sealed class SmallestEnclosingCircle
    {
        public static Circle MakeCircle(List<PointC> points)
        {
            // Clone list to preserve the caller's data, do Durstenfeld shuffle
            List<PointC> shuffled = new List<PointC>(points);
            Random rand = new Random();
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                PointC temp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = temp;
            }

            // Progressively add points to circle or recompute circle
            Circle c = Circle.INVALID;
            for (int i = 0; i < shuffled.Count; i++)
            {
                PointC p = shuffled[i];
                if (c.r < 0 || !c.Contains(p))
                    c = MakeCircleOnePoint(shuffled.GetRange(0, i + 1), p);
            }
            return c;
        }

        private static Circle MakeCircleOnePoint(List<PointC> points, PointC p)
        {
            Circle c = new Circle(p, 0);
            for (int i = 0; i < points.Count; i++)
            {
                PointC q = points[i];
                if (!c.Contains(q))
                {
                    if (c.r == 0)
                        c = MakeDiameter(p, q);
                    else
                        c = MakeCircleTwoPoints(points.GetRange(0, i + 1), p, q);
                }
            }
            return c;
        }

        private static Circle MakeCircleTwoPoints(List<PointC> points, PointC p, PointC q)
        {
            Circle circ = MakeDiameter(p, q);
            Circle left = Circle.INVALID;
            Circle right = Circle.INVALID;

            // For each point not in the two-point circle
            PointC pq = q.Subtract(p);
            foreach (PointC r in points)
            {
                if (circ.Contains(r))
                    continue;

                // Form a circumcircle and classify it on left or right side
                double cross = pq.Cross(r.Subtract(p));
                Circle c = MakeCircumcircle(p, q, r);
                if (c.r < 0)
                    continue;
                else if (cross > 0 && (left.r < 0 || pq.Cross(c.c.Subtract(p)) > pq.Cross(left.c.Subtract(p))))
                    left = c;
                else if (cross < 0 && (right.r < 0 || pq.Cross(c.c.Subtract(p)) < pq.Cross(right.c.Subtract(p))))
                    right = c;
            }

            // Select which circle to return
            if (left.r < 0 && right.r < 0)
                return circ;
            else if (left.r < 0)
                return right;
            else if (right.r < 0)
                return left;
            else
                return left.r <= right.r ? left : right;
        }

        public static Circle MakeDiameter(PointC a, PointC b)
        {
            PointC c = new PointC((a.x + b.x) / 2, (a.y + b.y) / 2);
            return new Circle(c, System.Math.Max(c.Distance(a), c.Distance(b)));
        }
        public static Circle MakeCircumcircle(PointC a, PointC b, PointC c)
        {
            // Mathematical algorithm: Circumscribed circle
            double ox = (System.Math.Min(System.Math.Min(a.x, b.x), c.x) + System.Math.Max(System.Math.Max(a.x, b.x), c.x)) / 2;
            double oy = (System.Math.Min(System.Math.Min(a.y, b.y), c.y) + System.Math.Max(System.Math.Max(a.y, b.y), c.y)) / 2;
            double ax = a.x - ox, ay = a.y - oy;
            double bx = b.x - ox, by = b.y - oy;
            double cx = c.x - ox, cy = c.y - oy;
            double d = (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by)) * 2;
            if (d == 0)
                return Circle.INVALID;
            double x = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
            double y = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;
            PointC p = new PointC(ox + x, oy + y);
            double r = System.Math.Max(System.Math.Max(p.Distance(a), p.Distance(b)), p.Distance(c));
            return new Circle(p, r);
        }
    }


    public struct PointC
    {

        public double x;
        public double y;


        public PointC(double x, double y)
        {
            this.x = x;
            this.y = y;
        }


        public PointC Subtract(PointC p)
        {
            return new PointC(x - p.x, y - p.y);
        }


        public double Distance(PointC p)
        {
            double dx = x - p.x;
            double dy = y - p.y;
            return System.Math.Sqrt(dx * dx + dy * dy);
        }


        // Signed area / determinant thing
        public double Cross(PointC p)
        {
            return x * p.y - y * p.x;
        }
    }

    public struct Circle
    {

        public static readonly Circle INVALID = new Circle(new PointC(0, 0), -1);

        private const double MULTIPLICATIVE_EPSILON = 1 + 1e-14;


        public PointC c;   // Center
        public double r;  // Radius


        public Circle(PointC c, double r)
        {
            this.c = c;
            this.r = r;
        }


        public bool Contains(PointC p)
        {
            return c.Distance(p) <= r * MULTIPLICATIVE_EPSILON;
        }


        public bool Contains(ICollection<PointC> ps)
        {

            foreach (PointC p in ps)
            {
                if (!Contains(p))
                    return false;
            }
            return true;
        }

    }

}
