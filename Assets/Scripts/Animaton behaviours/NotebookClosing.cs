using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clears CanvasDrawing when exiting state
/// </summary>
public class NotebookClosing : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        NotebookAnimationConnection c = animator.GetComponent<NotebookAnimationConnection>();
        c.Drawing.Clear();
    }
}
