using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Python.Runtime;

// ReSharper disable All

// Author: 武泽恺10225101429
namespace AppApi
{
    public class Apis
    {
        public static Mat LoadImage(string path, bool resize)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            var image = Cv2.ImRead(path, ImreadModes.Unchanged);
            if(resize)image = image.Resize(new Size(500, 500));
            return image;
        }
        
        public static void ShowImage(params Mat[] image)
        {
            var cnt = 1;
            foreach (var img in image)
            {
                Cv2.ImShow($"Image{cnt++}", img);
                Cv2.WaitKey();
            }
        }
        
        public static void ShowImageNoWait(params Mat[] image)
        {
            var cnt = 1;
            foreach (var img in image)
            {
                Cv2.ImShow($"Image{cnt++}", img);
            }
            Cv2.WaitKey(0);
        }
        
        public static Mat[] AllAlphas(Mat image)
        {
            return Cv2.Split(image);
        }
        
        public static Mat ToHsv(Mat image)
        {
            Cv2.CvtColor(image, image, ColorConversionCodes.BGR2HSV);
            return image;
        }

        public static void SaveImage(Mat image, string path)
        {
            Cv2.ImWrite(path, image);
        }
        
        public static Mat ToGray(Mat image)
        {
            if(image.Channels() > 1) Cv2.CvtColor(image, image, ColorConversionCodes.BGR2GRAY);
            return image;
        }
        
        public static Mat And(Mat img1, Mat img2)
        {
            var res = new Mat();
            Cv2.BitwiseAnd(img1, img2, res);
            return res;
        }
        
        public static Mat Or(Mat img1, Mat img2)
        {
            var res = new Mat();
            Cv2.BitwiseOr(img1, img2, res);
            return res;
        }
        
        public static Mat Add(Mat img1, Mat img2)
        {
            var res = new Mat();
            Cv2.Add(img1, img2, res);
            return res;
        }
        
        public static Mat Sub(Mat img1, Mat img2)
        {
            var res = new Mat();
            Cv2.Subtract(img1, img2, res);
            return res;
        }
        
        public static Mat Mul(Mat img1, Mat img2)
        {
            var res = new Mat();
            Cv2.Multiply(img1, img2, res);
            return res;
        }
        
        public static Mat Div(Mat img1, Mat img2)
        {
            var res = new Mat();
            Cv2.Divide(img1, img2, res);
            return res;
        }
        
        public static Mat Resize(Mat img, int width, int height)
        {
            Cv2.Resize(img, img, new Size(width, height));
            return img;
        }
        
        public static Mat Rotate(Mat img, double angle)
        {
            var center = new Point2f(img.Width / 2.0f, img.Height / 2.0f);
            var rot = Cv2.GetRotationMatrix2D(center, angle, 1.0);
            Cv2.WarpAffine(img, img, rot, new Size(img.Width, img.Height));
            return img;
        }

        public static Mat Translate(Mat img, int dx, int dy)
        {
            var mat = new Mat(2, 3, MatType.CV_32F);
            mat.Set<float>(0, 0, 1);
            mat.Set<float>(0, 1, 0);
            mat.Set<float>(0, 2, dx);
            mat.Set<float>(1, 0, 0);
            mat.Set<float>(1, 1, 1);
            mat.Set<float>(1, 2, dy);
            
            Cv2.WarpAffine(img, img, mat, new Size(img.Width, img.Height));
            return img;
        }

        //
        public static Mat Flip(Mat img, FlipMode flipCode)
        {
            Cv2.Flip(img, img, flipCode);
            return img;
        }
        
        // [[50, 50], [200, 50], [50, 200]] [[10, 100], [200, 50], [100, 250]]
        public static Mat AffineTransform(Mat img, Point2f[] src, Point2f[] dst)
        {
            var mat = Cv2.GetAffineTransform(src, dst);
            Cv2.WarpAffine(img, img, mat, new Size(img.Width, img.Height));
            return img;
        }
        
