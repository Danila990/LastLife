using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
public class OutlineHighLight : MonoBehaviour
{
    private bool _isEnabled;
    private bool _isInited;
    private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

    public enum Mode
    {
        OutlineAll,
        OutlineVisible,
        OutlineHidden,
        OutlineAndSilhouette,
        SilhouetteOnly
    }

    public Mode OutlineMode
    {
        get { return outlineMode; }
        set
        {
            outlineMode = value;
            needsUpdate = true;
        }
    }

    public Color OutlineColor
    {
        get { return outlineColor; }
        set
        {
            outlineColor = value;
            needsUpdate = true;
        }
    }

    public float OutlineWidth
    {
        get { return outlineWidth; }
        set
        {
            outlineWidth = value;
            needsUpdate = true;
        }
    }

    [Serializable]
    private class ListVector3
    {
        public List<Vector3> data;
    }

    [SerializeField] private Mode outlineMode;
    public bool UseCustomOutlineMaterial;
    [SerializeField, ShowIf("UseCustomOutlineMaterial")] private Material customOutlineMaterial;
    [SerializeField, HideIf("UseCustomOutlineMaterial")] private Color outlineColor = Color.white;

    [SerializeField, Range(0f, 10f)] private float outlineWidth = 2f;

    [Header("Optional")]
    [SerializeField, Tooltip(
         "Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. "
         + "Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
    private bool precomputeOutline;
    [SerializeField] private bool _selfInit;

    [SerializeField, HideInInspector] private List<Mesh> bakeKeys = new List<Mesh>();

    [SerializeField, HideInInspector] private List<ListVector3> bakeValues = new List<ListVector3>();

    [SerializeField] private MeshRenderer[] _meshRenderers;
    private Renderer[] renderers;
    private Material outlineMaskMaterial;
    private Material outlineFillMaterial;
    
    private bool needsUpdate;
    private static readonly int OutlineColor1 = Shader.PropertyToID("_OutlineColor");
    private static readonly int ZTest = Shader.PropertyToID("_ZTest");
    private static readonly int Width = Shader.PropertyToID("_OutlineWidth");

    public void Init()
    {
        if (!enabled)
            return;
        renderers = _meshRenderers.Length == 0 ? GetComponentsInChildren<Renderer>() : _meshRenderers;
        outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));
        outlineMaskMaterial.name = "OutlineMask (Instance)";

        if (UseCustomOutlineMaterial)
        {
            outlineFillMaterial = Instantiate(customOutlineMaterial);
        }
        else
        {
            outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));
        }
        
        outlineFillMaterial.name = "OutlineFill (Instance)";

        LoadSmoothNormals();
        needsUpdate = true;
        _isInited = true;
    }

    void SetMaterials()
    {
        foreach (var rend in renderers)
        {
            var materials = rend.sharedMaterials.ToList();
            materials.Add(outlineMaskMaterial);
            materials.Add(outlineFillMaterial);
            rend.materials = materials.ToArray();
        }
    }

    private void Update()
    {
        if (needsUpdate)
        {
            needsUpdate = false;
            UpdateMaterialProperties();
        }
    }

    private void OnDisable()
    {
        Disable();
    }

    public void Disable()
    {
        if(!_isEnabled) return;
        _isEnabled = false;
        if(renderers is null) return;
        foreach (var renderer in renderers)
        {
            var materials = renderer.sharedMaterials.ToList();
            materials.Remove(outlineMaskMaterial);
            materials.Remove(outlineFillMaterial);
            renderer.materials = materials.ToArray();
        }
    }

    public void Enable()
    {
        if(_isEnabled || !_isInited) return;
        _isEnabled = true;
        SetMaterials();
    }

    private void Start()
    {
        if (!_selfInit) return;
        SelfInit().Forget();
    }

    private async UniTaskVoid SelfInit()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        Init();
        await UniTask.NextFrame(ct);
        Enable();
    }

    void OnDestroy()
    {
        Destroy(outlineMaskMaterial);
        Destroy(outlineFillMaterial);
    }

    void Bake()
    {
        var bakedMeshes = new HashSet<Mesh>();

        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            if (!bakedMeshes.Add(meshFilter.sharedMesh))
            {
                continue;
            }

            var smoothNormals = SmoothNormals(meshFilter.sharedMesh);

            bakeKeys.Add(meshFilter.sharedMesh);
            bakeValues.Add(new ListVector3() { data = smoothNormals });
        }
    }

    void LoadSmoothNormals()
    {
        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            if (!registeredMeshes.Add(meshFilter.sharedMesh))
            {
                continue;
            }

            var index = bakeKeys.IndexOf(meshFilter.sharedMesh);
            var smoothNormals = (index >= 0) ? bakeValues[index].data : SmoothNormals(meshFilter.sharedMesh);
            meshFilter.sharedMesh.SetUVs(3, smoothNormals);
            var rend = meshFilter.GetComponent<Renderer>();
            if (rend != null)
            {
                CombineSubmeshes(meshFilter.sharedMesh, rend.sharedMaterials);
            }
        }

        foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (!registeredMeshes.Add(skinnedMeshRenderer.sharedMesh))
            {
                continue;
            }

            var sharedMesh = skinnedMeshRenderer.sharedMesh;
            sharedMesh.uv4 = new Vector2[sharedMesh.vertexCount];
            CombineSubmeshes(sharedMesh, skinnedMeshRenderer.sharedMaterials);
        }
    }

    List<Vector3> SmoothNormals(Mesh mesh)
    {
        var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index))
            .GroupBy(pair => pair.Key);
        var smoothNormals = new List<Vector3>(mesh.normals);
        foreach (var group in groups)
        {
            if (group.Count() == 1)
            {
                continue;
            }

            var smoothNormal = Vector3.zero;
            foreach (var pair in group)
            {
                smoothNormal += smoothNormals[pair.Value];
            }

            smoothNormal.Normalize();
            foreach (var pair in group)
            {
                smoothNormals[pair.Value] = smoothNormal;
            }
        }

        return smoothNormals;
    }

    void CombineSubmeshes(Mesh mesh, Material[] materials)
    {
        if (mesh.subMeshCount == 1)
        {
            return;
        }

        if (mesh.subMeshCount > materials.Length)
        {
            return;
        }

        mesh.subMeshCount++;
        mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
    }

    void UpdateMaterialProperties()
    {
        if (!UseCustomOutlineMaterial)
        {
            outlineFillMaterial.SetColor(OutlineColor1, outlineColor);
        }
        switch (outlineMode)
        {
            case Mode.OutlineAll:
                outlineMaskMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat(Width, outlineWidth);
                break;

            case Mode.OutlineVisible:
                outlineMaskMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat(Width, outlineWidth);
                break;

            case Mode.OutlineHidden:
                outlineMaskMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Greater);
                outlineFillMaterial.SetFloat(Width, outlineWidth);
                break;

            case Mode.OutlineAndSilhouette:
                outlineMaskMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat(Width, outlineWidth);
                break;

            case Mode.SilhouetteOnly:
                outlineMaskMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Greater);
                outlineFillMaterial.SetFloat(Width, 0f);
                break;
        }
    }
}