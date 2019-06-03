using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderStarter : MonoBehaviour
{
    [SerializeField] ComputeShader m_computeShader;
    private int m_kernelHandle;

    private ComputeBuffer m_sharedBuffer;

    [SerializeField] Material m_myMaterial;
    [SerializeField] Mesh m_myMesh;
    private ComputeBuffer m_drawArgs;

    // 16 byte alignment
    public struct MyBufferData {
        public Vector3 position;
        public float padding0;
    }
    const int SIZE_OF_STRUCT = 16;

    // Start is called before the first frame update
    void Start()
    {
        m_kernelHandle = m_computeShader.FindKernel("CSMain");
        m_drawArgs = new ComputeBuffer(
            1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments
        );
        m_drawArgs.SetData(new uint[5] {
            m_myMesh.GetIndexCount(0), 1, 0, 0, 0
        });


        // Number of struct instances, size of struct
        m_sharedBuffer = new ComputeBuffer(1, SIZE_OF_STRUCT);

        MyBufferData[] initBufferData = new MyBufferData[1];
        initBufferData[0].position = Vector3.zero;

        m_sharedBuffer.SetData(initBufferData);
    }

    // Update is called once per frame
    void Update()
    {
        m_computeShader.SetBuffer(m_kernelHandle, "sharedBuffer", m_sharedBuffer);
        m_computeShader.Dispatch(m_kernelHandle, 1, 1, 1);

        m_myMaterial.SetBuffer("sharedBuffer", m_sharedBuffer);
        Graphics.DrawMeshInstancedIndirect(m_myMesh, 0, m_myMaterial, new Bounds(Vector3.zero, Vector3.one*1000), m_drawArgs, 0);
    }

    void OnDestroy() {
        if (m_sharedBuffer != null) {
            m_sharedBuffer.Release();
            m_sharedBuffer = null;
        }
        if (m_drawArgs != null) m_drawArgs.Release();
    }
}