        [Obsolete]
        public static Mat Dft(Mat img)
        {
            var gray = new Mat();
            Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
            gray.ConvertTo(gray, MatType.CV_32FC1);
            var planes = new Mat[] { img, Mat.Zeros(img.Size(), MatType.CV_32F) };
            var complexImg = new Mat();
            Cv2.Merge(planes, complexImg);

            Cv2.Dft(complexImg, complexImg);
            Cv2.Split(complexImg, out planes);
            Cv2.Magnitude(planes[0], planes[1], img);
            Cv2.Log(img, img);
            Cv2.Normalize(img, img, 0, 255, NormTypes.MinMax);
            return img;
        }
        
        private static Mat DftShift(Mat img)
        {
            img = img[Rect.FromLTRB(0, 0, img.Cols & -2, img.Rows & -2)];
            var cx = img.Cols / 2;
            var cy = img.Rows / 2;
            var q0 = new Mat(img, new Rect(0, 0, cx, cy));
            var q1 = new Mat(img, new Rect(cx, 0, cx, cy));
            var q2 = new Mat(img, new Rect(0, cy, cx, cy));
            var q3 = new Mat(img, new Rect(cx, cy, cx, cy));

            var tmp = new Mat();
            q0.CopyTo(tmp);
            q3.CopyTo(q0);
            tmp.CopyTo(q3);

            q1.CopyTo(tmp);
            q2.CopyTo(q1);
            tmp.CopyTo(q2);
            return img;
        }
        
        public static Mat RealDft(Mat img)
        {
            img = ToGray(img);
            var padded = new Mat();
            var res = new Mat();
            var rows = Cv2.GetOptimalDFTSize(img.Rows);
            var columns = Cv2.GetOptimalDFTSize(img.Cols);
            Cv2.CopyMakeBorder(img, padded, 0, rows - img.Rows, 0, columns - img.Cols, BorderTypes.Constant, Scalar.All(0));
            padded.ConvertTo(padded, MatType.CV_32F);
            var planes = new Mat[] { padded, Mat.Zeros(padded.Size(), MatType.CV_32F) };
            var complexI = new Mat();
            Cv2.Merge(planes, complexI);
            Cv2.Dft(complexI, complexI);
            Cv2.Split(complexI, out planes);
            Cv2.Magnitude(planes[0], planes[1], res);
            var magI = res;
            magI += Scalar.All(1);
            Cv2.Log(magI, magI);
            magI = DftShift(magI);
            
            Cv2.Normalize(magI, magI, 0, 255, NormTypes.MinMax);
            magI.ConvertTo(magI, MatType.CV_8U);
            return magI;
        }
        
        public static Mat[] LogTransform(Mat img)
        {
            var gray = ToGray(img);
            var transformed = new Mat();
            gray.ConvertTo(transformed, MatType.CV_64F);
            var c = 255 / Math.Log(1 + 255);
            Cv2.Add(transformed, 1.0, transformed);
            Cv2.Log(transformed, transformed);
            Cv2.Multiply(transformed, c, transformed);
            return new[] { gray, transformed };
        }
        
        // 1
        // 20 241 0 255
        public static Mat[] LinearTransform(Mat img, int a, int b, int c ,int d)
        {
            var gray = ToGray(img);
            var transformed = new Mat();
            gray.ConvertTo(transformed, MatType.CV_64F);
            var c1 = (d - c) / (b - a);
            var c2 = (b * c - a * d) / (b - a);
            Cv2.Multiply(transformed, c1, transformed);
            Cv2.Add(transformed, c2, transformed);
            return new[] { gray, transformed };
        }
        
