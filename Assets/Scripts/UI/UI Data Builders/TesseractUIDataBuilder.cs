using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A constructor for the Tesseract UI data set
/// </summary>
public class TesseractUIDataBuilder : UIDataBuilderBase
{
    // This script is pretty much hardcoded to construct the very specific UI data set
    // All the nessessary references and values for constructing the data are meant to be set up in the inspector
    // Tooltips are absent because all references are expecting a very particular component
    // See the InfoCanvas gameobject in the SampleScene for the actual linked data

    // This data set shows how the tesseract is constructed and explains how to control it in the demo

    [Header("Sprites")]
    [SerializeField]
    Sprite m_squareToCubeSprite = null;

    [Header("Action linking")]
    [SerializeField]
    InfoUI m_infoUI;
    [SerializeField]
    UIDataBuilderBase m_nextData;

    UnityAction m_buildNextDataAction = null;

    void Setup() {
        // Setting up an action to switch to the next data set
        m_buildNextDataAction = () => { 
            m_infoUI.ClearData(); 
            m_nextData.BuildData(); 
            m_infoUI.DisplayNext(); 
        };
    }

    public override void BuildData() {
        Setup();

        // Adding data blocks in sets. All blocks that have addNext set to true will be displayed simultaneously.
        m_data.AddClear(true);
        m_data.AddTextLoc("Tesseract1", true);
        m_data.AddTextLoc("Tesseract2", true);
        m_data.AddTextLoc("Tesseract3", true);
        m_data.AddSprite(m_squareToCubeSprite);

        m_data.AddClear(true);
        m_data.AddText("", true);
        m_data.AddTextLoc("Tesseract4");

        m_data.AddClear(true);
        m_data.AddTextLoc("Tesseract5", true);
        m_data.AddTextLoc("Tesseract6", true);
        m_data.AddTextLoc("Tesseract7");

        m_data.AddClear(true);
        m_data.AddText("", true);
        m_data.AddTextLoc("Tesseract8");

        m_data.AddClear(true);
        m_data.AddTextLoc("Tesseract9", true);
        m_data.AddTextLoc("Tesseract10");

        m_data.AddClear(true);
        m_data.AddTextLoc("Tesseract11", true);
        m_data.AddTextLoc("Tesseract12", true);
        m_data.AddTextLoc("Tesseract13");

        m_data.AddClear(true);
        m_data.AddText("", true);
        m_data.AddTextLoc("Tesseract14");

        m_data.AddClear(true);
        m_data.AddTextLoc("Tesseract15", true);
        m_data.AddTextLoc("Tesseract16", true);
        m_data.AddTextLoc("Tesseract17");

        m_data.AddClear(true);
        m_data.AddAction(m_buildNextDataAction);
    }
}
