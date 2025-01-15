using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A constructor for the Tutorial UI data set
/// </summary>
public class TutorialUIDataBuilder : UIDataBuilderBase
{
    // This script is pretty much hardcoded to construct the very specific UI data set
    // All the nessessary references and values for constructing the data are meant to be set up in the inspector
    // Tooltips are absent because all references are expecting a very particular component
    // See the InfoCanvas gameobject in the SampleScene for the actual linked data

    // This data set explains controls and suggests to try different locomotion settings

    [Header("Sprites")]
    [SerializeField]
    Sprite m_thumbstickSprite = null;
    [SerializeField]
    Sprite m_rayButtonSprite = null;
    [SerializeField]
    Sprite m_triggerButtonSprite = null;
    [SerializeField]
    Sprite m_sideTriggerButtonSprite = null;

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
        m_data.AddTextLoc("Tutorial1", true);
        m_data.AddTextLoc("Tutorial2", true);
        m_data.AddTextLoc("Tutorial3", true);
        m_data.AddSprite(m_thumbstickSprite);

        m_data.AddClear(true);
        m_data.AddTextLoc("Tutorial7", true);
        m_data.AddTextLoc("Tutorial8", true);
        m_data.AddSprite(m_sideTriggerButtonSprite);

        m_data.AddClear(true);
        m_data.AddTextLoc("Tutorial8_1", true);
        m_data.AddTextLoc("Tutorial8_2");

        m_data.AddClear(true);
        m_data.AddTextLoc("Tutorial4", true);
        m_data.AddTextLoc("Tutorial5", true);
        m_data.AddTextLoc("Tutorial6");

        m_data.AddClear(true);
        m_data.AddTextLoc("Tutorial6_1", true);
        m_data.AddTextLoc("Tutorial6_2", true);
        m_data.AddTextLoc("Tutorial6_3", true);
        m_data.AddSprite(m_rayButtonSprite, true);
        m_data.AddSprite(m_triggerButtonSprite);

        m_data.AddClear(true);
        m_data.AddTextLoc("Tutorial9", true);
        m_data.AddTextLoc("Tutorial10");

        m_data.AddClear(true);
        m_data.AddAction(m_buildNextDataAction);
    }
}