        public static Mat[] HistNormalize(Mat img, Mat target)
        {
            img = ToGray(img);
            var img1 = img.Clone();

            var mHist1 = new List<int>();
            var mNum1 = new List<double>();
            var inhist1 = new List<int>();

            var mHist2 = new List<int>();
            var mNum2 = new List<double>();
            var inhist2 = new List<int>();
            
            for (var x = 0 ; x < 256; x++)
            {
                mHist1.Add(0);
                mHist2.Add(0);
            }
            
            for (var i = 0; i < img.Rows; i++)
            {
                for (var j = 0; j < img.Cols; j++)
                {
                    mHist1[img.At<byte>(i, j)] += 1;
                }
            }

            mNum1.Add(mHist1[0] / (double)img.Total());
            for (var i = 0; i < 255; i++)
            {
                mNum1.Add(mNum1[i] + mHist1[i + 1] / (double)img.Total());
            }
            for (var i = 0; i < 256; i++)
            {
                inhist1.Add((int)Math.Round(255 * mNum1[i]));
            }

            for (var i = 0; i < target.Rows; i++)
            {
                for (var j = 0; j < target.Cols; j++)
                {
                    mHist2[target.At<byte>(i, j)] += 1;
                }
            }

            mNum2.Add((double)mHist2[0] / target.Total());
            for (var i = 0; i < 255; i++)
            {
                mNum2.Add(mNum2[i] + mHist2[i + 1] / (double)target.Total());
            }
            for (var i = 0; i < 256; i++)
            {
                inhist2.Add((int)Math.Round(255 * mNum2[i]));
            }
            
            var g = new List<int>();
            for (var i = 0; i < 256; i++)
            {
                var a = inhist1[i];
                var flag = true;
                for (var j = 0; j < 256; j++)
                {
                    if (inhist2[j] != a) continue;
                    g.Add(j);
                    flag = false;
                    break;
                }

                if (!flag) continue;
                var minp = 255;
                var jmin = 0;
                for (var j = 0; j < 256; j++)
                {
                    var b = Math.Abs(inhist2[j] - a);
                    if (b >= minp) continue;
                    minp = b;
                    jmin = j;
                }
                g.Add(jmin);
            }
            
            img.ConvertTo(img, MatType.CV_8U);
            for (var i = 0; i < img.Rows; i++)
            {
                for (var j = 0; j < img.Cols; j++)
                {
                    img.Set(i, j,(byte)g[img.At<byte>(i, j)]);
                }
            }
            return new[] { img1, img };
        }
        
        public static Mat[] EqualizeHist(Mat img)
        {
            var gray = ToGray(img);
            var res = new Mat();
            Cv2.EqualizeHist(gray, res);
            return new[]{gray, res};
        }
        
        public static Mat[] Threshold(Mat img, int threshold, ThresholdTypes type)
        {
            var gray = ToGray(img);
            var res = new Mat();
            Cv2.Threshold(gray, res, threshold, 255, type);
            return new[]{gray, res};
        }
        
        public static Mat HoughLines(Mat img)
        {
            var blurredImg = new Mat();
            Cv2.GaussianBlur(img, blurredImg, new Size(3, 3), 0);
            var edges = new Mat();
            Cv2.Canny(blurredImg, edges, 50, 150, apertureSize: 3);
            var lines = Cv2.HoughLines(edges, 1, Math.PI / 2, 118);
            var result = img.Clone();
            foreach (var line in lines)
            {
                var rho = line.Rho;
                var theta = line.Theta;
                if ((theta < (Math.PI / 4.0)) || (theta > (3.0 * Math.PI / 4.0)))
                {
                    var pt1 = new Point((int)(rho / Math.Cos(theta)), 0);
                    var pt2 = new Point((int)((rho - result.Rows * Math.Sin(theta)) / Math.Cos(theta)), result.Rows);
                    Cv2.Line(result, pt1, pt2, new Scalar(0, 0, 255));
                }
                else
                {
                    var pt1 = new Point(0, (int)(rho / Math.Sin(theta)));
                    var pt2 = new Point(result.Cols, (int)((rho - result.Cols * Math.Cos(theta)) / Math.Sin(theta)));
                    Cv2.Line(result, pt1, pt2, new Scalar(0, 0, 255));
                }
            }
            return result;
        }
        
        public static Mat HoughLinesP(Mat img)
        {
            var blurredImg = new Mat();
            Cv2.GaussianBlur(img, blurredImg, new Size(3, 3), 0);
            var edges = new Mat();
            Cv2.Canny(blurredImg, edges, 50, 150, apertureSize: 3);
            var minLineLength = 200;
            var maxLineGap = 15;
            var linesP = Cv2.HoughLinesP(edges, 1, Math.PI / 180, 80, minLineLength, maxLineGap);
            var res = img.Clone();
            foreach (var line in linesP)
            {
                Cv2.Line(res, line.P1, line.P2, new Scalar(0, 255, 0), 3);
            }
            return img;
        }

