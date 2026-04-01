using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UISoundBinder : MonoBehaviour
{
    private IEnumerator Start()
    {
        // 혹시 몰라서 한프레임 미룸
        yield return null;

        // 긁어모으기
        Button[] allButtons = GetComponentsInChildren<Button>(true);

        // 모든 버튼에 클릭 사운드
        foreach (Button button in allButtons)
        {
            button.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlayUISFX(UISFXName.Click);
            });
        }
    }
}
