using System.Collections.Generic;
using OpenCvSharp;

namespace FaceIDHRM.Core.Interfaces
{
    public interface IFaceDetector
    {
        IEnumerable<Rect> DetectFaces(Mat frame);
    }
}
