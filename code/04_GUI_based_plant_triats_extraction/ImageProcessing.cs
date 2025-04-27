using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using System.Drawing;
using System.Drawing.Imaging;
using static System.Net.Mime.MediaTypeNames;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using static Plotly.NET.StyleParam;
using OxyPlot.Reporting;


namespace PlantInspector
{


    public class ImageProcessing
    {
        public enum DIRECTION
        {
            DIR_1 = 1,              // to the right
            DIR_2,                  // to the right-bottom
            DIR_3,                  // to the bottom
            DIR_4,                  // to the left-bottom
            DIR_5,                  // to the left
            DIR_6,                  // to the left-top
            DIR_7,                  // to the top
            DIR_8					// to the right-top

        };


        //------------------------------------------------------------------------------
        public (int, int) GetClosestMaskPoint(int pt_col, int pt_row, Image<Gray, Byte> labelsImg, int currentLabel, int[] statsData, Mat stats)
        {
            int bestCol = pt_col;
            int bestRow = pt_row;
            double minDistToCenter = 1 + labelsImg.Width + labelsImg.Height;

            for (int col = statsData[currentLabel * stats.Cols + 0];
                col < statsData[currentLabel * stats.Cols + 0] + statsData[currentLabel * stats.Cols + 2];
                col++)
                for (int row = statsData[currentLabel * stats.Cols + 1];
                    row < statsData[currentLabel * stats.Cols + 1] + statsData[currentLabel * stats.Cols + 3];
                    row++)
                    if (labelsImg.Data[row, col, 0] == currentLabel)
                    {
                        double newDist = Math.Sqrt((col - pt_col) * (col - pt_col) + (row - pt_row) * (row - pt_row));
                        if (newDist < minDistToCenter)
                        {
                            minDistToCenter = newDist;
                            bestCol = col;
                            bestRow = row;
                        }
                    }
            return (bestCol, bestRow);
        }


        public (Bitmap, Bitmap) GetFilteredMask(Bitmap inputMask, int connectivity, int noise_threshold)
        {
            Image<Bgra, Byte> emguInputMask = inputMask.ToImage<Bgra, Byte>();
            Image<Gray, Byte> emguGrayInputMask = emguInputMask.Convert<Gray, Byte>();
            Mat labelsMask = new Mat();
            Mat stats = new Mat();
            Mat centroids = new Mat();
            LineType emguConnectivity = (connectivity == 8) ? LineType.EightConnected : LineType.FourConnected;
            int nLabels = CvInvoke.ConnectedComponentsWithStats(emguGrayInputMask.Mat, labelsMask, stats, centroids, emguConnectivity);
            Image<Gray, Byte> labelsImg = labelsMask.ToImage<Gray, Byte>();
            Image<Gray, Byte> noNoiseImg = new Image<Gray, Byte>(labelsMask.Width, labelsMask.Height, new Gray(0));
            int[] statsData = new int[stats.Rows * stats.Cols];
            stats.CopyTo(statsData);
            int largestLabel = -1;
            int largestLabelArea = -1;
            int minCol = labelsMask.Width;
            int minRow = labelsMask.Height;
            int maxCol = -1;
            int maxRow = -1;
            HashSet<int> selLabels = new HashSet<int>();
            for (int label = 1; label < nLabels; label++)
            {
                if (statsData[label * stats.Cols + 4] > noise_threshold)
                {
                    selLabels.Add(label);
                    noNoiseImg.SetValue(new Gray(255), labelsImg.Cmp(label, CmpType.Equal));
                    if (statsData[label * stats.Cols + 4] > largestLabelArea)
                    {
                        largestLabel = label;
                        largestLabelArea = statsData[label * stats.Cols + 4];
                    }
                    if (minCol > statsData[label * stats.Cols + 0])
                        minCol = statsData[label * stats.Cols + 0];
                    if (minRow > statsData[label * stats.Cols + 1])
                        minRow = statsData[label * stats.Cols + 1];
                    if (maxCol < statsData[label * stats.Cols + 0] + statsData[label * stats.Cols + 2])
                        maxCol = statsData[label * stats.Cols + 0] + statsData[label * stats.Cols + 2];
                    if (maxRow < statsData[label * stats.Cols + 1] + statsData[label * stats.Cols + 3])
                        maxRow = statsData[label * stats.Cols + 1] + statsData[label * stats.Cols + 3];
                }
            }
            int midCol = (int)Math.Round((minCol + maxCol) / 2.0, 0);
            int midRow = (int)Math.Round((minRow + maxRow) / 2.0, 0);
            int plantCenterCol = midCol;
            int plantCenterRow = midRow;
            double minDistToCenter = 1 + maxCol + maxRow;
            if (labelsImg.Data[midRow, midCol, 0] != largestLabel)
                (plantCenterCol, plantCenterRow) = GetClosestMaskPoint(midCol, midRow, labelsImg, largestLabel, statsData, stats);
            Image<Gray, Byte> imgWithStems = noNoiseImg.Clone();
            foreach (int label in selLabels)
                if (label != largestLabel)
                {
                    int closestCol, closestRow;
                    (closestCol, closestRow) = GetClosestMaskPoint(plantCenterCol, plantCenterRow, labelsImg, label, statsData, stats);
                    imgWithStems.Draw(
                        new LineSegment2D(new Point(plantCenterCol, plantCenterRow), new Point(closestCol, closestRow)),
                        new Gray(255),
                        2);
                }

            return (noNoiseImg.Convert<Bgra, Byte>().ToBitmap(), imgWithStems.Convert<Bgra, Byte>().ToBitmap());
        }


