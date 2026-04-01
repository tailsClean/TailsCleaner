using UnityEngine;

public class PlayerActionState : StateMachineBehaviour
{
    [Header("재생할 사운드 이름")]
    public PlayerSFXName actionSfxName;
    
    [Header("상태 종료 시 사운드 강제 정지")]
    [Tooltip("체크하면 애니메이션이 끝날 때 소리 끔")]
    public bool stopOnExit = true;

    // 상태에 진입할 때 (애니메이션 시작)
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPlayerSFX(actionSfxName);
        }
    }

    // 상태를 빠져나갈 때 (애니메이션 종료)
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stopOnExit && SoundManager.Instance != null)
        {
            SoundManager.Instance.StopPlayerSFX();
        }
    }
}
