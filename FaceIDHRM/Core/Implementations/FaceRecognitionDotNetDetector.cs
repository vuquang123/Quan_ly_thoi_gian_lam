using System;
using System.Collections.Generic;
using System.IO;
using OpenCvSharp;
using FaceIDHRM.Core.Interfaces;

namespace FaceIDHRM.Core.Implementations
{
    public class FaceRecognitionDotNetDetector : IFaceDetector
    {
        private CascadeClassifier _frontalDetector;
        private CascadeClassifier _profileDetector;

        public FaceRecognitionDotNetDetector()
        {
            string frontalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "haarcascade_frontalface_default.xml");
            string profilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "haarcascade_profileface.xml");
            
            if (File.Exists(frontalPath))
                _frontalDetector = new CascadeClassifier(frontalPath);
            if (File.Exists(profilePath))
                _profileDetector = new CascadeClassifier(profilePath);
        }

        public IEnumerable<Rect> DetectFaces(Mat frame)
        {
            if (frame == null || frame.Empty()) return new Rect[0];

            using Mat gray = new Mat();
            Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

            // 1. Thử Frontal Face (Mặt thẳng)
            if (_frontalDetector != null && !_frontalDetector.Empty())
            {
                var frontalFaces = _frontalDetector.DetectMultiScale(gray, 1.1, 3);
                if (frontalFaces.Length > 0) return frontalFaces;
            }

            // 2. Fallback Thử Profile Face (Mặt nghiêng)
            if (_profileDetector != null && !_profileDetector.Empty())
            {
                var profileFaces = _profileDetector.DetectMultiScale(gray, 1.1, 3);
                if (profileFaces.Length > 0) return profileFaces;
            }

            return new Rect[0];
        }
    }
}
