using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum BorderType
{
    Inner,  // Border goes inward, reducing fill area
    Outer   // Border goes outward, expanding total size
}

[RequireComponent(typeof(RectTransform))]
[AddComponentMenu("UI/Effects/Rounded Corners")]
public class RoundedCorners : BaseMeshEffect
{
    [Header("Corner Settings")]
    [SerializeField] private bool useUniformRadius = true;

    [SerializeField] private float uniformRadius = 10f;

    [Header("Individual Corner Radius")]
    [SerializeField] private float topLeftRadius = 10f;
    [SerializeField] private float topRightRadius = 10f;
    [SerializeField] private float bottomLeftRadius = 10f;
    [SerializeField] private float bottomRightRadius = 10f;

    [Header("Quality Settings")]
    [SerializeField][Range(4, 32)] private int cornerSegments = 12;

    [Header("Border Settings")]
    [SerializeField] private bool hasBorder = false;
    [SerializeField] private BorderType borderType = BorderType.Inner;
    [SerializeField] private float borderWidth = 2f;
    [SerializeField] private Color borderColor = Color.black;

    // Properties for easy access
    public float UniformRadius
    {
        get => uniformRadius;
        set
        {
            uniformRadius = value;
            RefreshMesh();
        }
    }

    public bool UseUniformRadius
    {
        get => useUniformRadius;
        set
        {
            useUniformRadius = value;
            RefreshMesh();
        }
    }

    public float TopLeftRadius
    {
        get => topLeftRadius;
        set
        {
            topLeftRadius = value;
            RefreshMesh();
        }
    }

    public float TopRightRadius
    {
        get => topRightRadius;
        set
        {
            topRightRadius = value;
            RefreshMesh();
        }
    }

    public float BottomLeftRadius
    {
        get => bottomLeftRadius;
        set
        {
            bottomLeftRadius = value;
            RefreshMesh();
        }
    }

    public float BottomRightRadius
    {
        get => bottomRightRadius;
        set
        {
            bottomRightRadius = value;
            RefreshMesh();
        }
    }

    public int CornerSegments
    {
        get => cornerSegments;
        set
        {
            cornerSegments = Mathf.Clamp(value, 4, 32);
            RefreshMesh();
        }
    }

    public BorderType BorderType
    {
        get => borderType;
        set
        {
            borderType = value;
            RefreshMesh();
        }
    }

    public bool HasBorder
    {
        get => hasBorder;
        set
        {
            hasBorder = value;
            RefreshMesh();
        }
    }

    public float BorderWidth
    {
        get => borderWidth;
        set
        {
            borderWidth = value;
            RefreshMesh();
        }
    }

    public Color BorderColor
    {
        get => borderColor;
        set
        {
            borderColor = value;
            RefreshMesh();
        }
    }

    private void RefreshMesh()
    {
        if (graphic != null)
        {
            graphic.SetVerticesDirty();
        }
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        RectTransform rectTransform = GetComponent<RectTransform>();
        Rect rect = rectTransform.rect;

        // Get the actual radius values to use
        float tlRadius = useUniformRadius ? uniformRadius : topLeftRadius;
        float trRadius = useUniformRadius ? uniformRadius : topRightRadius;
        float blRadius = useUniformRadius ? uniformRadius : bottomLeftRadius;
        float brRadius = useUniformRadius ? uniformRadius : bottomRightRadius;

        // Clamp radius values to prevent overlapping
        float maxRadius = Mathf.Min(rect.width, rect.height) * 0.5f;
        tlRadius = Mathf.Min(tlRadius, maxRadius);
        trRadius = Mathf.Min(trRadius, maxRadius);
        blRadius = Mathf.Min(blRadius, maxRadius);
        brRadius = Mathf.Min(brRadius, maxRadius);

        vh.Clear();

        // Create the rounded rectangle mesh
        CreateRoundedRectMesh(vh, rect, tlRadius, trRadius, blRadius, brRadius);
    }

