using UnityEngine;

public class GameOfLife_ComputeShader : MonoBehaviour
{
    [SerializeField] private ComputeShader _computeShader;
    [SerializeField] private Texture _initialTexture;

    private RenderTexture _outputTexture;
    private RenderTexture _bufferTexture;
    private bool _swap;

    private void Start()
    {
        // Create two render textures for double buffering
        _outputTexture = CreateRenderTexture();
        _bufferTexture = CreateRenderTexture();

        // Initialize the first buffer with the initial texture
        Graphics.Blit(_initialTexture, _outputTexture);
    }

    private RenderTexture CreateRenderTexture()
    {
        RenderTexture rt = new RenderTexture(_initialTexture.width, _initialTexture.height, 24);
        rt.enableRandomWrite = true;
        rt.filterMode = FilterMode.Point;
        rt.Create();
        return rt;
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // Select the current input and output textures based on the swap flag
        RenderTexture inputTexture = _swap ? _bufferTexture : _outputTexture;
        RenderTexture resultTexture = _swap ? _outputTexture : _bufferTexture;

        // Set the textures for the compute shader
        _computeShader.SetTexture(0, "InputTexture", inputTexture);
        _computeShader.SetTexture(0, "Result", resultTexture);
        _computeShader.SetInts("resolution", new int[] { inputTexture.width, inputTexture.height });

        // Dispatch the compute shader
        int threadGroupsX = Mathf.CeilToInt(inputTexture.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(inputTexture.height / 8.0f);
        _computeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        // Blit the result texture to the screen
        Graphics.Blit(resultTexture, dest);

        // Swap the buffers
        _swap = !_swap;
    }
}