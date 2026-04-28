using OpenCvSharp;
using FaceIDHRM.Core.Interfaces;

namespace FaceIDHRM.Core.Implementations
{
    public class OpenCvCamera : ICamera
    {
        private VideoCapture _capture;

        public OpenCvCamera()
        {
            _capture = new VideoCapture(0);
            _capture.Open(0);
        }

        public bool IsOpened => _capture != null && _capture.IsOpened();

        public Mat ReadFrame()
        {
            if (IsOpened)
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

        public void Release()
        {
            if (IsOpened)
            {
                _capture.Release();
                _capture.Dispose();
            }
        }
    }
}
