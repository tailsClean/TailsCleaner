using UnityEngine;

[CreateAssetMenu(fileName = "PassiveData", menuName = "Skill/PassiveData")]
public class PassiveSkillData : ScriptableObject
{
    [Header("기본 정보")]
    public int PassiveId;
    public string PassiveName;
    public int SubTag;

    [TextArea(2, 4)]
    public string Desc;

    [Header("패시브 모디파이어")]
    [SerializeReference] public PassiveModifier Modifier;

    [Header("패시브 모디파이어 설정 (수동)")]
    [SerializeReference] public PassiveModifierConfig Config;
}