    private void CreateRoundedRectMesh(VertexHelper vh, Rect rect, float tlRadius, float trRadius, float blRadius, float brRadius)
    {
        List<Vector2> outerVertices = new List<Vector2>();

        // Generate smooth outer vertices
        GenerateSmoothVertices(outerVertices, rect, tlRadius, trRadius, blRadius, brRadius);

        Image image = GetComponent<Image>();
        Color color = image ? image.color : Color.white;

        if (hasBorder && borderWidth > 0)
        {
            // Create border with inner and outer shapes
            CreateBorderMesh(vh, rect, tlRadius, trRadius, blRadius, brRadius, color);
        }
        else
        {
            // Create simple filled shape
            CreateFilledMesh(vh, outerVertices, rect, color);
        }
    }

    private void CreateFilledMesh(VertexHelper vh, List<Vector2> outerVertices, Rect rect, Color color)
    {
        // Create the mesh using a fan triangulation from center
        Vector2 center = rect.center;
        Vector2 centerUV = new Vector2(0.5f, 0.5f);
        vh.AddVert(center, color, centerUV);

        // Add outer vertices
        for (int i = 0; i < outerVertices.Count; i++)
        {
            Vector2 vertex = outerVertices[i];
            Vector2 uv = new Vector2(
                (vertex.x - rect.xMin) / rect.width,
                (vertex.y - rect.yMin) / rect.height
            );
            vh.AddVert(vertex, color, uv);
        }

        // Create triangles from center to outer edge
        int vertexCount = outerVertices.Count;
        for (int i = 0; i < vertexCount; i++)
        {
            int current = i + 1; // +1 because center is at index 0
            int next = (i + 1) % vertexCount + 1;
            vh.AddTriangle(0, current, next);
        }
    }

    private void CreateBorderMesh(VertexHelper vh, Rect rect, float tlRadius, float trRadius, float blRadius, float brRadius, Color fillColor)
    {
        List<Vector2> borderVertices = new List<Vector2>();
        List<Vector2> fillVertices = new List<Vector2>();

        if (borderType == BorderType.Inner)
        {
            // Inner border: border shape = original rect, fill shape = shrunk rect
            GenerateSmoothVertices(borderVertices, rect, tlRadius, trRadius, blRadius, brRadius);

            // Calculate inner rect (shrunk by border width)
            Rect innerRect = new Rect(
                rect.x + borderWidth,
                rect.y + borderWidth,
                Mathf.Max(0, rect.width - borderWidth * 2),
                Mathf.Max(0, rect.height - borderWidth * 2)
            );

            // Adjust inner radii proportionally
            float widthRatio = innerRect.width > 0 ? innerRect.width / rect.width : 0;
            float heightRatio = innerRect.height > 0 ? innerRect.height / rect.height : 0;
            float radiusScale = Mathf.Min(widthRatio, heightRatio);

            float innerTlRadius = Mathf.Max(0, tlRadius * radiusScale);
            float innerTrRadius = Mathf.Max(0, trRadius * radiusScale);
            float innerBlRadius = Mathf.Max(0, blRadius * radiusScale);
            float innerBrRadius = Mathf.Max(0, brRadius * radiusScale);

            if (innerRect.width > 0 && innerRect.height > 0)
            {
                GenerateSmoothVertices(fillVertices, innerRect, innerTlRadius, innerTrRadius, innerBlRadius, innerBrRadius);
            }
        }
        else // BorderType.Outer
        {
            // Outer border: border shape = expanded rect, fill shape = original rect
            GenerateSmoothVertices(fillVertices, rect, tlRadius, trRadius, blRadius, brRadius);

            // Calculate outer rect (expanded by border width)
            Rect outerRect = new Rect(
                rect.x - borderWidth,
                rect.y - borderWidth,
                rect.width + borderWidth * 2,
                rect.height + borderWidth * 2
            );

            // Adjust outer radii
            float outerTlRadius = tlRadius + borderWidth;
            float outerTrRadius = trRadius + borderWidth;
            float outerBlRadius = blRadius + borderWidth;
            float outerBrRadius = brRadius + borderWidth;

            GenerateSmoothVertices(borderVertices, outerRect, outerTlRadius, outerTrRadius, outerBlRadius, outerBrRadius);
        }

        // Add border vertices
        for (int i = 0; i < borderVertices.Count; i++)
        {
            Vector2 vertex = borderVertices[i];
            Vector2 uv = CalculateUV(vertex, rect, borderType == BorderType.Outer ? borderWidth : 0);
            vh.AddVert(vertex, borderColor, uv);
        }

        // Add fill vertices
        int fillStartIndex = borderVertices.Count;
        for (int i = 0; i < fillVertices.Count; i++)
        {
            Vector2 vertex = fillVertices[i];
            Vector2 uv = CalculateUV(vertex, rect, borderType == BorderType.Outer ? borderWidth : 0);
            vh.AddVert(vertex, fillColor, uv);
        }

        // Create border triangulation
        if (borderVertices.Count > 0 && fillVertices.Count > 0)
        {
            CreateConsistentBorderTriangles(vh, borderVertices.Count, fillVertices.Count, fillStartIndex);
        }

        // Create fill triangulation
        if (fillVertices.Count > 2)
        {
            Vector2 fillCenter = CalculateCentroid(fillVertices);
            Vector2 centerUV = CalculateUV(fillCenter, rect, borderType == BorderType.Outer ? borderWidth : 0);
            int centerIndex = vh.currentVertCount;
            vh.AddVert(fillCenter, fillColor, centerUV);

            // Triangulate fill area from center
            for (int i = 0; i < fillVertices.Count; i++)
            {
                int current = fillStartIndex + i;
                int next = fillStartIndex + (i + 1) % fillVertices.Count;
                vh.AddTriangle(centerIndex, current, next);
            }
        }
        else if (borderVertices.Count > 2)
        {
            // No fill area, just create border as solid
            Vector2 borderCenter = CalculateCentroid(borderVertices);
            Vector2 centerUV = CalculateUV(borderCenter, rect, borderType == BorderType.Outer ? borderWidth : 0);
            int centerIndex = vh.currentVertCount;
            vh.AddVert(borderCenter, borderColor, centerUV);

            for (int i = 0; i < borderVertices.Count; i++)
            {
                int current = i;
                int next = (i + 1) % borderVertices.Count;
                vh.AddTriangle(centerIndex, current, next);
            }
        }
    }