        public static Mat EdgeDetectRoberts(Mat img)
        {
            img = ToGray(img);
            var kernelx = new Mat(2, 2, MatType.CV_16S);
            kernelx.Set(0, 0, -1);
            kernelx.Set(0, 1, 0);
            kernelx.Set(1, 0, 0);
            kernelx.Set(1, 1, 1);

            var kernely = new Mat(2, 2, MatType.CV_16S);
            kernely.Set(0, 0, 0);
            kernely.Set(0, 1, -1);
            kernely.Set(1, 0, 1);
            kernely.Set(1, 1, 0);
            
            var x = new Mat();
            var y = new Mat();
            Cv2.Filter2D(img, x, MatType.CV_16S, kernelx);
            Cv2.Filter2D(img, y, MatType.CV_16S, kernely);

            var absX = new Mat();
            var absY = new Mat();
            Cv2.ConvertScaleAbs(x, absX);
            Cv2.ConvertScaleAbs(y, absY);

            var roberts = new Mat();
            Cv2.AddWeighted(absX, 0.5, absY, 0.5, 0, roberts);
            return roberts;
        }
        
        public static Mat EdgeDetectSobel(Mat img)
        {
            img = ToGray(img);
            var x = new Mat();
            var y = new Mat();
            Cv2.Sobel(img, x, MatType.CV_16S, 1, 0);
            Cv2.Sobel(img, y, MatType.CV_16S, 0, 1);

            var absX = new Mat();
            var absY = new Mat();
            Cv2.ConvertScaleAbs(x, absX);
            Cv2.ConvertScaleAbs(y, absY);

            var sobel = new Mat();
            Cv2.AddWeighted(absX, 0.5, absY, 0.5, 0, sobel);
            return sobel;
        }
        
        public static Mat EdgeDetectLaplacian(Mat img)
        {
            img = ToGray(img);
            var laplacian = new Mat();
            Cv2.Laplacian(img, laplacian, MatType.CV_16S);
            var absLaplacian = new Mat();
            Cv2.ConvertScaleAbs(laplacian, absLaplacian);
            return absLaplacian;
        }
        
        public static Mat EdgeDetectLoG(Mat img)
        {
            img = ToGray(img);
            var image = new Mat();
            Cv2.CopyMakeBorder(img, image, 2, 2, 2, 2, BorderTypes.Reflect);
            Cv2.GaussianBlur(image, image, new Size(3, 3), 0, 0);
            float[,] m1Data = new float[,]
            {
                { 0, 0, -1, 0, 0 },
                { 0, -1, -2, -1, 0 },
                { -1, -2, 16, -2, -1 },
                { 0, -1, -2, -1, 0 },
                { 0, 0, -1, 0, 0 }
            };
            var m1 = new Mat(5, 5, MatType.CV_32FC1);
            for (int i = 0; i < m1.Rows; i++)
            {
                for (int j = 0; j < m1.Cols; j++)
                {
                    m1.Set<float>(i, j, m1Data[i, j]);
                }
            }
            var image1 = new Mat();
            Cv2.Filter2D(image, image1, MatType.CV_8U, m1);
            Cv2.ConvertScaleAbs(image1, image1);
            return image1;
        }
        
        public static Mat EdgeDetectCanny(Mat img)
        {
            img = ToGray(img);
            var canny = new Mat();
            Cv2.Canny(img, canny, 50, 150);
            return canny;
        }
        
        public static Mat MeanFilter(Mat img)
        {
            img = ToGray(img);
            var mean = new Mat();
            Cv2.Blur(img, mean, new Size(5, 5));
            return mean;
        }
        
        private static Mat ApplyFilter(Mat complexImg, Mat mask)
        {
            var planes = new Mat[2];
            Cv2.Split(complexImg, out planes);
            planes[0] = planes[0].Mul(mask);
            planes[1] = planes[1].Mul(mask);

            var filtered = new Mat();
            Cv2.Merge(planes, filtered);
            return DftShift(filtered);
        }
        
        private static Mat InverseDFT(Mat complexImg)
        {
            Mat inverseTransform = new Mat();
            Cv2.Dft(complexImg, inverseTransform, DftFlags.Inverse | DftFlags.RealOutput | DftFlags.Scale);
            inverseTransform.ConvertTo(inverseTransform, MatType.CV_8U);
            return inverseTransform;
        }
        
