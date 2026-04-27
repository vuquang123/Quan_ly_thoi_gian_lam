using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenCvSharp;

namespace FaceIDHRM.Managers
{
    public class FaceIDManager
    {
        private VideoCapture _capture;
        private CascadeClassifier _faceDetector;
        private CascadeClassifier _profileFaceDetector;
        
        private string _faceDataFolder;

        public FaceIDManager()
        {
            _faceDataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FaceData");
            if (!Directory.Exists(_faceDataFolder))
                Directory.CreateDirectory(_faceDataFolder);

            string sourceCascadePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "haarcascade_frontalface_default.xml");
            string tempCascadePath = Path.Combine(Path.GetTempPath(), "haarcascade_frontalface_default.xml");
            
            if (File.Exists(sourceCascadePath))
            {
                File.Copy(sourceCascadePath, tempCascadePath, true);
                _faceDetector = new CascadeClassifier(tempCascadePath);
            }

            string sourceProfilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "haarcascade_profileface.xml");
            if (!File.Exists(sourceProfilePath)) sourceProfilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\haarcascade_profileface.xml");
            string tempProfilePath = Path.Combine(Path.GetTempPath(), "haarcascade_profileface.xml");

            if (File.Exists(sourceProfilePath))
            {
                File.Copy(sourceProfilePath, tempProfilePath, true);
                _profileFaceDetector = new CascadeClassifier(tempProfilePath);
            }
        }

        public void BatCamera()
        {
            _capture = new VideoCapture(0);
            _capture.Open(0);
        }

        public void TatCamera()
        {
            if (_capture != null && _capture.IsOpened())
            {
                _capture.Release();
                _capture.Dispose();
            }
        }

        public Mat GetFrame()
        {
            if (_capture != null && _capture.IsOpened())
            {
                Mat frame = new Mat();
                _capture.Read(frame);
                if (!frame.Empty())
                {
                    Cv2.Flip(frame, frame, FlipMode.Y);
                }
                return frame;
            }
            return null;
        }

        public Rect[] PhatHienKhuonMat(Mat frame)
        {
            if (_faceDetector != null && !frame.Empty())
            {
                Mat gray = new Mat();
                Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
                return _faceDetector.DetectMultiScale(gray, 1.1, 4);
            }
            return new Rect[0];
        }

        public bool PhatHienGocNghieng(Mat frame)
        {
            if (_profileFaceDetector != null && !frame.Empty())
            {
                Mat gray = new Mat();
                Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
                
                // Haar Cascade góc nghiêng mặc định bắt 1 chiều diện mạo, lật ảnh lại để test chiều còn lại
                var profiles = _profileFaceDetector.DetectMultiScale(gray, 1.2, 5);
                if (profiles.Length > 0) return true;

                Mat flippedGray = new Mat();
                Cv2.Flip(gray, flippedGray, FlipMode.Y);
                profiles = _profileFaceDetector.DetectMultiScale(flippedGray, 1.2, 5);
                if (profiles.Length > 0) return true;
                
                return false;
            }
            return false;
        }

        public string Enrollment(string maNV, Mat faceImage)
        {
            string fileName = $"Face_{maNV}.jpg";
            string filePath = Path.Combine(_faceDataFolder, fileName);
            
            File.WriteAllBytes(filePath, faceImage.ImEncode(".jpg"));
            
            return fileName; 
        }

        public string Verification(Mat faceImage, List<Models.NhanVien> danhSachNhanVien)
        {
            double maxCorrelation = 0.0;
            string foundMaNV = null;
            
            Mat grayFace = new Mat();
            Cv2.CvtColor(faceImage, grayFace, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(grayFace, grayFace);

            foreach(var nv in danhSachNhanVien)
            {
                if (string.IsNullOrEmpty(nv.FaceDataPath)) continue;

                string actualPath = nv.FaceDataPath;
                if (!Path.IsPathRooted(actualPath)) 
                    actualPath = Path.Combine(_faceDataFolder, actualPath);

                if (!File.Exists(actualPath)) continue;

                Mat savedImage = Mat.FromImageData(File.ReadAllBytes(actualPath), ImreadModes.Grayscale);
                if (savedImage.Empty()) continue;

                Cv2.EqualizeHist(savedImage, savedImage);

                double[] scales = { 0.85, 1.0, 1.15 };
                double bestMatchForThisNv = 0.0;

                foreach (var scale in scales)
                {
                    using Mat scaledSaved = new Mat();
                    Cv2.Resize(savedImage, scaledSaved, new OpenCvSharp.Size((int)(grayFace.Width * scale), (int)(grayFace.Height * scale)));

                    using Mat target = new Mat();
                    grayFace.CopyTo(target);

                    // Cắt lấy 80% diện tích ở giữa làm template để nó có thể trượt (slide) bên trong target
                    // Điều này giúp thuật toán nhận diện được ngay cả khi mặt bị lệch tọa độ (translation) một chút
                    int cropW = (int)(scaledSaved.Width * 0.8);
                    int cropH = (int)(scaledSaved.Height * 0.8);
                    if (cropW <= 0 || cropH <= 0) continue;

                    int cropX = (scaledSaved.Width - cropW) / 2;
                    int cropY = (scaledSaved.Height - cropH) / 2;
                    Rect cropRect = new Rect(cropX, cropY, cropW, cropH);
                    
                    using Mat template = new Mat(scaledSaved, cropRect);

                    // Đảm bảo template phải nhỏ hơn hoặc bằng target
                    if (template.Width > target.Width || template.Height > target.Height) continue;

                    using Mat result = new Mat();
                    Cv2.MatchTemplate(target, template, result, TemplateMatchModes.CCoeffNormed);
                    Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out _);

                    if (maxVal > bestMatchForThisNv) bestMatchForThisNv = maxVal;
                }

                Console.WriteLine($"[DEBUG OpenCV] So sánh với {nv.MaNV} -> Sai số Multi-Scale: {bestMatchForThisNv:F3}");

                // Hạ threshold xuống 0.45 vì đã cắt template nhỏ lại, điểm CCoeffNormed sẽ giảm một chút nhưng chống nhiễu tốt hơn
                if (bestMatchForThisNv > maxCorrelation && bestMatchForThisNv > 0.45) 
                {
                    maxCorrelation = bestMatchForThisNv;
                    foundMaNV = nv.MaNV;
                }
            }
            return foundMaNV;
        }
    }
}
