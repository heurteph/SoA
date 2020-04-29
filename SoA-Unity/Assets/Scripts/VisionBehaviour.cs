using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionBehaviour : MonoBehaviour
{
    [Space]
    [Header("Character Camera")]

    [SerializeField]
    [Tooltip("The offset to the head")]
    private Vector3 cameraOffset = Vector3.zero;
    // = new Vector(0, 0.3f, 0.2f); // Inside Offset
    // = new Vector(0, 0.4f, 0.45f); // Outside Offset

    [SerializeField]
    [Tooltip("The angle to the head")]
    private Vector3 cameraAngle = Vector3.zero;
    // = new Vector(0, 0, 0);    // Inside Angle
    // = new Vector(30, 180, 0); // Outside Angle

    [SerializeField]
    [Tooltip("The texture to render the view")]
    private RenderTexture targetTexture;

    private Camera visionCamera;
    private Transform headMesh;
    private GameObject head;
    //[SerializeField]
    //private GameObject characterMesh;

    [Space]
    [Header("Brightness Detector")]

    [SerializeField]
    [Range(0,1)]
    private float normalPercentageThreshold = 0.5f;

    [SerializeField]
    [Range(0, 1)]
    private float protectedPercentageThreshold = 0.6f;

    private float percentageThreshold;
    public float PercentageThreshold { get { return percentageThreshold; } }

    [SerializeField]
    [Range(0, 255)]
    private float brightnessThreshold = 200f;

    [SerializeField]
    [Range(0, 100)]
    private float brightnessDamage = 10f;

    [SerializeField]
    private EnergyBehaviour energyBehaviour;

    private delegate void BrightnessThresholdHandler(float b);
    private event BrightnessThresholdHandler brightnessThresholdEvent;

    [SerializeField]
    private DebuggerBehaviour debuggerBehaviour;

    public delegate void GrayscaleChangedHandler(GameObject sender, Texture2D t, float p);
    public event GrayscaleChangedHandler grayScaleChangedEvent;

    void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        percentageThreshold = normalPercentageThreshold;
        brightnessThresholdEvent += energyBehaviour.DecreaseEnergy;
        grayScaleChangedEvent += debuggerBehaviour.DisplayVision;

        InjectCameraToFBX(); // create the camera inside the script
    }

    // Update is called once per frame
    void Update()
    {
        //head.transform.position = headMesh.transform.position + cameraOffset;
        //head.transform.rotation = Quaternion.Euler(cameraAngle) * headMesh.transform.rotation;

        Texture2D t2D = RenderTexturetoTexture2D(visionCamera.targetTexture);
        ComputeBrightnessAverage(t2D);
    }

    Texture2D RenderTexturetoTexture2D (RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D t2D = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        t2D.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        t2D.Apply();

        return t2D;
    }

    /* Many methods to compute brightness level of a frame */

    void ComputeBrightnessAverage (Texture2D t2D)
    {
        float sum = 0;

        for (int i=0; i < t2D.width; i++)
        {
            for (int j=0; j< t2D.height; j++)
            {
                if (t2D.GetPixel(i, j).grayscale*255 >= brightnessThreshold)
                {
                    sum++;
                    t2D.SetPixel(i, j, new Color(1, 1, 1));
                }
                else
                {
                    t2D.SetPixel(i, j, new Color(0, 0, 0));
                }
              //  float grayscale = t2D.GetPixel(i, j).grayscale; // monstre 2
              //  t2D.SetPixel(i, j, new Color(grayscale,grayscale,grayscale)); // monstre
            }
        }

        if (sum >= percentageThreshold * t2D.width * t2D.height)
        {
            brightnessThresholdEvent(brightnessDamage);
        }

        t2D.Apply();
        grayScaleChangedEvent(this.gameObject, t2D, sum / (t2D.width * t2D.height));
    }

    public void CoverEyes()
    {
        percentageThreshold = protectedPercentageThreshold;
    }

    public void UncoverEyes()
    {
        percentageThreshold = normalPercentageThreshold;
    }

    public void InjectCameraToFBX()
    {
        headMesh = GameObject.FindWithTag("Head").transform;
        if(headMesh == null)
        {
            throw new System.Exception("Camera Character Error : No head element found");
        }
        head = new GameObject();
        head.transform.position = headMesh.transform.position + cameraOffset;
        head.transform.rotation = Quaternion.Euler(cameraAngle) * headMesh.transform.rotation;
        
        visionCamera = head.AddComponent<Camera>();
        visionCamera.nearClipPlane = 0.1f;
        visionCamera.targetTexture = targetTexture;
        
        head.transform.SetParent(headMesh);
    }


} // FINISH
