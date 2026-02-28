using UnityEngine;
using UnityEngine.UI;


public abstract class PlayerEnhancement : MonoBehaviour
{
    [Header("공통 기본 정보")]
    public bool Isbase;
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public int GroupID { get; private set; }                // 해당 파츠 고유ID
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public string IconSprite { get; private set; }
    [field: SerializeField] public string IconClickEffect { get; private set; }
    [field: SerializeField] public string IconClickSound { get; private set; }

    [SerializeField] protected EnhancementEventChannelSO _onEquipEnhancement;


    //
    [field: SerializeField] public Sprite SpriteImage { get; private set; }
    protected Button _button;
    //

    #region 강화 데이터
    // 임시 강화
    [Header("강화 데이터")]
    public bool IsEnhance;
    [field: SerializeField] public int EnhanceID { get; private set; }
    [field: SerializeField] public int EnhanceGroupID { get; private set; }
    [field: SerializeField] public int EnhanceLevel { get; private set; } = 1;
    [field: SerializeField] public bool IsEnhanceMaxLevel { get; private set; }
    [field: SerializeField] public float EnhanceAddValue { get; private set; }
    [field: SerializeField] public int EnhanceCostGold { get; private set; }
    [field: SerializeField] public int EnhanceCostBluePrint { get; private set; }
    [field: SerializeField] public int EnhanceBluePrintID { get; private set; }

    // 강화 관련 정보와 메서드 저장 공간
    protected EnhanceData EquipEnhance { get; private set; }


    public void OnEnhance() => EnhanceLevel++;
    #endregion

    protected virtual void Awake()
    {
        EquipEnhance = new EnhanceData(this);



        //
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() => EquipEnhancement(this));
        //
    }


    // 장비(유물) 착용 메서드
    public virtual void EquipEnhancement(PlayerEnhancement enhancement) => 
        _onEquipEnhancement.OnStartEvent(enhancement);
    public void Remove() => Destroy(gameObject);









    #region 에디터 설정
#if UNITY_EDITOR
    private void OnValidate()
    {
        var image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogWarning($"{gameObject.name}에 Image컴포넌트가 없음");
            return;
        }
        if (SpriteImage != null)
            image.sprite = SpriteImage;
    }
#endif
    #endregion
}