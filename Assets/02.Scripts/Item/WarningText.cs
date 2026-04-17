using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;


public static class WarningText
{
    public static async void ShowText(string warnigText)
    {
        var handle = Addressables.InstantiateAsync("WarningText");
        Debug.Log(handle);
        await handle.Task;

        var parent = ItemManager.Instance;
        var obj = handle.Result;

        if (obj == null)
            return;

        obj.transform.SetParent(parent.gameObject.transform, false);
        obj.transform.SetAsLastSibling();

        parent.StartCoroutine(SetTextAndMove(obj, warnigText));
    }

    private static IEnumerator SetTextAndMove(GameObject gameObject, string warnigText)
    {
        if(Time.deltaTime == 0)
        {
            Time.timeScale = 1f;
        }
        TextMeshProUGUI text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        text.text = warnigText;
        text.color = Color.red;

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        float timer = 0f;
        while(timer < 0.625f)
        {
            timer += Time.deltaTime;
            rectTransform.Translate(new Vector2(0, Time.deltaTime * 45f));
            yield return null;
        }

        Addressables.ReleaseInstance(gameObject);
        GameObject.Destroy(gameObject);
    }
}