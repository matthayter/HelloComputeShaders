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

    [SerializeField] int m_objectCount;
    const int GROUP_SIZE = 256;

    // Flocking params
    public float m_rotationSpeed = 1f;
    public float m_boidSpeed = 1f;
    public float m_neighbourDistance = 1f;

    // 16 byte alignment
    public struct MyBufferData {
        public Vector3 position;
        public Vector3 direction;
        public float speed;
        public float padding0;
    }
    const int SIZE_OF_STRUCT = 32;

    // Start is called before the first frame update
    void Start()
    {
        m_kernelHandle = m_computeShader.FindKernel("CSMain");
        m_drawArgs = new ComputeBuffer(
            1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments
        );
        m_drawArgs.SetData(new uint[5] {
            m_myMesh.GetIndexCount(0), (uint)m_objectCount, 0, 0, 0
        });

        // Number of struct instances, size of struct
        m_sharedBuffer = new ComputeBuffer(m_objectCount, SIZE_OF_STRUCT);

        MyBufferData[] initBufferData = new MyBufferData[m_objectCount];
        for (int i = 0; i < m_objectCount; i++) {
            initBufferData[i].position = Random.insideUnitSphere;
            initBufferData[i].direction = Random.onUnitSphere;
            initBufferData[i].speed = m_boidSpeed;
        }

        m_sharedBuffer.SetData(initBufferData);

        m_computeShader.SetInt("BoidsCount", m_objectCount);
    }

    // Update is called once per frame
    void Update()
    {
        m_computeShader.SetFloat("RotationSpeed", m_rotationSpeed);
        m_computeShader.SetFloat("BoidSpeed", m_boidSpeed);
        m_computeShader.SetFloat("NeighbourDistance", m_neighbourDistance);

        m_computeShader.SetFloat("DeltaTime", Time.deltaTime);
        m_computeShader.SetVector("FlockPosition", this.transform.position);

        m_computeShader.SetBuffer(m_kernelHandle, "sharedBuffer", m_sharedBuffer);
        m_computeShader.Dispatch(m_kernelHandle, m_objectCount / GROUP_SIZE + 1, 1, 1);

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
