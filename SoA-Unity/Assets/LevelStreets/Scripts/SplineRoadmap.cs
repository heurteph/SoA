﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ROADSECTION { LONG, STRAIGHT, TURN, SHARP }

public class SplineRoadmap : MonoBehaviour
{
    [SerializeField]
    private ROADSECTION[] roadSections;
    public ROADSECTION[] RoadSections { get { return roadSections; } }
}
