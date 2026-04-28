using OpenCvSharp;

namespace FaceIDHRM.Core.Interfaces
{
    public interface ICamera
    {
        Mat ReadFrame();
        void Release();
        bool IsOpened { get; }
    }
}
