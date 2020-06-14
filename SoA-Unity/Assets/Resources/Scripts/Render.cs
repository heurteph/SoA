using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct StructTexture
{
    public Texture2D tex;
}

public class Render : MonoBehaviour
{
    [SerializeField]
    public PlayerController pc;

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

    Matrix4x4 projection_base;

    // Start is called before the first frame update
    //a l'init ici
    void Awake()
    {
        state_blur = false;
        state_chromatique = false;
        //courbe blur a gérer en fonction de l'état de vie

        lerp_effet = 1.0f;

        cam = GetComponent<Camera>();
        projection_base = new Matrix4x4();
        for(int i= 0; i < 4; i++)
        {
            projection_base.SetRow(i, cam.projectionMatrix.GetRow(i));
        }

        Debug.Log("Camera "+cam.projectionMatrix);
        Debug.Log("Camera w and h " + cam.pixelWidth + " et " + cam.pixelHeight);
        //Cam format
        //RT = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 24, RenderTextureFormat.ARGB32, 4);
        //FULL HD
        //RT = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32, 4);
        //HD
        //RT = new RenderTexture(1080, 720, 24, RenderTextureFormat.ARGB32, 4);
        RT = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32, 4);
        //RT = new RenderTexture(540, 360, 24, RenderTextureFormat.ARGB32, 11);
        //RT = new RenderTexture(270, 180, 24, RenderTextureFormat.ARGB32, 4);
        RT.volumeDepth = 3;

        //RT.antiAliasing = 1;
        RT.useMipMap = true;
        RT.autoGenerateMips = true;
        RT.enableRandomWrite = true;
        RT.Create();

        //cam.targetTexture = RT;

        if (RT.useMipMap)
        {
            Debug.Log("MipMap activate !"+RT.width+ " "+RT.height);
        }

        //Debug.Log("Camera " + cam.pixelWidth + " " + cam.pixelHeight);


