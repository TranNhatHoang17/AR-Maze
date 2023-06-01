using OpenCvSharp.Demo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using static UnityEngine.UIElements.VisualElement;

public class ContoursFinder : WebCamera
{
    [SerializeField] private FlipMode ImageFlip;
    [SerializeField] private float Threshold = 96.4f;
    [SerializeField] private bool ShowProcessingImage = true;
    [SerializeField] private float CurveAccuracy = 10.0f;
    [SerializeField] private float MinArea = 5000.0f;
    [SerializeField] private PolygonCollider2D PolygonCollider;


    [SerializeField] private CircleCollider2D circleCollider;
    [SerializeField] private PolygonCollider2D boxCollider;

    private Mat image;
    private Mat processImage = new Mat();
    private Point[][] contours;
    private HierarchyIndex[] hierachy;
    private Vector2[] vectorList;

    private Point[][] contoursd;
    private HierarchyIndex[] hierachyd;

    private Point[][] contoursx;
    private HierarchyIndex[] hierarchyx;

    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        image = OpenCvSharp.Unity.TextureToMat(input);
        Cv2.Flip(image, image, ImageFlip);
        Cv2.CvtColor(image, processImage, ColorConversionCodes.BGR2GRAY);
        Cv2.Threshold(processImage, processImage, Threshold, 255, ThresholdTypes.Binary);
        Cv2.InRange(processImage, new Scalar(0), new Scalar(50), processImage);
        Cv2.FindContours(processImage, out contours, out hierachy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple, null);


        PolygonCollider.pathCount = 0;
        foreach (Point[] contour in contours)
        {
            Point[] points = Cv2.ApproxPolyDP(contour, CurveAccuracy, true);
            var area = Cv2.ContourArea(contour);

            if (area > MinArea)
            {
                drawContour(processImage, new Scalar(127, 127, 127), 2, points);

                PolygonCollider.pathCount++;
                PolygonCollider.SetPath(PolygonCollider.pathCount - 1, toVector2(points));
            }
        }

        // Convert the ROI to HSV color space
        Mat hsvd = new Mat();
        Cv2.CvtColor(image, hsvd, ColorConversionCodes.BGR2HSV);

        // Threshold on red color range
        Mat lower_red = new Mat();
        Mat upper_red = new Mat();
        Cv2.InRange(hsvd, new Scalar(0, 100, 100), new Scalar(10, 255, 255), lower_red);
        Cv2.InRange(hsvd, new Scalar(160, 100, 100), new Scalar(179, 255, 255), upper_red);
        Mat maskd = lower_red + upper_red;

        // Apply morphology close and open
        Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
        Cv2.MorphologyEx(maskd, maskd, MorphTypes.Close, kernel);
        Cv2.MorphologyEx(maskd, maskd, MorphTypes.Open, kernel);

        // Apply Canny edge detection
        Mat edgesd = new Mat(image.Size(), MatType.CV_8UC4);
        Cv2.Canny(maskd, edgesd, 50, 150);
        boxCollider.pathCount = 0;
        // Find contours in the resulting binary image
        Cv2.FindContours(edgesd, out contoursd, out hierachyd, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple, null);
        foreach (Point[] contour in contoursd)
        {
            Point[] points = Cv2.ApproxPolyDP(contour, CurveAccuracy, true);
            var area = Cv2.ContourArea(contour);
            if (area > MinArea)
            {
                drawContour(processImage, new Scalar(127, 127, 127), 2, points);

                boxCollider.pathCount++;
                boxCollider.SetPath(boxCollider.pathCount - 1, toVector2(points));
            }
        }

        // Convert the ROI to HSV color space
        Mat hsvx = new Mat();
        Cv2.CvtColor(image, hsvx, ColorConversionCodes.BGR2HSV);
        // Threshold on green color range
        Mat lower_green = new Mat();
        Mat upper_green = new Mat();
        Cv2.InRange(hsvx, new Scalar(35, 100, 100), new Scalar(85, 255, 255), lower_green);
        Cv2.InRange(hsvx, new Scalar(95, 100, 100), new Scalar(145, 255, 255), upper_green);

        Mat maskx = lower_green + upper_green;

        // Apply morphology close and open
        Mat kernelx = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
        Cv2.MorphologyEx(maskx, maskx, MorphTypes.Close, kernelx);
        Cv2.MorphologyEx(maskx, maskx, MorphTypes.Open, kernelx);

        // Apply Canny edge detection
        Mat edgesx = new Mat(image.Size(), MatType.CV_8UC4);
        Cv2.Canny(maskx, edgesx, 50, 150);
        // Find contours in the resulting binary image
        Cv2.FindContours(edgesx, out contoursx, out hierarchyx, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple, null);
        foreach (Point[] contour in contoursx)
        {
            Point[] points = Cv2.ApproxPolyDP(contour, 1, true);
            var area = Cv2.ContourArea(contour);
            if (area > MinArea)
            {
                drawContour(processImage, new Scalar(127, 127, 127), 2, points);

                // Calculate center and radius of the circle
                Point center = CalculateCenter(points);
                float radius = CalculateRadius(center, points);

                // Set circle collider properties
                circleCollider.offset = new Vector2(center.X, center.Y);
                circleCollider.radius = radius;
            }
        }

        if (output == null)

            output = OpenCvSharp.Unity.MatToTexture(ShowProcessingImage ? processImage : image);
        else
            OpenCvSharp.Unity.MatToTexture(ShowProcessingImage ? processImage : image, output);
        return true;

    }

    private Vector2[] toVector2(Point[] points)
    {
        vectorList = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            vectorList[i] = new Vector2(points[i].X, points[i].Y);
        }
        return vectorList;
    }
    private void drawContour(Mat Image, Scalar Color, int Thickness, Point[] Points)
    {
        for (int i = 1; i < Points.Length; i++)
        {
            Cv2.Line(Image, Points[i - 1], Points[i], Color, Thickness);
        }
        Cv2.Line(Image, Points[Points.Length - 1], Points[0], Color, Thickness);
    }
    private Point CalculateCenter(Point[] points)
    {
        int sumX = 0;
        int sumY = 0;

        foreach (Point point in points)
        {
            sumX += point.X;
            sumY += point.Y;
        }

        int centerX = sumX / points.Length;
        int centerY = sumY / points.Length;

        return new Point(centerX, centerY);
    }
    // Function to calculate the radius of the circle
    private float CalculateRadius(Point center, Point[] points)
    {
        float maxDistance = 0;

        foreach (Point point in points)
        {
            float distance = Mathf.Sqrt(Mathf.Pow(center.X - point.X, 2) + Mathf.Pow(center.Y - point.Y, 2));

            if (distance > maxDistance)
            {
                maxDistance = distance;
            }
        }

        return maxDistance;
    }
}