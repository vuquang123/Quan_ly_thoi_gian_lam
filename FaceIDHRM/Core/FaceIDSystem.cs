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

        public FaceIDSystem(ICamera camera, IFaceDetector detector, IFaceRecognizer recognizer)
        {
            _camera = camera;
            _detector = detector;
            _recognizer = recognizer;
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

        public double[] GetEncoding(Mat faceImage)
        {
            return _recognizer.GetEncoding(faceImage);
        }

        public void EnrollUser(NhanVien user, Mat faceImage)
        {
            user.FaceEncoding = _recognizer.GetEncoding(faceImage);
        }

        public string Verification(Mat faceImage, List<NhanVien> users)
        {
            double[] currentEncoding = _recognizer.GetEncoding(faceImage);
            
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