    private Vector2 CalculateUV(Vector2 vertex, Rect originalRect, float expansion)
    {
        Rect uvRect = new Rect(
            originalRect.x - expansion,
            originalRect.y - expansion,
            originalRect.width + expansion * 2,
            originalRect.height + expansion * 2
        );

        return new Vector2(
            (vertex.x - uvRect.xMin) / uvRect.width,
            (vertex.y - uvRect.yMin) / uvRect.height
        );
    }

    private Vector2 CalculateCentroid(List<Vector2> vertices)
    {
        if (vertices.Count == 0) return Vector2.zero;

        Vector2 centroid = Vector2.zero;
        foreach (Vector2 vertex in vertices)
        {
            centroid += vertex;
        }
        return centroid / vertices.Count;
    }

    private void CreateConsistentBorderTriangles(VertexHelper vh, int borderCount, int fillCount, int fillStartIndex)
    {
        // Ensure consistent triangle distribution to prevent dissolving
        if (borderCount == 0 || fillCount == 0) return;

        // Map vertices consistently around the perimeter
        for (int i = 0; i < Mathf.Max(borderCount, fillCount); i++)
        {
            // Calculate corresponding indices with wrapping
            int borderIndex1 = i % borderCount;
            int borderIndex2 = (i + 1) % borderCount;
            int fillIndex1 = (i * fillCount / Mathf.Max(borderCount, 1)) % fillCount + fillStartIndex;
            int fillIndex2 = ((i + 1) * fillCount / Mathf.Max(borderCount, 1)) % fillCount + fillStartIndex;

            // Create consistent quads to prevent gaps
            if (i < borderCount && i < fillCount)
            {
                vh.AddTriangle(borderIndex1, fillIndex1, fillIndex2);
                vh.AddTriangle(borderIndex1, fillIndex2, borderIndex2);
            }
        }
    }

