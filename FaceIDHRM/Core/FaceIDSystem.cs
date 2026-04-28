using System;
using System.Collections.Generic;
using System.Linq;
using FaceIDHRM.Core.Interfaces;
using FaceIDHRM.Models;
using OpenCvSharp;

namespace FaceIDHRM.Core
{
    public class FaceIDSystem
    {
        private readonly ICamera _camera;
        private readonly IFaceDetector _detector;
        private readonly IFaceRecognizer _recognizer;
        private readonly CascadeClassifier _eyeCascade;

        public FaceIDSystem(ICamera camera, IFaceDetector detector, IFaceRecognizer recognizer)
        {
            _camera = camera;
            _detector = detector;
            _recognizer = recognizer;
            try
            {
                _eyeCascade = new CascadeClassifier("haarcascade_eye.xml");
            }
            catch
            {
                // Fallback if file missing
            }
        }

        public void BatCamera()
        {
            
        }

        public void TatCamera()
        {
            _camera.Release();
        }

        public Mat GetFrame()
        {
            return _camera.ReadFrame();
        }

        public Rect[] PhatHienKhuonMat(Mat frame)
        {
            return _detector.DetectFaces(frame).ToArray();
        }

        public Mat AlignFace(Mat faceImage)
        {
            if (faceImage == null || faceImage.Empty() || _eyeCascade == null || _eyeCascade.Empty())
                return faceImage;

            using Mat gray = new Mat();
            Cv2.CvtColor(faceImage, gray, ColorConversionCodes.BGR2GRAY);
            
            var eyes = _eyeCascade.DetectMultiScale(gray, 1.1, 3, HaarDetectionTypes.ScaleImage, new Size(20, 20));
            
            if (eyes.Length >= 2)
            {
                var sortedEyes = eyes.OrderBy(e => e.X).ToArray();
                Point leftEye = new Point(sortedEyes[0].X + sortedEyes[0].Width / 2, sortedEyes[0].Y + sortedEyes[0].Height / 2);
                Point rightEye = new Point(sortedEyes[1].X + sortedEyes[1].Width / 2, sortedEyes[1].Y + sortedEyes[1].Height / 2);
                
                double dY = rightEye.Y - leftEye.Y;
                double dX = rightEye.X - leftEye.X;
                double angle = Math.Atan2(dY, dX) * 180.0 / Math.PI;

                Point2f center = new Point2f((leftEye.X + rightEye.X) / 2f, (leftEye.Y + rightEye.Y) / 2f);

                using Mat rotMatrix = Cv2.GetRotationMatrix2D(center, angle, 1.0);
                Mat alignedFace = new Mat();
                Cv2.WarpAffine(faceImage, alignedFace, rotMatrix, faceImage.Size());
                return alignedFace;
            }
            
            return faceImage.Clone();
        }

        public bool CheckLiveness(Mat faceImage)
        {
            if (faceImage == null || faceImage.Empty()) return false;

            using Mat gray = new Mat();
            Cv2.CvtColor(faceImage, gray, ColorConversionCodes.BGR2GRAY);

            using Mat laplacian = new Mat();
            Cv2.Laplacian(gray, laplacian, MatType.CV_64F);
            Cv2.MeanStdDev(laplacian, out _, out Scalar stdDev);
            double variance = stdDev.Val0 * stdDev.Val0;

            if (variance < 40.0) return false; // Ngưỡng sắc nét chống ảnh mờ giả lập

            using Mat hist = new Mat();
            int[] channels = { 0 };
            int[] histSize = { 256 };
            Rangef[] ranges = { new Rangef(0, 256) };
            Cv2.CalcHist(new Mat[] { gray }, channels, null, hist, 1, histSize, ranges);

            double nonZeroBins = 0;
            for (int i = 0; i < 256; i++)
            {
                if (hist.At<float>(i) > gray.Total() * 0.005) 
                    nonZeroBins++;
            }

            if (nonZeroBins < 15) return false; // Chống dải tương phản bị bóp méo do màn hình

            return true;
        }

        public double[] GetEncoding(Mat faceImage)
        {
            using Mat aligned = AlignFace(faceImage);
            return _recognizer.GetEncoding(aligned);
        }

        public void EnrollUser(NhanVien user, Mat faceImage)
        {
            user.FaceEncoding = GetEncoding(faceImage);
        }

        public string Verification(Mat faceImage, List<NhanVien> users)
        {
            if (!CheckLiveness(faceImage))
            {
                return null; 
            }

            double[] currentEncoding = GetEncoding(faceImage);
            
            foreach (var user in users)
            {
                if (user.FaceEncoding != null && user.FaceEncoding.Length > 0)
                {
                    if (user.FaceEncoding.Length == 30000)
                    {
                        double[] pose1 = new double[10000];
                        double[] pose2 = new double[10000];
                        double[] pose3 = new double[10000];
                        Array.Copy(user.FaceEncoding, 0, pose1, 0, 10000);
                        Array.Copy(user.FaceEncoding, 10000, pose2, 0, 10000);
                        Array.Copy(user.FaceEncoding, 20000, pose3, 0, 10000);

                        if (_recognizer.Compare(pose1, currentEncoding, 0.70) ||
                            _recognizer.Compare(pose2, currentEncoding, 0.70) ||
                            _recognizer.Compare(pose3, currentEncoding, 0.70))
                        {
                            return user.MaNV;
                        }
                    }
                    else
                    {
                        if (_recognizer.Compare(user.FaceEncoding, currentEncoding, 0.70))
                        {
                            return user.MaNV;
                        }
                    }
                }
            }
            return null;
        }
    }
}
