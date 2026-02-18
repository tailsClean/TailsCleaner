public class PassiveData
{
    public int Id;              // passive_skill_id     패시브 스킬 ID 42001
    public string Name;         // passive_name         패시브 이름 (매이크 라쿤 ~)
    public int SubTag;          // 대상 서브 태그        40101
    public string Desc;         // 설명

    public PassiveData(int id, string name, int subTag, string desc = "")
    {
        Id = id;
        Name = name;
        SubTag = subTag;
        Desc = desc;
    }
}
