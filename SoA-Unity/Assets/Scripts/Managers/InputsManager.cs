using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputsManager
{
    private Inputs inputs;
    public Inputs Inputs { get { return inputs; } }

    private static InputsManager instance = null;
    public static InputsManager Instance {
        get
        {
            if (instance == null)
            {
                instance = new InputsManager();
            }
            return instance;
        }
    }

    private InputsManager()
    {
        inputs = new Inputs();
    }
}
