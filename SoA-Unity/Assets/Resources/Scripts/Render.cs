using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct StructTexture
{
    public Texture2D tex;
}

public class Render : MonoBehaviour
{
    public bool shader_actif = false;
    private Material mat;
    public string shader_name = "NoVision";
    int it = 0;

    float time_actual = 0.0f;
    float time_refresh = 0.25f;

    // Start is called before the first frame update
    void Awake()
    {
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
                ComputeShader shader = Resources.Load<ComputeShader>("Shaders/Test");

                Debug.Log("Depth est de " + source.format);

                /*RenderTexture RT_0 = new RenderTexture(source.width, source.height, source.depth, source.format, RenderTextureReadWrite.Default);
                RT_0.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
                RT_0.enableRandomWrite = true;
                RT_0.volumeDepth = 3;
                RT_0.Create();*/



                RenderTexture RT_0 = new RenderTexture(source);
                RT_0.enableRandomWrite = true;

                int w = Screen.currentResolution.width;
                int h = Screen.currentResolution.height;
                float ratio = 16.0f / 9.0f;
                float n_w = 1024.0f;
                float n_h = n_w / ratio;

                /*Texture2D tex = new Texture2D(source.width, source.height, TextureFormat.ARGB32, false);
                RenderTexture.active = source;
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                tex.Apply();
                tex.Resize((int)n_w, (int)n_h);*/

                Debug.Log("Width " + w + " et Height " + h);

                shader.SetFloat("width", w);
                shader.SetFloat("height", h);

                //double max = 2097152;


                //pour tab
                int nb_element = 1;//w * h;



                uint[] values = new uint[nb_element];
                values[0] = 0;

                //taille d'un element pareil a cuda ou ogl
                ComputeBuffer cb = new ComputeBuffer(nb_element, sizeof(uint));
                cb.SetData(values);

                //qd pas trouvé erreur compilation
                int indexKernel = shader.FindKernel("CSMain");

                shader.SetBuffer(indexKernel, "res", cb);

                //exemple metter une texture dans shader
                shader.SetTexture(indexKernel, "Result", source);

                //comme en cuda
                //partage en 16 zones

                n_w = source.width;
                n_h = source.height;


                int x, y, z;
                /*x = 64;
                y = 32;//avec dépassement ici donc plusieurs tour par thread qd nécessaire
                z = 1;*/


                float tmp = n_w / 32;
                //pour tronquer à l'entier le plus haut chiffre quand reste non null
                x = (int)( ((int)(tmp)) < tmp ? tmp+1 : tmp ); //60;
                tmp = n_h / 32;
                y = (int)(((int)(tmp)) < tmp ? tmp+1 : tmp); //34;
                z = 1;

                Debug.Log("x est de " + x + " y est de " + y);

                shader.Dispatch(indexKernel, x, y, z);
                cb.GetData(values);
                cb.Release();

                double total = values[0];
                total /= 100.0f;
                total /= (w * h);

                if (total > 0.5f)
                {
                    mat.SetFloat("type", 1);
                }

                Debug.Log("Values est de " + total);
                time_actual = Time.realtimeSinceStartup;
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
            changeShader("VertexShader");
            mat.SetFloat("width", Screen.currentResolution.width);
            mat.SetFloat("height", Screen.currentResolution.height);

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

    public void Update()
    {
        //mat.SetFloat("time", Mathf.Sin(Time.realtimeSinceStartup*40.0f));
    }
}
