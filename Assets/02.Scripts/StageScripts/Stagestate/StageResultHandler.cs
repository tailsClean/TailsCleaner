using UnityEngine;

// - 테스트용: F7=Clear, F8=Fail
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

        if (_events == null) return;
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
    }

    private void HandleStageResult(StageResult result, StageFailReason reason)
    {
        Debug.Log($"[StageResultUI] result={result}, reason={reason}");

        // 여기서 성공/실패 UI, 다음 버튼(로비/다음층/재도전) 처리
        // 보상 지급은 Success/FailState에서 처리됨
    }
}