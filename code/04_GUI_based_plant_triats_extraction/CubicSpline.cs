// Original Author: Ryan Seghers
// Original Copyright (C) 2013-2014 Ryan Seghers
// Original License: MIT https://opensource.org/licenses/MIT
// Original Source Code: https://github.com/SCToolsfactory/SCJMapper-V2/tree/master/OGL
// Related Article: https://www.codeproject.com/Articles/560163/Csharp-Cubic-Spline-Interpolation
// Modified by: Scott W Harden in 2022 (released under MIT license)
// Modified by Csaba Hegedus in 2022

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace PlantInspector
{
    public class CubicSpline
    {
        /// <summary>
        /// Generate a smooth (interpolated) curve that follows the path of the given X/Y points
        /// </summary>
        public void InterpolateXY(double[] x_i, double[] y_i, int count, ref double[] x_o, ref double[] y_o)
        {
            if (x_i == null || y_i == null || x_i.Length != y_i.Length)
                return;     // REM: do nothing if improper input data

            int inputPointCount = x_i.Length;
            double[] inputDistances = new double[inputPointCount];
            for (int i = 1; i < inputPointCount; i++)
            {
                double dx = x_i[i] - x_i[i - 1];
                double dy = y_i[i] - y_i[i - 1];
                double distance = System.Math.Sqrt(dx * dx + dy * dy);
                inputDistances[i] = inputDistances[i - 1] + distance;
            }

            double meanDistance = inputDistances.Last() / (count - 1);
            double[] evenDistances = Enumerable.Range(0, count).Select(x => x * meanDistance).ToArray();
            x_o = Interpolate(inputDistances, x_i, evenDistances);
            y_o = Interpolate(inputDistances, y_i, evenDistances);
        }

        private double[] Interpolate(double[] xOrig, double[] yOrig, double[] xInterp)
        {
            int n = xOrig.Length;
            double[] a = new double[n - 1];
            double[] b = new double[n - 1];
            FitMatrix(xOrig, yOrig, ref a, ref b);

            double[] yInterp = new double[xInterp.Length];
            for (int i = 0; i < yInterp.Length; i++)
            {
                int j;
                for (j = 0; j < xOrig.Length - 2; j++)
                    if (xInterp[i] <= xOrig[j + 1])
                        break;

                double dx = xOrig[j + 1] - xOrig[j];
                double t = (xInterp[i] - xOrig[j]) / dx;
                double y = (1 - t) * yOrig[j] + t * yOrig[j + 1] +
                    t * (1 - t) * (a[j] * (1 - t) + b[j] * t);
                yInterp[i] = y;
            }

            return yInterp;
        }

        private void FitMatrix(double[] x, double[] y, ref double[] a, ref double[] b)
        {
            int n = x.Length;
            double[] r = new double[n];
            double[] A = new double[n];
            double[] B = new double[n];
            double[] C = new double[n];

            double dx1, dx2, dy1, dy2;

            dx1 = x[1] - x[0];
            C[0] = 1.0f / dx1;
            B[0] = 2.0f * C[0];
            r[0] = 3 * (y[1] - y[0]) / (dx1 * dx1);

            for (int i = 1; i < n - 1; i++)
            {
                dx1 = x[i] - x[i - 1];
                dx2 = x[i + 1] - x[i];
                A[i] = 1.0f / dx1;
                C[i] = 1.0f / dx2;
                B[i] = 2.0f * (A[i] + C[i]);
                dy1 = y[i] - y[i - 1];
                dy2 = y[i + 1] - y[i];
                r[i] = 3 * (dy1 / (dx1 * dx1) + dy2 / (dx2 * dx2));
            }

            dx1 = x[n - 1] - x[n - 2];
            dy1 = y[n - 1] - y[n - 2];
            A[n - 1] = 1.0f / dx1;
            B[n - 1] = 2.0f * A[n - 1];
            r[n - 1] = 3 * (dy1 / (dx1 * dx1));

            double[] cPrime = new double[n];
            cPrime[0] = C[0] / B[0];
            for (int i = 1; i < n; i++)
                cPrime[i] = C[i] / (B[i] - cPrime[i - 1] * A[i]);

            double[] dPrime = new double[n];
            dPrime[0] = r[0] / B[0];
            for (int i = 1; i < n; i++)
                dPrime[i] = (r[i] - dPrime[i - 1] * A[i]) / (B[i] - cPrime[i - 1] * A[i]);

            double[] k = new double[n];
            k[n - 1] = dPrime[n - 1];
            for (int i = n - 2; i >= 0; i--)
                k[i] = dPrime[i] - cPrime[i] * k[i + 1];

            for (int i = 1; i < n; i++)
            {
                dx1 = x[i] - x[i - 1];
                dy1 = y[i] - y[i - 1];
                a[i - 1] = k[i - 1] * dx1 - dy1;
                b[i - 1] = -k[i] * dx1 + dy1;
            }
        }
    }
}