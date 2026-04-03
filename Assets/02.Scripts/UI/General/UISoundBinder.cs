using UnityEngine;
using UnityEngine.UI;

public class UISoundBinder : MonoBehaviour
{
    private void Start()
    {
        // 버튼 달린 모든 자식들
        Button[] allButtons = GetComponentsInChildren<Button>(true);

        foreach (Button button in allButtons)
        {
            // 사운드 재생 스크립트 붙어있는지 확인
            if (button.gameObject.GetComponent<AutoButtonSound>() == null)
            {
                // 버튼 오브젝트에 사운드 재생 컴포넌트 달기
                button.gameObject.AddComponent<AutoButtonSound>();
            }
        }
    }
}
