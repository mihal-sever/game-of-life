using UnityEngine;

public class PaintComputeShader : MonoBehaviour
{
    [SerializeField] private ComputeShader _drawComputeShader;
    [SerializeField] private Color _backgroundColour;
    [SerializeField] private Color _brushColour;
    [SerializeField] private Color _gridLineColor;
    [SerializeField] private uint _gridWidth = 20;
    [SerializeField] private uint _gridHeight = 20;

    private RenderTexture _canvasRenderTexture;
    private int _initBackgroundKernel;
    private int _updateKernel;

    private void Start()
    {
        uint canvasWidth = (uint)Screen.width;
        uint canvasHeight = (uint)Screen.height;
        uint cellSize = (uint)Mathf.Min((int)(canvasWidth / _gridWidth), (int)(canvasHeight / _gridHeight));

        _canvasRenderTexture = new RenderTexture((int)canvasWidth, (int)canvasHeight, 24);
        _canvasRenderTexture.filterMode = FilterMode.Point;
        _canvasRenderTexture.enableRandomWrite = true;
        _canvasRenderTexture.Create();

        _initBackgroundKernel = _drawComputeShader.FindKernel("InitBackground");
        _updateKernel = _drawComputeShader.FindKernel("Update");
        
        _drawComputeShader.SetTexture(_initBackgroundKernel, "_Canvas", _canvasRenderTexture);
        _drawComputeShader.SetTexture(_updateKernel, "_Canvas", _canvasRenderTexture);
        _drawComputeShader.GetKernelThreadGroupSizes(_initBackgroundKernel,
            out uint xGroupSize, out uint yGroupSize, out _);
        _drawComputeShader.Dispatch(_initBackgroundKernel,
            Mathf.CeilToInt((float)canvasWidth / xGroupSize),
            Mathf.CeilToInt((float)canvasHeight / yGroupSize),
            1);
        
        _drawComputeShader.SetVector("_BackgroundColour", _backgroundColour);
        _drawComputeShader.SetVector("_BrushColour", _brushColour);
        _drawComputeShader.SetVector("_GridLineColor", _gridLineColor);
        _drawComputeShader.SetFloat("_CanvasWidth", canvasWidth);
        _drawComputeShader.SetFloat("_CanvasHeight", canvasHeight);
        _drawComputeShader.SetFloat("_GridWidth", _gridWidth);
        _drawComputeShader.SetFloat("_GridHeight", _gridHeight);
        _drawComputeShader.SetFloat("_CellSize", cellSize);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            _drawComputeShader.SetVector("_MousePosition", Input.mousePosition);
            _drawComputeShader.SetBool("_MouseDown", Input.GetMouseButton(0));
            
            _drawComputeShader.GetKernelThreadGroupSizes(_updateKernel,
                out uint xGroupSize, out uint yGroupSize, out _);
            _drawComputeShader.Dispatch(_updateKernel,
                Mathf.CeilToInt((float)_canvasRenderTexture.width / xGroupSize),
                Mathf.CeilToInt((float)_canvasRenderTexture.height / yGroupSize),
                1);
        }
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(_canvasRenderTexture, dest);
    }
}