        Debug.Log("RT " + RT);
        changeShader("NoVision");
        //Debug.Log("Resolution "+Screen.currentResolution);
        mat.SetFloat("height", Screen.currentResolution.height);
        mat.SetFloat("width", Screen.currentResolution.width);
    }

    public void changeShader(string name)
    {
        shader_name = name;
        mat = new Material(Shader.Find("Shaders/" + shader_name));
    }

    private void OnPreRender()
    {
        //cam.targetTexture = RT;
    }


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!shader_actif)
        {
            Graphics.Blit(source, destination);
        }
        else
        {

            //texture 512 512
            //avec numthreads 32 32 1
            //512/32 par dim => 16 group
            //si 16 8 1
            //1920 * 1080 => 
            //1920 / 16 => 120
            //1080 / 8 => 135
            //1920/32 => 60
            //1080/32 => 33.75

            //RenderTextureReadWrite texture = source;

            //test vers struct en Compute Shader
            //StructTexture [] str = new StructTexture[1];
            //str[0] = new StructTexture();
            mat.SetFloat("type", 0);

            if (Time.realtimeSinceStartup - time_actual > time_refresh)
            {
                //before create
                //source.useMipMap = true;
                //source.GenerateMips();
                ComputeShader shader = Resources.Load<ComputeShader>("Shaders/Test");
                Debug.Log("Depth est de " + source.format);

                /*RenderTexture RT_0 = new RenderTexture(source.width, source.height, source.depth, source.format, RenderTextureReadWrite.Default);
                RT_0.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
                RT_0.enableRandomWrite = true;
                RT_0.volumeDepth = 3;
                RT_0.Create();*/


                //RenderTexture RT_0 = new RenderTexture(source);
                //RenderTexture RT_0 = new RenderTexture(source.width, source.height, source.depth, source.format, RenderTextureReadWrite.Default);

                /*RenderTexture RT_0 = new RenderTexture(source.width, source.height, source.depth, source.format, 4);
                //RT_0.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
                //RT_0.enableRandomWrite = true;
                RT_0.volumeDepth = 3;

                RT_0.useMipMap = true;
                RT_0.autoGenerateMips = true;
                RT_0.enableRandomWrite = true;
                RT_0.Create();

                //Graphics.CopyTexture(source, RT_0);
                Graphics.Blit(source, RT_0);*/

                //RT_0.GenerateMips();

                RenderTexture RT_0 = new RenderTexture(32, 32, source.depth, source.format, 0);
                //RT_0.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
                //RT_0.enableRandomWrite = true;
                RT_0.volumeDepth = 3;

                RT_0.enableRandomWrite = true;
                RT_0.Create();

                //Graphics.CopyTexture(source, RT_0);
                Graphics.Blit(source, RT_0);

                RT = RT_0;


                //Debug.Log("Refresh, width " + RT_0.width + " et height " + RT_0.height);
                Debug.Log("Refresh, width " + RT.width + " et height " + RT.height);


                int w = source.width;//Screen.currentResolution.width;
                int h = source.height;//Screen.currentResolution.height;

                Debug.Log("W est de "+w+" et h est de "+h);

                //ca c'est pour redim de la texture
                float ratio = 16.0f / 9.0f;
                float n_w = 128.0f;
                float n_h = n_w / ratio;

                //a mettre de base
                /*n_w = w;
                n_h = h;*/
                n_w = RT.width;
                n_h = RT.height;


                /*Texture2D tex = new Texture2D(source.width, source.height, TextureFormat.ARGB32, true);
                tex.minimumMipmapLevel = 1;
                RenderTexture.active = source;
                tex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                tex.Apply();
                

                //tex.Resize((int)n_w, (int)n_h);*/



                Debug.Log("Width " + n_w + " et Height " + n_h);//+ " Nb de level "+tex.mipmapCount);

                shader.SetFloat("width", w);
                shader.SetFloat("height", h);

                //double max = 2097152;


                //pour tab
                int nb_element = 2;//w * h;

                float mipLevel = 1;

                uint[] values = new uint[nb_element];
                values[0] = 0;

                values[1] = 0;

                //taille d'un element pareil a cuda ou ogl
                ComputeBuffer cb = new ComputeBuffer(nb_element, sizeof(uint));
                cb.SetData(values);


                n_w /= mipLevel;
                n_h /= mipLevel;

                //pour RT
                shader.SetFloat("width", n_w);
                shader.SetFloat("height", n_h);

                Debug.Log("Render " + n_w + " et " + n_h);

                //qd pas trouvé erreur compilation
                int indexKernel = shader.FindKernel("CSMain");

                shader.SetBuffer(indexKernel, "res", cb);

                //exemple metter une texture dans shader
                //de base
                //shader.SetTexture(indexKernel, "Result", source);
                //shader.SetTexture(indexKernel, "Result", RT_0,(int)(mipLevel-1));
                shader.SetTexture(indexKernel, "Result", RT, (int)(mipLevel - 1));
                
                //avec redim
                //shader.SetTexture(indexKernel, "Result", tex);

                //comme en cuda
                //partage en 16 zones

                //n_w = source.width;
                //n_h = source.height;


                int x, y, z;
                /*x = 64;
                y = 32;//avec dépassement ici donc plusieurs tour par thread qd nécessaire
                z = 1;*/

                float reducer = 32.0f;

                float tmp = n_w / reducer;
                //pour tronquer à l'entier le plus haut chiffre quand reste non null
                x = (int)( ((int)(tmp)) != tmp ? tmp+1 : tmp ); //60;
                tmp = n_h / reducer;
                y = (int)(((int)(tmp)) != tmp ? tmp+1 : tmp); //34;
                z = 1;

                Debug.Log("x est de " + x + " y est de " + y);

                shader.Dispatch(indexKernel, x, y, z);

                cb.GetData(values);
                cb.Release();

                double total = values[0];
                total /= 100.0f;
                total /= (n_w * n_h);

                if (total > 0.5f)
                {
                    mat.SetFloat("type", 1);
                }

                Debug.Log("Values est de " + total);
                Debug.Log("Cmpt est de " + values[1]);
                time_actual = Time.realtimeSinceStartup;
                
                //before create
                //source.useMipMap = false;
            }





                

            //Graphics.Blit(source, destination);

            //mat = new Material(shader);

            //float total = 0.0f;
            //Shader.SetGlobalFloat("res",total);

            //float tmp = 0;



            //ComputeShader 

            //comme cuda
            //ComputeBuffer tableau = new ComputeBuffer(1, sizeof(float));

            //mat.SetBuffer("tab",tableau);

            //changeShader("Contour");
            changeShader("PostProcessV1");
            //mat.SetFloat("width", Screen.currentResolution.width);
            //mat.SetFloat("height", Screen.currentResolution.height);
            //mat.SetFloat("width", source.width);
            //mat.SetFloat("height", source.height);
            mat.SetFloat("width", coef_blur);
            mat.SetFloat("height", coef_blur);
            mat.SetFloat("life", pc.vie);
            mat.SetFloat("_CoefBlur", coef_intensity);
            mat.SetFloat("_Radius", radius);
            mat.SetVector("_OffsetColor",new Vector4(offSetColor.x, offSetColor.y, offSetColor.z,1.0f));
            mat.SetInt("_StateBlur",(state_blur ? 1 : 0));
            mat.SetInt("_StateChromatique", (state_chromatique ? 1 : 0));
            mat.SetInt("_VignettePleine", (state_vignette_pleine ? 1 : 0));
            mat.SetFloat("_LerpEffect", lerp_effet);


            Graphics.Blit(source, destination,mat);

            /*if (Time.realtimeSinceStartup - time_actual > time_refresh)
            {
                Texture2D tex = new Texture2D(source.width, source.height, TextureFormat.RGB24, false);
                Graphics.CopyTexture(source, tex);
                //RenderTexture.active = source;
                //tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                //tex.Apply();
                tex.Resize((int)n_w, (int)n_h);
                //Debug.Log("Width " + w + " Height " + h+ " New Height " +n_h);
                float total = 0;
                //if (it == 0)
                //{
                for (int j = 0; j < tex.height; j++)
                {
                    for (int i = 0; i < tex.width; i++)
                    {
                        Color col = tex.GetPixel(i, j);
                        float tmp = (col.r + col.g + col.b) / 3.0f;
                        Debug.Log("Tmp est de " + tmp);
                        total += tmp;
                    }
                }
                //}

                //Debug.Log("Total est de " + total);
                float type;
                float res = total / (tex.height * tex.width);
                Debug.Log("Resultat est de " + res);
                time_actual = Time.realtimeSinceStartup;
            }*/



            /*if ((total /res) > 0.8f)
            {
                mat.SetFloat("type", 1);
            }
            else
            {
                mat.SetFloat("type", 0);
            }*/
            //mat.SetFloat("type", 0);
            //mat.SetFloat("time", Mathf.Sin(Time.realtimeSinceStartup));
            //Debug.Log("Total est de " + total);

            //mat.SetFloat("total", 0);
            //float[] data = new float[1];

            /*Graphics.Blit(source, destination, mat);
            tmp = mat.GetFloat("time");
            Debug.Log("Time est de " + tmp);*/
            //recup
            //tableau.GetData(data);

            //Debug.Log("Total est de " + data[0]);

            //total = Shader.GetGlobalFloat("res");//mat.GetFloat("total");
            //Debug.Log("Total est de "+total);
            //it++;
        }
    }
    private void OnPostRender()
    {
        //Camera.main.targetTexture = null;
        //Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), RT);
    }

    public void Update()
    {
        Matrix4x4 shear = Matrix4x4.identity;
        //shear.SetRow(1,new Vector4(0,1,2,0));
        //shear.SetRow(0, new Vector4(2, 0, 0, 0));
        //shear.SetRow(0, new Vector4(0, 2, 0, 0));
        shear.SetRow(2, new Vector4(offsetX, offsetY, 1, 0));
        //shear.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, -1.0f));
        cam.projectionMatrix = projection_base * shear;


        //vie
        


        //default 0.01 et 1000
        //n near plan
        //f far plan
        //default -1 1
        //t top et b bottom
        //r right et l left

        //Debug.Log("Affichage " + cam.nearClipPlane + " " + cam.farClipPlane);

        float angle_horizontal = cam.fieldOfView;

        float near, far, left, right, bottom, top;
        //near = 0.01f;
        //far = 1000.0f;
        near = cam.nearClipPlane;
        far = cam.farClipPlane;
        //left = bottom = -1.0f;
        //right = top = 1.0f;
        right = -Mathf.Cos(angle_horizontal / 2.0f) * near;
        left = Mathf.Cos(angle_horizontal / 2.0f) * near;
        /*right = near / Mathf.Cos(-angle_horizontal / 2.0f);
        right = Mathf.Cos(-angle_horizontal) * right;
        left = near / Mathf.Cos(angle_horizontal / 2.0f);
        right = Mathf.Cos(angle_horizontal) * left;*/

        float angle_vertical = angle_horizontal / cam.aspect; //angle_horizontal / (cam.rect.width / cam.rect.height);
        bottom = Mathf.Sin(-angle_vertical) * near;
        top = Mathf.Sin(angle_vertical) * near;
        /*bottom = near / Mathf.Cos(-angle_vertical / 2.0f);
        bottom = Mathf.Sin(-angle_vertical) * bottom;
        top = near / Mathf.Cos(angle_vertical / 2.0f);
        top = Mathf.Sin(angle_vertical) * top;*/

        /*Debug.Log("Near "+near+" Far "+far+" aspect "+cam.aspect+" angle Vertical "+angle_vertical+" angle Horizontal "+angle_horizontal);
        Debug.Log("Left " + left + " Right " + right);
        Debug.Log("Bottom " + bottom + " Top " + top);*/


        //right += (offsetX >= 0.0f ? offsetX : 0.0f);
        //left -= (offsetX < 0.0f ? offsetX : 0.0f);
        
        /*right += offsetX;
        left -= offsetX;

        bottom -= offsetY;
        top += offsetY;*/

        //bottom -= (offsetY < 0.0f ? offsetY : 0.0f);
        //top += (offsetY >= 0.0f ? offsetY : 0.0f);

        Matrix4x4 mat = new Matrix4x4();
        //2n/r-l 0 r+l/r-l 0
        mat.SetRow(0,new Vector4((2*near)/(right-left),0,(right+left)/(right- left),0));
        //0 2n/t-b t+b/t-b 0
        mat.SetRow(1, new Vector4(0,(2*near)/(top-bottom),(top+bottom)/(top - bottom),0));
        //0 0 -f+n/f-n -2fn/f-n
        mat.SetRow(2, new Vector4(0,0,-(far+near)/(far-near),-(2*far*near)/(far-near)));
        //0 0 -1 0
        mat.SetRow(3, new Vector4(0,0,-1.0f,0));


        //Debug.Log("Mat " + mat);
        cam.projectionMatrix = projection_base;// * shear;
        //Debug.Log("Camera " + cam.projectionMatrix);
        //mat.SetFloat("time", Mathf.Sin(Time.realtimeSinceStartup*40.0f));
    }
}