        public static Mat IdealLowPassFilter(Mat img, double cutoff)
        {
            img = ToGray(img);
            var rows = img.Rows;
            var cols = img.Cols;
            var mask = new Mat(rows, cols, MatType.CV_32F, Scalar.All(0));
            var center = new Point(cols / 2, rows / 2);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    double distance = Math.Sqrt(Math.Pow(i - center.Y, 2) + Math.Pow(j - center.X, 2));
                    if (distance <= cutoff)
                    {
                        mask.Set<float>(i, j, 1);
                    }
                }
            }

            img.ConvertTo(img, MatType.CV_32F);
            var complexI = new Mat();
            Cv2.Dft(img, complexI, DftFlags.ComplexOutput);

            complexI = DftShift(complexI);
            complexI = ApplyFilter(complexI, mask);
            return InverseDFT(complexI);
        }
        
        public static Mat ButterworthLowPassFilter(Mat img, double cutoff, int order)
        {
            img = ToGray(img);
            var rows = img.Rows;
            var cols = img.Cols;
            var mask = new Mat(rows, cols, MatType.CV_32F);
            var center = new Point(cols / 2, rows / 2);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    double distance = Math.Sqrt(Math.Pow(i - center.Y, 2) + Math.Pow(j - center.X, 2));
                    double value = 1.0 / (1.0 + Math.Pow(distance / cutoff, 2 * order));
                    mask.Set<float>(i, j, (float)value);
                }
            }
            
            img.ConvertTo(img, MatType.CV_32F);
            var complexImg = new Mat();
            Cv2.Dft(img, complexImg, DftFlags.ComplexOutput);
            complexImg = DftShift(complexImg);
            var complexFiltered = ApplyFilter(complexImg, mask);
            return InverseDFT(complexFiltered);
        }
        
        public static Mat GaussianLowPassFilter(Mat img, double cutoff)
        {
            img = ToGray(img);
            var rows = img.Rows;
            var cols = img.Cols;
            var mask = new Mat(rows, cols, MatType.CV_32F);
            var center = new Point(cols / 2, rows / 2);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    double distance = Math.Sqrt(Math.Pow(i - center.Y, 2) + Math.Pow(j - center.X, 2));
                    double value = Math.Exp(-(Math.Pow(distance, 2)) / (2 * Math.Pow(cutoff, 2)));
                    mask.Set<float>(i, j, (float)value);
                }
            }
            
            img.ConvertTo(img, MatType.CV_32F);
            var complexImg = new Mat();
            Cv2.Dft(img, complexImg, DftFlags.ComplexOutput);
            complexImg = DftShift(complexImg);
            var complexFiltered = ApplyFilter(complexImg, mask);
            return InverseDFT(complexFiltered);
        }
        
        public static Mat SharpenRobert(Mat image)
        {
            image = ToGray(image);
            var h = image.Rows;
            var w = image.Cols;
            var imageNew = new Mat(image.Size(), image.Type(), Scalar.All(0));

            for (var i = 1; i < h - 1; i++)
            {
                for (var j = 1; j < w - 1; j++)
                {
                    var pixel1 = Math.Abs(image.At<byte>(i, j) - image.At<byte>(i + 1, j + 1));
                    var pixel2 = Math.Abs(image.At<byte>(i + 1, j) - image.At<byte>(i, j + 1));
                    imageNew.Set(i, j, (byte)(pixel1 + pixel2));
                }
            }

            return imageNew;
        }
        public static Mat SharpenSobel(Mat image)
        {
            image = ToGray(image);

            var h = image.Rows;
            var w = image.Cols;
            var imageNew = new Mat(image.Size(), MatType.CV_8UC1, Scalar.All(0)); 

            for (var i = 1; i < h - 1; i++)
            {
                for (var j = 1; j < w - 1; j++)
                {
                    var gx = Math.Abs(image.At<byte>(i - 1, j - 1) +
                                      2 * image.At<byte>(i, j - 1) +
                                      image.At<byte>(i + 1, j - 1) -
                                      image.At<byte>(i - 1, j + 1) -
                                      2 * image.At<byte>(i, j + 1) -
                                      image.At<byte>(i + 1, j + 1));

                    var gy = Math.Abs(image.At<byte>(i - 1, j - 1) +
                                      2 * image.At<byte>(i - 1, j) +
                                      image.At<byte>(i - 1, j + 1) -
                                      image.At<byte>(i + 1, j - 1) -
                                      2 * image.At<byte>(i + 1, j) -
                                      image.At<byte>(i + 1, j + 1));

                    imageNew.Set(i, j, (byte)(gx+gy));
                }
            }
            return imageNew;
        }

        public static Mat SharpenPrewitt(Mat image)
        {
            image = ToGray(image);
            
            var h = image.Rows;
            var w = image.Cols;
            var imageNew = new Mat(image.Size(), MatType.CV_8UC1, Scalar.All(0));

            for (var i = 1; i < h - 1; i++)
            {
                for (var j = 1; j < w - 1; j++)
                {
                    var gx = Math.Abs(image.At<byte>(i - 1, j - 1) +
                                      image.At<byte>(i, j - 1) +
                                      image.At<byte>(i + 1, j - 1) -
                                      image.At<byte>(i - 1, j + 1) -
                                      image.At<byte>(i, j + 1) -
                                      image.At<byte>(i + 1, j + 1));

                    var gy = Math.Abs(image.At<byte>(i - 1, j - 1) +
                                      image.At<byte>(i - 1, j) +
                                      image.At<byte>(i - 1, j + 1) -
                                      image.At<byte>(i + 1, j - 1) -
                                      image.At<byte>(i + 1, j) -
                                      image.At<byte>(i + 1, j + 1));

                    imageNew.Set(i, j, (byte)(gx + gy));
                }
            }

            return imageNew;
        }
        
        public static Mat SharpenLaplacian(Mat img)
        {
            img = ToGray(img);
            var h = img.Rows;
            var w = img.Cols;
            var imageNew = new Mat(img.Size(), img.Type(), Scalar.All(0));

            for (var i = 1; i < h - 1; i++)
            {
                for (var j = 1; j < w - 1; j++)
                {
                    var pixelValue = 4 * img.At<byte>(i, j)
                                     - img.At<byte>(i + 1, j)
                                     - img.At<byte>(i - 1, j)
                                     - img.At<byte>(i, j + 1)
                                     - img.At<byte>(i, j - 1);
                    imageNew.Set(i, j, (byte)pixelValue);
                }
            }

            return imageNew;
        }

        public enum FilterType { Ideal, Butterworth, Gaussian }

        public static Mat HighPassFilter(Mat img, FilterType filterType, double d0)
        {
            img = ToGray(img);
            var rows = img.Rows;
            var cols = img.Cols;

            var padded = new Mat();
            var m = Cv2.GetOptimalDFTSize(rows);
            var n = Cv2.GetOptimalDFTSize(cols);
            Cv2.CopyMakeBorder(img, padded, 0, m - rows, 0, n - cols, BorderTypes.Constant, Scalar.All(0));

            padded.ConvertTo(padded, MatType.CV_32F);
            Mat complexI = new Mat();
            Cv2.Dft(padded, complexI, DftFlags.ComplexOutput);
            complexI = DftShift(complexI);

            Mat filter = CreateFilter(complexI.Size(), filterType, d0);

            Mat complexFiltered = new Mat();
            Cv2.MulSpectrums(complexI, filter, complexFiltered, 0);
            complexFiltered = DftShift(complexFiltered);

            Mat inverseDFT = new Mat();
            Cv2.Dft(complexFiltered, inverseDFT, DftFlags.Inverse | DftFlags.RealOutput);
            Mat cropped = new Mat(inverseDFT, new Rect(0, 0, img.Cols, img.Rows));

            Mat magnitudeImage = new Mat();
            Cv2.Normalize(cropped, magnitudeImage, 0, 255, NormTypes.MinMax);
            magnitudeImage.ConvertTo(magnitudeImage, MatType.CV_8U);

            return magnitudeImage;
        }

        private static Mat CreateFilter(Size size, FilterType filterType, double d0)
        {
            Mat filter = new Mat(size, MatType.CV_32F, Scalar.All(0));
            int cx = size.Width / 2;
            int cy = size.Height / 2;

            for (int i = 0; i < size.Height; i++)
            {
                for (int j = 0; j < size.Width; j++)
                {
                    double distance = Math.Sqrt((i - cy) * (i - cy) + (j - cx) * (j - cx));
                    double value = 0;

                    switch (filterType)
                    {
                        case FilterType.Ideal:
                            value = (distance > d0) ? 1 : 0;
                            break;
                        case FilterType.Butterworth:
                            int n = 2;
                            value = 1.0 / (1.0 + Math.Pow(d0 / distance, 2 * n));
                            break;
                        case FilterType.Gaussian:
                            value = 1 - Math.Exp(-(distance * distance) / (2 * d0 * d0));
                            break;
                    }

                    filter.Set<float>(i, j, (float)value);
                }
            }

            Mat[] toMerge = new Mat[] { filter, filter };
            Mat complexFilter = new Mat();
            Cv2.Merge(toMerge, complexFilter); 

            return complexFilter;
        }
        
        public static Mat Erode(Mat img, int kernelSize)
        {
            img = ToGray(img);
            var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(kernelSize, kernelSize));
            var eroded = new Mat();
            Cv2.Erode(img, eroded, kernel);
            return eroded;
        }
        
        public static Mat Dilate(Mat img, int kernelSize)
        {
            img = ToGray(img);
            var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(kernelSize, kernelSize));
            var dilated = new Mat();
            Cv2.Dilate(img, dilated, kernel);
            return dilated;
        }
        
        public static Mat Open(Mat img, int kernelSize)
        {
            img = ToGray(img);
            var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(kernelSize, kernelSize));
            var opened = new Mat();
            Cv2.MorphologyEx(img, opened, MorphTypes.Open, kernel);
            return opened;
        }
        
        public static Mat Close(Mat img, int kernelSize)
        {
            img = ToGray(img);
            var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(kernelSize, kernelSize));
            var closed = new Mat();
            Cv2.MorphologyEx(img, closed, MorphTypes.Close, kernel);
            return closed;
        }
        
        public static Mat SaltPepperNoise(Mat img, double prob)
        {
            var rows = img.Rows;
            var cols = img.Cols;
            var output = new Mat(rows, cols, img.Type());
            var random = new Random();
            var thres = 1 - prob;
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < cols; j++)
                {
                    var rdn = random.NextDouble();
                    if (rdn < prob)
                    {
                        output.Set(i, j, 0);
                    }
                    else if (rdn > thres)
                    {
                        output.Set(i, j, 255);
                    }
                    else
                    {
                        output.Set(i, j, img.At<Vec3b>(i, j));
                    }
                }
            }
            return output;
        }
        
        public static Mat[] MeanRecover(Mat image, int type = 0)
        {
            switch (type)
            {
                case 0:
                    return MeanRecoverArithmetic(image);
                case 1:
                    return MeanRecoverGeometric(image);
                default:
                    return null;
            }
        }
        
        public static Mat[] MeanRecoverArithmetic(Mat image)
        {
            image = ToGray(image);
            var output = new Mat(image.Size(), MatType.CV_8UC1);

            for (var i = 0; i < image.Rows; i++)
            {
                for (var j = 0; j < image.Cols; j++)
                {
                    var sum = 0;
                    var count = 0;

                    for (var m = -1; m <= 1; m++)
                    {
                        for (var n = -1; n <= 1; n++)
                        {
                            var y = i + m;
                            var x = j + n;

                            if (y >= 0 && y < image.Rows && x >= 0 && x < image.Cols)
                            {
                                sum += image.At<byte>(y, x);
                                count++;
                            }
                        }
                    }

                    output.Set(i, j, (byte)(sum / count));
                }
            }

            return new[] { image, output };
        }
        
        public static Mat[] MeanRecoverGeometric(Mat image)
        {
            image = ToGray(image);
            var output = new Mat(image.Size(), MatType.CV_8UC1);

            for (var i = 0; i < image.Rows; i++)
            {
                for (var j = 0; j < image.Cols; j++)
                {
                    var ji = 1.0;

                    for (var m = -1; m <= 1; m++)
                    {
                        if (j + m >= 0 && j + m < image.Cols)
                        {
                            ji *= image.At<byte>(i, j + m);
                        }
                    }
                    output.Set(i, j, (byte)Math.Pow(ji, 1.0 / 3));
                }
            }


            return new[] { image, output };
        }

        public static Mat MeanRecoverRGB(Mat image)
        {
            var output = new Mat(image.Size(), image.Type());

            var channels = image.Channels();
            for (var i = 0; i < image.Rows; i++)
            {
                for (var j = 0; j < image.Cols; j++)
                {
                    var pixelSum = new float[channels];

                    for (var m = -1; m <= 1; m++)
                    {
                        for (var n = -1; n <= 1; n++)
                        {
                            var x = Math.Min(Math.Max(i + m, 0), image.Rows - 1);
                            var y = Math.Min(Math.Max(j + n, 0), image.Cols - 1);

                            var pixel = image.At<Vec3b>(x, y);
                            for (var c = 0; c < channels; c++)
                            {
                                pixelSum[c] += pixel[c];
                            }
                        }
                    }
                    var newPixel = new Vec3b();
                    for (var c = 0; c < channels; c++)
                    {
                        newPixel[c] = (byte)(pixelSum[c] / 9);
                    }
                    output.Set<Vec3b>(i, j, newPixel);
                }
            }

            return output;
        }

        private static byte GetMiddle(List<byte> array)
        {
            array.Sort();
            return array[array.Count / 2];
        }
                
        public static Mat MedianRecover(Mat image)
        {
            var output = new Mat(image.Size(), image.Type());
            var channels = image.Channels();
            for (var i = 0; i < image.Rows; i++)
            {
                for (var j = 0; j < image.Cols; j++)
                {
                    var newPixel = new Vec3b();
                    for (var c = 0; c < channels; c++)
                    {
                        var array = new List<byte>();
                        for (var m = -1; m <= 1; m++)
                        {
                            for (var n = -1; n <= 1; n++)
                            {
                                var x = Math.Min(Math.Max(i + m, 0), image.Rows - 1);
                                var y = Math.Min(Math.Max(j + n, 0), image.Cols - 1);
                                array.Add(image.At<Vec3b>(x, y)[c]);
                            }
                        }
                        newPixel[c] = GetMiddle(array);
                    }
                    output.Set<Vec3b>(i, j, newPixel);
                }
            }
            return output;
        }
        
        public static Mat SelectiveRecover(Mat image)
        {
            var output = new Mat(image.Size(), MatType.CV_8UC1);
            var min = 20;
            var max = 220;
            for (var i = 0; i < image.Rows; i++)
            {
                for (var j = 0; j < image.Cols; j++)
                {
                    var pixelValue = image.At<byte>(i, j);
                    if (pixelValue > min && pixelValue < max)
                    {
                        output.Set(i, j, pixelValue);
                    }
                    else
                    {
                        output.Set(i, j, (byte)255);
                    }
                }
            }
            return output;
        }

        public static Mat SharpenColoredImageLaplacian(Mat img)
        {
            float[,] kernelData = new float[,]
            {
                { -1, -1, -1 },
                { -1,  9, -1 },
                { -1, -1, -1 }
            };
            Mat kernelMat = new Mat(3, 3, MatType.CV_32F);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    kernelMat.Set<float>(i, j, kernelData[i, j]);
                }
            }
            Mat dst = new Mat();
            Cv2.Filter2D(img, dst, img.Depth(), kernelMat);

            return dst;
        }

        public static Mat SharpenColoredImageSobel(Mat src, double origin, double sharpen)
        {
            Mat gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            Mat gradX = new Mat();
            Mat gradY = new Mat();
            Cv2.Sobel(gray, gradX, MatType.CV_16S, 1, 0);
            Cv2.Sobel(gray, gradY, MatType.CV_16S, 0, 1);
            Mat absGradX = new Mat();
            Mat absGradY = new Mat();
            Cv2.ConvertScaleAbs(gradX, absGradX);
            Cv2.ConvertScaleAbs(gradY, absGradY);
            Mat grad = new Mat();
            Cv2.AddWeighted(absGradX, 0.5, absGradY, 0.5, 0, grad);
            Mat gradColor = new Mat();
            Cv2.CvtColor(grad, gradColor, ColorConversionCodes.GRAY2BGR);
            Mat sharpened = new Mat();
            Cv2.AddWeighted(src, origin, gradColor, -sharpen, 0, sharpened);
            return sharpened;
        }

        public static Mat GetSRImage(string imgPath)
        {
            return null;
        }
        
    }
}
