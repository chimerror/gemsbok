using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TitlesAnimatorBehaviour : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameManager.Instance.MoveToMenus();
    }
}
