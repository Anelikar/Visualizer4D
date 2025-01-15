using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A constructor for the Intro UI data set
/// </summary>
public class IntroUIDataBuilder : UIDataBuilderBase
{
    // This script is pretty much hardcoded to construct the very specific Intro UI data set
    // All the nessessary references and values for constructing the data are meant to be set up in the inspector
    // Tooltips are absent because all references are expecting a very particular component
    // See the InfoCanvas gameobject in the SampleScene for the actual linked data

    // This data set introduces the player to the demo, explains basic controls,
    // and offers a brief explanation of the 4D visualization

    [Header("Sprites")]
    [SerializeField]
    Sprite m_thumbstickSprite = null;
    [SerializeField]
    Sprite m_3dGizmosSprite = null;
    [SerializeField]
    Sprite m_4dGizmosSprite = null;

    [Header("Action linking")]
    [SerializeField]
    InfoUIControls m_infoUIControls;
    [SerializeField]
    GameObject m_visualiser4D;
    [SerializeField]
    QuickSound m_visualiserSound;
    [SerializeField]
    Transform4D m_transform4D;
    [SerializeField]
    ConstantRotation4D m_constRotation;
    [SerializeField]
    InfoUI m_infoUI;
    [SerializeField]
    UIDataBuilderBase m_nextData;

    UnityAction m_showTeserractAction = null;
    UnityAction m_projectTeserractAction = null;
    UnityAction m_spinTeserractAction = null;
    UnityAction m_endIntroAction = null;
    UnityAction m_buildNextDataAction = null;

    [Header("Shape animation setup")]
    [SerializeField]
    Transform4D.Euler4 m_startingConstantRotation = Transform4D.Euler4.Zero;
    [SerializeField]
    Vector4 m_startingScale = new Vector4(0.5f, 0.5f, 0.5f, 1);
    [SerializeField]
    float m_animationTime = 1f;

    void Start() {
        BuildData();
    }

    /// <summary>
    /// Initializes fields used in constructing data blocks and prepares the tesseract for the projection animation
    /// </summary>
    void Setup() {
        // Saving relevant current tesseract state
        //float projLightDst = m_transform4D.ProjectionLightDistance;
        Transform4D.Euler4 constRot = m_constRotation.ConstantRotation;

        // Preparing the tesseract for the projection animation
        m_transform4D.ProjectionLightDistance = 0;
        m_transform4D.Scale = m_startingScale;
        m_constRotation.ConstantRotation = m_startingConstantRotation;
        m_visualiser4D.SetActive(false);

        // Setting up actions
        m_endIntroAction = m_infoUIControls.StartSwapUI;
        m_showTeserractAction = () => { 
            m_visualiser4D.SetActive(true); 
            m_visualiserSound.PlayQuickSound(); 
        };
        m_projectTeserractAction = () => { StartCoroutine(TeserractProjectionAnimation(m_animationTime)); };
        m_spinTeserractAction = () => { m_constRotation.ConstantRotation = constRot; };
        m_buildNextDataAction = () => { 
            m_infoUI.ClearData();
            m_nextData.BuildData(); 
            m_infoUI.DisplayNext(); 
        };
    }

    public override void BuildData() {
        // Initializing data fields and tesseract state
        Setup();

        // Adding data blocks in sets. All blocks that have addNext set to true will be displayed simultaneously.
        m_data.AddClear(true);
        m_data.AddTextLoc("Intro1", true);
        m_data.AddTextLoc("Intro2", true);
        m_data.AddSprite(m_thumbstickSprite, true);
        m_data.AddTextLoc("Intro2_1");

        m_data.AddClear(true);
        m_data.AddTextLoc("Intro3", true);
        m_data.AddTextLoc("Intro4", true);
        m_data.AddSprite(m_3dGizmosSprite);
        m_data.AddTextLoc("Intro5");

        m_data.AddClear(true);
        m_data.AddText("", true);
        m_data.AddTextLoc("Intro6", true);
        m_data.AddTextLoc("Intro7");

        m_data.AddClear(true);
        m_data.AddTextLoc("Intro9", true);
        m_data.AddTextLoc("Intro10", true);
        m_data.AddSprite(m_4dGizmosSprite);

        m_data.AddClear(true);
        m_data.AddTextLoc("Intro12", true);
        m_data.AddAction(m_showTeserractAction);
        m_data.AddTextLoc("Intro13", true);
        m_data.AddAction(m_projectTeserractAction);

        m_data.AddClear(true);
        m_data.AddTextLoc("Intro14", true);
        m_data.AddTextLoc("Intro15", true);
        m_data.AddAction(m_spinTeserractAction);

        m_data.AddClear(true);
        m_data.AddAction(m_endIntroAction, true);
        m_data.AddAction(m_buildNextDataAction);
    }

    /// <summary>
    /// A coroutine method that animates stretching the cube into 4D to become a tesseract.
    /// Hardcoded to work with 0 .. 1.5 projection light distance and final scale of [1, 1, 1, 1].
    /// </summary>
    IEnumerator TeserractProjectionAnimation(float time) {
        float elapsedTime = 0.0f;

        // w is set to a very low value because the scale of 0 doesn't make sense
        // projection light distance of 1 casts a 3D shade of a normal cube
        m_transform4D.Scale.x = 1;
        m_transform4D.Scale.y = 1;
        m_transform4D.Scale.z = 1;
        m_transform4D.Scale.w = 0.0001f;
        m_transform4D.ProjectionLightDistance = 1;

        while (elapsedTime <= time) {
            // Tesseract is scaled on w to provide an illusion of projecting inwards 
            // despite the inner cube actually being closer to the observer on the w axis in the "shadowcast" projection
            m_transform4D.Scale.w = Mathf.Lerp(0.0001f, 1, elapsedTime / time);

            m_transform4D.ProjectionLightDistance = Mathf.Lerp(1, 1.5f, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
