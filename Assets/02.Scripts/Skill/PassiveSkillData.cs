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

    [Header("패시브 수치 (수동)")]
    [SerializeReference] public PassiveModifierConfig Config;
}