        public List<Point> GetPlantContour(Bitmap input_bitmap)
        {
            List<Point> contour_points = new List<Point>();
            contour_points.Clear();

            if (input_bitmap == null || input_bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                return contour_points;

            // get the pixels of input bitmap
            Rectangle input_rect = new Rectangle(0, 0, input_bitmap.Width, input_bitmap.Height);
            BitmapData input_data = input_bitmap.LockBits(input_rect, ImageLockMode.ReadOnly, input_bitmap.PixelFormat);
            IntPtr ptr1 = input_data.Scan0;
            int bytespp = 4;    
            int inputstride = (input_data.Width * bytespp + 3) / 4 * 4;
            int nbofinputbytes = inputstride * input_data.Height;
            byte[] inp = new byte[nbofinputbytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr1, inp, 0, nbofinputbytes);

            Boolean bContinue = true;
            int x0 = 0;
            int y0 = 0;

            while (bContinue == true)
            {
                List<Point> new_spot = GetNextSpot(inp, bytespp, input_bitmap.Width, input_bitmap.Height, inputstride, ref x0, ref y0);
                if (new_spot.Count == 0)
                    bContinue = false;
                else
                {
                    for (int i = 0; i < new_spot.Count; i++)
                        contour_points.Add(new_spot[i]);
                    
                    bContinue = false;  
                }
            }

            input_bitmap.UnlockBits(input_data);
            return contour_points;
        }


        public List<Point> GetPlantContour_OpenCV(Bitmap input_bitmap)
        {
            List<Point> contour_points = new List<Point>();
            contour_points.Clear();

            if (input_bitmap == null || input_bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                return contour_points;

            Image<Bgra, Byte> emguInputMask = input_bitmap.ToImage<Bgra, Byte>();
            Image<Gray, Byte> emguGrayInputMask = emguInputMask.Convert<Gray, Byte>();
            Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
            Mat hier = new Mat();
            CvInvoke.FindContours(
                emguGrayInputMask, contours, hier,
                Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            contour_points.AddRange(contours[0].ToArray());

            return contour_points;
        }

        public List<Point> GetNextSpot(byte[] inp, int bytespp, int width, int height, int inputstride, ref int x0, ref int y0)
        {
            List<Point> spot = new List<Point>();
            spot.Clear();


            // search for the first object position (walking down vertically in a middle band)
            Boolean bPlant = false;
            while (bPlant == false && y0 < 3 * height / 4)
            {
                y0++;
                x0 = 0; //  input_bitmap.Width / 4;
                while (bPlant == false && x0 < width)
                {
                    x0++;
                    if (inp[bytespp * x0 + 1 + y0 * inputstride] == 255)
                        bPlant = true;
                }
            }
            Point pt0 = new Point(x0, y0);
            spot.Add(pt0);
            int nbofpoints = 1;

            // Follow the contour - in clockwise manner
            int xc = x0;
            int yc = y0;

            DIRECTION dir = DIRECTION.DIR_6;   // starting direction
            bool bClosed = false;
            bool bContinuationFound = true;
            int TryCounter = 0;
            while (!bClosed)
            {
                bContinuationFound = false;
                int x = -1;
                int y = -1;
                switch (dir)
                {
                    case DIRECTION.DIR_1:  // look for edge point to the right
                    default:
                        x = xc + 1; y = yc;
                        if (x >= 0 && x < width && y >= 0 && y < height && inp[bytespp * x + 1 + y * inputstride] == 255)
                        {
                            bContinuationFound = true;
                            dir = DIRECTION.DIR_6;
                        }
                        else
                            dir = DIRECTION.DIR_2;
                        break;
                    case DIRECTION.DIR_2:  // look for edge point to the right-bottom
                        x = xc + 1; y = yc + 1;
                        if (x >= 0 && x < width && y >= 0 && y < height && inp[bytespp * x + 1 + y * inputstride] == 255)
                        {
                            bContinuationFound = true;
                            dir = DIRECTION.DIR_7;
                        }
                        else
                            dir = DIRECTION.DIR_3;
                        break;
                    case DIRECTION.DIR_3:  // look for edge point to the bottom
                        x = xc; y = yc + 1;
                        if (x >= 0 && x < width && y >= 0 && y < height && inp[bytespp * x + 1 + y * inputstride] == 255)
                        {
                            bContinuationFound = true;
                            dir = DIRECTION.DIR_8;
                        }
                        else
                            dir = DIRECTION.DIR_4;
                        break;
                    case DIRECTION.DIR_4:  // look for edge point to the left-bottom
                        x = xc - 1; y = yc + 1;
                        if (x >= 0 && x < width && y >= 0 && y < height && inp[bytespp * x + 1 + y * inputstride] == 255)
                        {
                            bContinuationFound = true;
                            dir = DIRECTION.DIR_1;
                        }
                        else
                            dir = DIRECTION.DIR_5;
                        break;
                    case DIRECTION.DIR_5:  // look for edge point to the left
                        x = xc - 1; y = yc;
                        if (x >= 0 && x < width && y >= 0 && y < height && inp[bytespp * x + 1 + y * inputstride] == 255)
                        {
                            bContinuationFound = true;
                            dir = DIRECTION.DIR_2;
                        }
                        else
                            dir = DIRECTION.DIR_6;
                        break;
                    case DIRECTION.DIR_6:  // look for edge point to the left-top
                        x = xc - 1; y = yc - 1;
                        if (x >= 0 && x < width && y >= 0 && y < height && inp[bytespp * x + 1 + y * inputstride] == 255)
                        {
                            bContinuationFound = true;
                            dir = DIRECTION.DIR_3;
                        }
                        else
                            dir = DIRECTION.DIR_7;
                        break;
                    case DIRECTION.DIR_7:  // look for edge point to the top
                        x = xc; y = yc - 1;
                        if (x >= 0 && x < width && y >= 0 && y < height && inp[bytespp * x + 1 + y * inputstride] == 255)
                        {
                            bContinuationFound = true;
                            dir = DIRECTION.DIR_4;
                        }
                        else
                            dir = DIRECTION.DIR_8;
                        break;
                    case DIRECTION.DIR_8:  // look for edge point to the right-top
                        x = xc + 1; y = yc - 1;
                        if (x >= 0 && x < width && y >= 0 && y < height && inp[bytespp * x + 1 + y * inputstride] == 255)
                        {
                            bContinuationFound = true;
                            dir = DIRECTION.DIR_5;
                        }
                        else
                            dir = DIRECTION.DIR_1;
                        break;
                }
                TryCounter++;

                if (bContinuationFound || TryCounter == 8)
                {
                    if (x != -1 && y != -1)
                    {
                        Point pt = new Point(x, y);
                        spot.Add(pt);
                        nbofpoints++;
                        xc = x; yc = y;
                    }
                    TryCounter = 0;

                    // check/finish, if closed
                    if (nbofpoints > 3)
                    {
                        if (System.Math.Abs(x0 - x) < 2 && System.Math.Abs(y0 - y) < 2)
                            bClosed = true;
                    }
                }

            }

            return spot;
        }

        public double GetAreaBasedOnMask(Bitmap input_bitmap)
        {
            double counter = 0;

            if (input_bitmap == null || input_bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                return counter;

            // get the pixels of input bitmap
            Rectangle input_rect = new Rectangle(0, 0, input_bitmap.Width, input_bitmap.Height);
            BitmapData input_data = input_bitmap.LockBits(input_rect, ImageLockMode.ReadOnly, input_bitmap.PixelFormat);
            IntPtr ptr1 = input_data.Scan0;
            int bytespp = 4;   
            int inputstride = (input_data.Width * bytespp + 3) / 4 * 4;
            int nbofinputbytes = inputstride * input_data.Height;
            byte[] inp = new byte[nbofinputbytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr1, inp, 0, nbofinputbytes);

            for (int i = 0; i < inp.Length; i += bytespp)
            {
                // Byte 0 holds transparency, we only check bytes 0, 1, and 2
                if ((inp[i] == 255) || (inp[i + 1] == 255) || (inp[i + 2] == 255))
                    counter++;
            }

            input_bitmap.UnlockBits(input_data);
            return counter;

        }


        public List<double> GetEquivalentCircleFromBitmap(Bitmap input_bitmap)
        {
            List<double> circledata = new List<double>();

            BitmapData imageData = input_bitmap.LockBits(
                new Rectangle(0, 0, input_bitmap.Width, input_bitmap.Height),
                ImageLockMode.ReadOnly,
                input_bitmap.PixelFormat);
            IntPtr ptr1 = imageData.Scan0;
            int bytespp = 4;   
            int inputstride = (input_bitmap.Width * bytespp + 3) / 4 * 4;
            int nbofinputbytes = inputstride * input_bitmap.Height;
            byte[] inp = new byte[nbofinputbytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr1, inp, 0, nbofinputbytes);

            int n_points = 0;
            double sum_x = 0;
            double sum_y = 0;
            for (int y = 0; y < input_bitmap.Height; y++)
                for (int x = 0; x < input_bitmap.Width; x++)
                    if (inp[y * inputstride + x * bytespp] > 0)
                    {
                        n_points++;
                        sum_x += (double)x + 0.5;  // Add 0.5 because there lies the mass centre of the pixel
                        sum_y += (double)y + 0.5;  // Add 0.5 because there lies the mass centre of the pixel
                    }

            input_bitmap.UnlockBits(imageData);

            if (n_points == 0)
            {
                // invalid mask
                circledata.Add(0.0);
                circledata.Add(0.0);
                circledata.Add(-1.0);
                circledata.Add(-1.0);
            }
            else
            {
                circledata.Add(sum_x / n_points);
                circledata.Add(sum_y / n_points);
                circledata.Add(System.Math.Sqrt(n_points / System.Math.PI));
                circledata.Add(n_points);
            }

            return circledata;
        }

        public void SaveBitmap(Bitmap Input_Bitmap, string FileName, System.Drawing.Imaging.ImageFormat format)
        {
            if (Input_Bitmap == null)
                return;

            // Due to bitmap saving behaviour, deep copy must be created for saving
            Bitmap tmp_bmp = new Bitmap(Input_Bitmap);

            System.Drawing.Image img = new Bitmap(tmp_bmp);
            img.Save(FileName, format);

            tmp_bmp.Dispose();
        }

    }
}
