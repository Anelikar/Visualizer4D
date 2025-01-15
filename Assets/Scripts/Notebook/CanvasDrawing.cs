using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A proof of concept behaviour that sets up a draw texture and a brush and provides a method to draw using them
/// </summary>
public class CanvasDrawing : MonoBehaviour
{
    [SerializeField]
    [Tooltip("UI image that will hold a texture to draw on.\nCan't be changed at runtime")]
    UnityEngine.UI.Image m_image;

    [SerializeField]
    [Tooltip("Resolution of a draw texture\nCan't be changed at runtime")]
    Vector2Int m_spriteResolution = new Vector2Int(142, 200);
    Texture2D m_tex;

    [SerializeField]
    [Tooltip("Background color of the draw texture\nCan't be changed at runtime")]
    Color m_canvasColor = new Color(0, 0, 0, 0);

    [SerializeField]
    [Tooltip("Color of the brush\nCan't be changed at runtime")]
    Color m_drawColor = new Color(0, 0, 0, 1);

    [SerializeField]
    [Tooltip("Radius of the brush\nCan't be changed at runtime")]
    [Range(0, 5)]
    int m_brushRadius = 3;
    int m_brushDiameter;

    [SerializeField]
    [Tooltip("Minimum transparency of the brush edge\nCan't be changed at runtime")]
    [Range(0, 1)]
    float m_edgeTransparency = 0f;

    [SerializeField]
    [Tooltip("Color dropoff from the center of the brush\nCan't be changed at runtime")]
    [Range(0.5f, 1.5f)]
    float m_brushSpreadMulti = 0.9f;
    Color[] m_brush;

    enum ApplicationType
    {
        Add,
        Lerp
    }
    [SerializeField]
    ApplicationType m_applicationType = ApplicationType.Add;

    delegate Color[] ApplyBrush(Color[] c);
    ApplyBrush m_applyBrush;

    [SerializeField]
    [Tooltip("Localspace bounds of the texture\nCan't be changed at runtime")]
    Vector2[] m_bounds = new Vector2[] { new Vector2(-0.075f, -0.105f), new Vector2(0.075f, 0.105f) };
    Vector2 m_boundsLength;

    void Awake() {
        if (m_image != null) {
            // Initializing texture, brush and the sprite
            InitTex();
            InitBrush();
            m_image.sprite = Sprite.Create(m_tex, new Rect(0, 0, m_tex.width, m_tex.height), Vector2.zero);
            m_boundsLength = new Vector2(Mathf.Abs(m_bounds[0].x) + Mathf.Abs(m_bounds[1].x), Mathf.Abs(m_bounds[0].y) + Mathf.Abs(m_bounds[1].y));

            // Initializing brush application type
            switch (m_applicationType) {
                case ApplicationType.Add:
                    m_applyBrush = ApplyBrushAdd;
                    break;
                case ApplicationType.Lerp:
                    m_applyBrush = ApplyBrushLerp;
                    break;
                default:
                    m_applyBrush = ApplyBrushAdd;
                    break;
            }
        }
    }

