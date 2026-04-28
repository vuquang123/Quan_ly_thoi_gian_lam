using System;
using OpenCvSharp;
using FaceIDHRM.Core.Interfaces;

namespace FaceIDHRM.Core.Implementations
{
    public class FaceRecognitionDotNetRecognizer : IFaceRecognizer
    {
        public double[] GetEncoding(Mat faceImage)
        {
            using Mat gray = new Mat();
            Cv2.CvtColor(faceImage, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(gray, gray);
            using Mat resized = new Mat();
            Cv2.Resize(gray, resized, new OpenCvSharp.Size(100, 100));
            
            double[] encoding = new double[10000];
            int idx = 0;
            for (int r = 0; r < 100; r++)
            {
                for (int c = 0; c < 100; c++)
                {
                    encoding[idx++] = resized.At<byte>(r, c);
                }
            }
            return encoding;
        }

        public bool Compare(double[] knownEncoding, double[] encodingToCheck, double tolerance = 0.50)
        {
            if (knownEncoding == null || encodingToCheck == null || knownEncoding.Length != 10000 || encodingToCheck.Length != 10000)
                return false;

            using Mat knownMat = new Mat(100, 100, MatType.CV_8UC1);
            using Mat checkMat = new Mat(100, 100, MatType.CV_8UC1);

            int idx = 0;
            for (int r = 0; r < 100; r++)
            {
                for (int c = 0; c < 100; c++)
                {
                    knownMat.Set<byte>(r, c, (byte)knownEncoding[idx]);
                    checkMat.Set<byte>(r, c, (byte)encodingToCheck[idx]);
                    idx++;
                }
            }

            // Implement sliding window by cropping the known template slightly
            int cropW = 80;
            int cropH = 80;
            int cropX = 10;
            int cropY = 10;
            Rect cropRect = new Rect(cropX, cropY, cropW, cropH);
            
            using Mat template = new Mat(knownMat, cropRect);

            using Mat result = new Mat();
            Cv2.MatchTemplate(checkMat, template, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out _);

            Console.WriteLine($"[DEBUG Recognizer] Correlation: {maxVal:F3}");

            return maxVal >= tolerance;
        }
    }
}
