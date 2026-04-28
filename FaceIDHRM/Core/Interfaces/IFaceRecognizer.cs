using OpenCvSharp;

namespace FaceIDHRM.Core.Interfaces
{
    public interface IFaceRecognizer
    {
        double[] GetEncoding(Mat faceImage);
        bool Compare(double[] knownEncoding, double[] encodingToCheck, double tolerance = 0.6);
    }
}
