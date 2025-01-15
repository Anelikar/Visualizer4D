using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A constructor for the Lecture UI data set
/// </summary>
public class LectureUIDataBuilder : UIDataBuilderBase
{
    // This script is pretty much hardcoded to construct the very specific UI data set

    // This data set offers a deeper dive into 4D visualization and its practical appications

    void Setup() {
        
    }

    public override void BuildData() {
        // There is nothing to setup for this data set but the method call is kept for consistency
        Setup();

        // Adding data blocks in sets. All blocks that have addNext set to true will be displayed simultaneously.
        m_data.AddClear(true);
        m_data.AddText("", true);
        m_data.AddTextLoc("Lecture2", true);
        m_data.AddTextLoc("Lecture3");

        m_data.AddClear(true);
        m_data.AddTextLoc("Lecture4", true);
        m_data.AddTextLoc("Lecture5", true);
        m_data.AddTextLoc("Lecture6");

        m_data.AddClear(true);
        m_data.AddTextLoc("Lecture7", true);
        m_data.AddTextLoc("Lecture8", true);
        m_data.AddTextLoc("Lecture9");

        m_data.AddClear(true);
        m_data.AddTextLoc("Lecture10", true);
        m_data.AddTextLoc("Lecture11");

        m_data.AddClear(true);
        m_data.AddTextLoc("Lecture12", true);
        m_data.AddTextLoc("Lecture13");
        m_data.AddTextLoc("Lecture14");

        m_data.AddClear(true);
        m_data.AddTextLoc("Lecture15", true);
        m_data.AddTextLoc("Lecture16", true);
        m_data.AddTextLoc("Lecture17");

        m_data.AddClear(true);
        m_data.AddTextLoc("Lecture18", true);
        m_data.AddTextLoc("Lecture18_1", true);
        m_data.AddTextLoc("Lecture18_2");

        m_data.AddClear(true);
        m_data.AddTextLoc("Lecture19", true);
        m_data.AddTextLoc("Lecture20", true);
        m_data.AddTextLoc("Lecture21");

        m_data.AddClear(true);
        m_data.AddTextLoc("Lecture21_1", true);
        m_data.AddTextLoc("Lecture21_2", true);
        m_data.AddTextLoc("Lecture21_3");

        m_data.AddClear(true);
        m_data.AddTextLoc("Lecture22", true);
        m_data.AddTextLoc("Lecture24", true);
        m_data.AddTextLoc("Lecture25");

        m_data.AddClear(true);
    }
}
