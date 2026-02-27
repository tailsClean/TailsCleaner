using UnityEngine;
using MonsterEnum;


public class patternTable : ScriptableObject
{
    public int patternId;

    public PatternType patternType;

    public float castTime;
    public float duration;
    public float range;
    public float powerScale;
    public float cooldown;
    public float patternMultiply;
    public float damageMultiply;
    public float detectRange;
    public float triggerValue;
    public float projectileSpeed;
    public float projectileSize;
    public int projectileCount;
    public float projectileRadius;
    public float fireInterval;
    public float lifeTime;
    public PierceType pierceType;
    public bool follow;
    public float arcHeight;
    public float areaRadius;
    public float areaDamageInterval;
    public float rushSpeed;
    public float explodeRange;

    public int summonMonsterId;
    public int summonCount;
    

    

    public DebuffType debuffType;

    public float debuffValue;
}