    private void GenerateSmoothVertices(List<Vector2> vertices, Rect rect, float tlRadius, float trRadius, float blRadius, float brRadius)
    {
        // Ensure radii don't overlap
        float maxRadiusX = rect.width * 0.5f;
        float maxRadiusY = rect.height * 0.5f;

        tlRadius = Mathf.Min(tlRadius, Mathf.Min(maxRadiusX, maxRadiusY));
        trRadius = Mathf.Min(trRadius, Mathf.Min(maxRadiusX, maxRadiusY));
        blRadius = Mathf.Min(blRadius, Mathf.Min(maxRadiusX, maxRadiusY));
        brRadius = Mathf.Min(brRadius, Mathf.Min(maxRadiusX, maxRadiusY));

        // Start from bottom-left and go clockwise

        // Bottom-left corner
        if (blRadius > 0.001f)
        {
            Vector2 cornerCenter = new Vector2(rect.xMin + blRadius, rect.yMin + blRadius);
            AddSmoothCorner(vertices, cornerCenter, blRadius, 180f, 270f);
        }
        else
        {
            vertices.Add(new Vector2(rect.xMin, rect.yMin));
        }

        // Bottom edge
        if (rect.width > blRadius + brRadius + 0.001f)
        {
            vertices.Add(new Vector2(rect.xMax - brRadius, rect.yMin));
        }

        // Bottom-right corner
        if (brRadius > 0.001f)
        {
            Vector2 cornerCenter = new Vector2(rect.xMax - brRadius, rect.yMin + brRadius);
            AddSmoothCorner(vertices, cornerCenter, brRadius, 270f, 0f);
        }
        else if (rect.width > blRadius + 0.001f)
        {
            vertices.Add(new Vector2(rect.xMax, rect.yMin));
        }

        // Right edge
        if (rect.height > brRadius + trRadius + 0.001f)
        {
            vertices.Add(new Vector2(rect.xMax, rect.yMax - trRadius));
        }

        // Top-right corner
        if (trRadius > 0.001f)
        {
            Vector2 cornerCenter = new Vector2(rect.xMax - trRadius, rect.yMax - trRadius);
            AddSmoothCorner(vertices, cornerCenter, trRadius, 0f, 90f);
        }
        else if (rect.height > brRadius + 0.001f)
        {
            vertices.Add(new Vector2(rect.xMax, rect.yMax));
        }

        // Top edge
        if (rect.width > trRadius + tlRadius + 0.001f)
        {
            vertices.Add(new Vector2(rect.xMin + tlRadius, rect.yMax));
        }

        // Top-left corner
        if (tlRadius > 0.001f)
        {
            Vector2 cornerCenter = new Vector2(rect.xMin + tlRadius, rect.yMax - tlRadius);
            AddSmoothCorner(vertices, cornerCenter, tlRadius, 90f, 180f);
        }
        else if (rect.width > trRadius + 0.001f)
        {
            vertices.Add(new Vector2(rect.xMin, rect.yMax));
        }

        // Left edge
        if (rect.height > tlRadius + blRadius + 0.001f)
        {
            vertices.Add(new Vector2(rect.xMin, rect.yMin + blRadius));
        }
    }

    private void AddSmoothCorner(List<Vector2> vertices, Vector2 center, float radius, float startAngle, float endAngle)
    {
        // Normalize angles
        if (endAngle < startAngle)
            endAngle += 360f;

        float angleRange = endAngle - startAngle;

        // Add corner vertices with smooth interpolation
        for (int i = 0; i <= cornerSegments; i++)
        {
            float t = (float)i / cornerSegments;
            // Use smooth interpolation for better curve quality
            float smoothT = t * t * (3f - 2f * t); // Smoothstep function
            float angle = startAngle + angleRange * smoothT;
            float radian = angle * Mathf.Deg2Rad;

            Vector2 point = center + new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)) * radius;
            vertices.Add(point);
        }
    }

    

#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();
        uniformRadius = 10f;
        cornerSegments = 12;
        useUniformRadius = true;
        borderType = BorderType.Inner;
        hasBorder = false;
        borderWidth = 2f;
        borderColor = Color.black;
    }
#endif
}