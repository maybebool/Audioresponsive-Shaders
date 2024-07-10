using UnityEngine;

public class GridInstancer : MonoBehaviour
{

    public uint rez = 64; // How many instances per side. (rez * rez => total instances)
    public CustomRenderTexture inTex; // The texture we will use as a base
    
    public Material outMat; // the instancing shader material
    public Mesh iMesh; // the mesh to instance
    
    private ComputeBuffer argsB; // instance info to pass to the shader

    private Bounds bounds;
    
    void Start()
    {
        // Indirect Args Buffer determines how many instances to draw
        argsB = new ComputeBuffer(5, sizeof(int), ComputeBufferType.IndirectArguments);
        var args = new uint[5];
        args[0] = (uint) iMesh.GetIndexCount(0); // index count per instance,
        args[1] = (uint) (rez*rez); // instance count,
        args[2] = (uint) iMesh.GetIndexStart(0); // start index location,
        args[3] = (uint) iMesh.GetBaseVertex(0); // base vertex location,
        args[4] = 0; // start instance location.
        argsB.SetData(args); // Store it to a buffer in GPU land

        // Set bounds
        bounds = new Bounds(Vector3.zero, Vector3.one * 100);
        
        // Pass texture to shader graph
        outMat.SetTexture("_inTex", inTex);
        outMat.SetFloat("_Rez", rez);

    }

    void Update()
    {
        Graphics.DrawMeshInstancedIndirect(iMesh, 0, outMat, bounds, argsB);
    }

    private void OnDestroy()
    {
        argsB.Release();
    }
}
