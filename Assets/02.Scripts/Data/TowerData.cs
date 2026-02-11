using UnityEngine;

public class TowerData
{
    public int tower_id; //고유 id 4자리(5001 ~ 9999) 뒤 두자리는 몇 번째 탑인지 표시

    public string tower_name_key; //스트링 테이블과 연결되는 키값 

    public int need_stage_id; //해금 스테이지 id 5자리(50210 ~ 99999)

    public string tower_icon_resource; //타워 아이콘 이미지명

    public string bgm_resource; //타워 배경음악 리소스명 
}
