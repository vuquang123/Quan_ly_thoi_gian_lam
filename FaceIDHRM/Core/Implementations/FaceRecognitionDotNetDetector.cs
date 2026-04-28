using System;
using System.Collections.Generic;
using System.IO;
using OpenCvSharp;
using FaceIDHRM.Core.Interfaces;

namespace FaceIDHRM.Core.Implementations
{
    public class FaceRecognitionDotNetDetector : IFaceDetector
    {
        private CascadeClassifier _faceDetector;

        public FaceRecognitionDotNetDetector()
        {
            string sourceCascadePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "haarcascade_frontalface_default.xml");
            string tempCascadePath = Path.Combine(Path.GetTempPath(), "haarcascade_frontalface_default.xml");
            
            if (File.Exists(sourceCascadePath))
            {
                File.Copy(sourceCascadePath, tempCascadePath, true);
                _faceDetector = new CascadeClassifier(tempCascadePath);
            }
        }

        public IEnumerable<Rect> DetectFaces(Mat frame)
        {
            if (_faceDetector != null && !frame.Empty())
            {
                Mat gray = new Mat();
                Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
                return _faceDetector.DetectMultiScale(gray, 1.1, 4);
            }
            return new Rect[0];
        }
    }
}
