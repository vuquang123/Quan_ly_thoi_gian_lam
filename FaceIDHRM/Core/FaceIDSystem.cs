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
                    if (_recognizer.Compare(user.FaceEncoding, currentEncoding, 0.45))
                    {
                        return user.MaNV;
                    }
                }
            }
            return null;
        }
    }
}
