using UnityEngine;

/// <summary>
/// 스테이지 결과(승/패)를 “정리하고 멈추는” 최소 처리기.
/// - FAIL/CLEAR 시 스폰 정지 + 몬스터 정리 + 스테이지 Pause
/// - 테스트용: F7=Clear, F8=Fail
/// </summary>
public class StageResultHandler : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool _enableDebugHotkeys = true;

    private StageEvents _events;
    private StageController _controller;

    public void Bind(StageEvents events, StageController controller)
    {
        Unbind();
        _events = events;
        _controller = controller;

        if (_events != null)
            _events.OnStageResult += HandleStageResult;
    }

    public void Unbind()
    {
        if (_events != null)
            _events.OnStageResult -= HandleStageResult;

        _events = null;
        _controller = null;
    }

    private void OnDestroy() => Unbind();

    private void Update()
    {
        if (!_enableDebugHotkeys) return;
        if (_controller == null) return;

        if (Input.GetKeyDown(KeyCode.F7))
            _controller.EndStage(StageResult.Clear, StageFailReason.기타);

        if (Input.GetKeyDown(KeyCode.F8))
            _controller.EndStage(StageResult.Fail, StageFailReason.기타);
    }

    private void HandleStageResult(StageResult result, StageFailReason reason)
    {
        Debug.Log($"[StageResultUI] result={result}, reason={reason}");

        // 여기서 성공/실패 UI, 다음 버튼(로비/다음층/재도전) 처리
        // 보상 지급은 Success/FailState에서 처리됨
    }
}