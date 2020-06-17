using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Render_PostProcess : MonoBehaviour
{
    private GameObject player;

    [SerializeField]
    private float coef_blur = 300.0f;
    [SerializeField]
    private float coef_intensity = 16.0f;

    [SerializeField]
    private float radius = 0.6f;
    [SerializeField]
    private Vector3 offSetColor;
    [SerializeField]
    private bool state_blur;
    [SerializeField]
    private bool state_chromatique;
    [SerializeField]
    private bool state_vignette_pleine;
    [SerializeField]
    private float lerp_effet;

    public bool shader_actif = false;
    private Material mat;
    public string shader_name = "NoVision";
    private Camera cam;
    private RenderTexture RT;
    int it = 0;

    float time_actual = 0.0f;
    float time_refresh = 1.0f;

    public float coef = 0.001f;

    public float offsetX = 0.0f;
    public float offsetY = 0.0f;

    // Start is called before the first frame update
    //a l'init ici
    void Awake()
    {
        state_blur = true;
        state_chromatique = true;
        //courbe blur a gérer en fonction de l'état de vie

        player = GameObject.FindGameObjectWithTag("Player");

        if(player == null)
        {
            throw new System.NullReferenceException("Missing an object tagged Player in the scene");
        }

        lerp_effet = 1.0f;

        cam = GetComponent<Camera>();
        changeShader("PostProcessV1");
    }

    public void changeShader(string name)
    {
        shader_name = name;
        mat = new Material(Shader.Find("Shaders/" + shader_name));
    }


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //activé
        if (!shader_actif)
        {
            Graphics.Blit(source, destination);
        }
        else
        {
            mat.SetFloat("type", 0);

            changeShader("PostProcessV1");
            mat.SetFloat("width", coef_blur);
            mat.SetFloat("height", coef_blur);
            mat.SetFloat("life", player.GetComponent<EnergyBehaviour>().Energy / 10); // 0-1000 -> 0-100
            mat.SetFloat("_CoefBlur", coef_intensity);
            mat.SetFloat("_Radius", radius);
            mat.SetVector("_OffsetColor",new Vector4(offSetColor.x, offSetColor.y, offSetColor.z,1.0f));
            mat.SetInt("_StateBlur",(state_blur ? 1 : 0));
            mat.SetInt("_StateChromatique", (state_chromatique ? 1 : 0));
            mat.SetInt("_VignettePleine", (state_vignette_pleine ? 1 : 0));
            mat.SetFloat("_LerpEffect", lerp_effet);


            Graphics.Blit(source, destination,mat);
        }
    }
}