    private void OnDrawGizmosSelected() {
        // Drawing boundary lines in editor
        Gizmos.color = Color.red;
        Vector3 bl = transform.rotation * new Vector3(m_bounds[0].x, 0, m_bounds[0].y) + transform.position;
        Vector3 br = transform.rotation * new Vector3(m_bounds[1].x, 0, m_bounds[0].y) + transform.position;
        Vector3 tl = transform.rotation * new Vector3(m_bounds[0].x, 0, m_bounds[1].y) + transform.position;
        Vector3 tr = transform.rotation * new Vector3(m_bounds[1].x, 0, m_bounds[1].y) + transform.position;
        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);
    }

    /// <summary>
    /// Sets up the brush array
    /// </summary>
    void InitBrush() {
        // Initializing brush array size
        m_brushDiameter = m_brushRadius * 2 + 1;
        m_brush = new Color[m_brushDiameter * m_brushDiameter];

        // Initializing brush array alphas
        for (int i = 0; i < m_brushDiameter; i++) {
            for (int j = 0; j < m_brushDiameter; j++) {
                m_brush[i * m_brushDiameter + j] = m_drawColor;
                float len1 = Mathf.Abs(i - m_brushRadius) * m_brushSpreadMulti;
                float len2 = Mathf.Abs(j - m_brushRadius) * m_brushSpreadMulti;
                float dst = Mathf.Sqrt(len1 * len1 + len2 * len2) / m_brushRadius;
                m_brush[i * m_brushDiameter + j].a = Mathf.Lerp(1, m_edgeTransparency, Mathf.Clamp01(dst));
            }
        }
    }

    /// <summary>
    /// Creates a new texture with the same size as spriteResolution and saves it in m_tex
    /// </summary>
    void InitTex() {
        m_tex = new Texture2D(m_spriteResolution.x, m_spriteResolution.y);
        Clear();
    }

    /// <summary>
    /// Sets the entire texture to a canvasColor
    /// </summary>
    public void Clear() {
        Color[] c = new Color[m_tex.width * m_tex.height];
        for (int i = 0; i < c.Length; i++) {
            c[i] = m_canvasColor;
        }
        m_tex.SetPixels(c);
        m_tex.Apply();
    }

    /// <summary>
    /// Converts a 3d world point into a 2d point on a canvas
    /// </summary>
    /// <param name="point">3d worldspace point</param>
    /// <returns>Prijected 2d point on a canvas</returns>
    Vector2 WorldToBounds(Vector3 point) {
        // world to local
        point = point - transform.position;
        // Getting the projection to a draw plane.
        point = Vector3.ProjectOnPlane(point, transform.rotation * Vector3.up);
        // Reversing the rotation of the vector as if it was projected in a default rotation.
        point = Quaternion.Inverse(transform.rotation) * point;
        // Recalculating the vector so it starts from bounds[0] and normalizing its components. Also discarding y.
        return new Vector2((point.x - m_bounds[0].x) / m_boundsLength.x, (point.z - m_bounds[0].y) / m_boundsLength.y);
    }

    /// <summary>
    /// Converts a 2d point on a canvas to a pixel coordinate on a texture
    /// </summary>
    /// <param name="bounds">2d point on a canvas</param>
    /// <returns>Pixel coordinate on a texture</returns>
    Vector2Int BoundsToTexCoord(Vector2 bounds) {
        return new Vector2Int(
            (int)Mathf.Round((m_tex.width - 1) * bounds.x),
            (int)Mathf.Round((m_tex.height - 1) * bounds.y)
            );
    }

    /// <summary>
    /// Applies a brush onto a texture at a 3d worldspace point projected onto canvas
    /// </summary>
    /// <param name="point">3d worldspace brush position that will be projected onto canvas</param>
    public void Draw(Vector3 point) {
        if (m_tex != null) {
            Draw(BoundsToTexCoord(WorldToBounds(point)));
        }
    }

    /// <summary>
    /// Applies a brush onto a texture at a specified texture coordinates
    /// </summary>
    /// <param name="coord">Texture coordinates of a brush</param>
    void Draw(Vector2Int coord) {
        // Checking that the brush is in bounds
        if ((coord.x + m_brushDiameter < m_tex.width) && (coord.y + m_brushDiameter < m_tex.height) && (coord.x >= 0) && (coord.y >= 0)) {
            Color[] pixels = m_tex.GetPixels(coord.x, coord.y, m_brushDiameter, m_brushDiameter);
            pixels = m_applyBrush(pixels);
            m_tex.SetPixels(coord.x, coord.y, m_brushDiameter, m_brushDiameter, pixels);
            m_tex.Apply();
        }
    }

    /// <summary>
    /// Applies a brush to the pixels by lerping between their values
    /// </summary>
    /// <param name="pixels">Affected pixels</param>
    /// <returns>Pixels with the brush applied</returns>
    Color[] ApplyBrushLerp(Color[] pixels) {
        Color[] newPixels = new Color[m_brushDiameter * m_brushDiameter];
        for (int i = 0; i < pixels.Length; i++) {
            newPixels[i] = new Color(
                Mathf.Lerp(pixels[i].r, m_brush[i].r, m_brush[i].a),
                Mathf.Lerp(pixels[i].g, m_brush[i].g, m_brush[i].a),
                Mathf.Lerp(pixels[i].b, m_brush[i].b, m_brush[i].a),
                Mathf.Lerp(pixels[i].a, m_brush[i].a, m_brush[i].a)
                );
        }
        return newPixels;
    }

    /// <summary>
    /// Applies a brush to the pixels by adding their values
    /// </summary>
    /// <param name="pixels">Affected pixels</param>
    /// <returns>Pixels with the brush applied</returns>
    Color[] ApplyBrushAdd(Color[] pixels) {
        Color[] newPixels = new Color[m_brushDiameter * m_brushDiameter];
        for (int i = 0; i < pixels.Length; i++) {
            newPixels[i] = new Color(
                pixels[i].r + m_brush[i].r * m_brush[i].a,
                pixels[i].g + m_brush[i].g * m_brush[i].a,
                pixels[i].b + m_brush[i].b * m_brush[i].a,
                pixels[i].a + m_brush[i].a
                );
        }
        return newPixels;
    }
}
