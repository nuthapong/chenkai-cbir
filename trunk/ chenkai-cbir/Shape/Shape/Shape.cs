using System;
using System.Collections.Generic;
using System.Media;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

/*
 * How to use:
 * Put the image1.bmp and image2 in the same folder as Shape.exe
 * it returns the distance between two pics
 */

namespace CBIR
{
    class Shape
    {
        public static void PrintHuMoments(MCvHuMoments huMoment)
        {
            double[] m = new double[7];
            m[0] = huMoment.hu1;
            m[1] = huMoment.hu2;
            m[2] = huMoment.hu3;
            m[3] = huMoment.hu4;
            m[4] = huMoment.hu5;
            m[5] = huMoment.hu6;
            m[6] = huMoment.hu7;

            for (int i = 0; i < 7; i++)
            {
                int sign;
                double result = Math.Abs(m[i]);

                if (m[i] > 0)
                    sign = 1;
                else if (m[i] < 0)
                    sign = -1;
                else
                    sign = 0;

                result = sign * Math.Log10(result);

                Console.WriteLine("hu{0}: {1}", i + 1, result);
            }
        }

        // get the Hu
        public static bool IsGetShapeFeature(string ImgPath, ref MCvHuMoments huMoments)
        {
            IntPtr img = IntPtr.Zero;
            MCvMoments moments = new MCvMoments();
            if ((img = CvInvoke.cvLoadImage(ImgPath, LOAD_IMAGE_TYPE.CV_LOAD_IMAGE_GRAYSCALE)) != IntPtr.Zero)
            {
                CvInvoke.cvMoments(img, ref moments, 0);
                CvInvoke.cvGetHuMoments(ref moments, ref huMoments);
                CvInvoke.cvReleaseImage(ref img);
                return true;
            }
            else
                return false;
        }

        //return the distances between two pics. dist >= 0: func works right, else fail
        public static double MatchShapes(string ImgPath1, string ImgPath2)
        {
            const double eps = 0.000000000000000001;
            MCvHuMoments huMoment1 = new MCvHuMoments();
            MCvHuMoments huMoment2 = new MCvHuMoments();
            double[] ma = new double[7];
            double[] mb = new double[7];

            IsGetShapeFeature(ImgPath1, ref huMoment1);
           /* Here is a bug.
            * I still cannot figure out why the first time calling IsGetShapeFeautre() it returns wrong anwser
            * 
            * Details:
            *   CvInvoke.cvMoments(img, ref moments, 0) cannot work right first time, then CVInvoke.cvGetHuMoments() returns wrong hu
            *   And I find when I use C++ to call this function, it works fine at the first, just for C#, it has this problem
            * Actual:
            *   moments.m00, moments.m01, moments.02 etc returns NaN
            *   huMoments.hu[i] returns NaN
            * Excepted:
            *   moments.m00 etc returns double value
            *   huMoments.hu[i] returns double value
            *   
            * So I call the IsGetShapeFeature() once to pass the wrong anwser, it's really an ugly way to fix it. I will back to fix it when I am free.
            *   
            *  2009/8/30
            */
            if (IsGetShapeFeature(ImgPath1, ref huMoment1) && IsGetShapeFeature(ImgPath2, ref huMoment2))
            {
                ma[0] = huMoment1.hu1;
                ma[1] = huMoment1.hu2;
                ma[2] = huMoment1.hu3;
                ma[3] = huMoment1.hu4;
                ma[4] = huMoment1.hu5;
                ma[5] = huMoment1.hu6;
                ma[6] = huMoment1.hu7;

                mb[0] = huMoment1.hu1;
                mb[1] = huMoment2.hu2;
                mb[2] = huMoment2.hu3;
                mb[3] = huMoment2.hu4;
                mb[4] = huMoment2.hu5;
                mb[5] = huMoment2.hu6;
                mb[6] = huMoment2.hu7;

                double result = 0;

                for (int i = 0; i < 7; i++)
                {
                    double ama = Math.Abs(ma[i]);
                    double amb = Math.Abs(mb[i]);

                    int sma, smb;
                    if (ma[i] > 0)
                        sma = 1;
                    else if (ma[i] < 0)
                        sma = -1;
                    else
                        sma = 0;
                    if (mb[i] > 0)
                        smb = 1;
                    else if (mb[i] < 0)
                        smb = -1;
                    else
                        smb = 0;

                    if (ama > eps && amb > eps)
                    {
                        ama = sma * Math.Log10(ama);
                        amb = smb * Math.Log10(amb);
                        result += (amb - ama) * (amb - ama);
                    }
                }

                result = Math.Sqrt(result);

                return result;
            }
            else
                return -1;
        }

        static void Main(string[] args)
        {
            string img1 = "image1.bmp";
            string img2 = "image2.bmp";

            double dist;

            MCvHuMoments huMoments = new MCvHuMoments();
            /*
            IsGetShapeFeature(img1, ref huMoments);
            PrintHuMoments(huMoments);

            IsGetShapeFeature(img2, ref huMoments);
            PrintHuMoments(huMoments);

            IsGetShapeFeature(img1, ref huMoments);
            PrintHuMoments(huMoments);
            */
            //Console.ReadLine();
            
            if ((dist = MatchShapes(img1, img2)) >= 0)
            {
                Console.WriteLine("The distance between {0} and {1} is {2}", img1, img2, dist);
            }
            else
                Console.WriteLine("Fail to compute the distance");
            Console.ReadLine(); 
        }
    }
}
