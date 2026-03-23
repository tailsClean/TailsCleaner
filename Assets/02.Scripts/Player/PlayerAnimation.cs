using UnityEngine;

public class PlayerAnimation
{
    private Animator _animator;


    public float CurrentAniTime => _animator.GetCurrentAnimatorStateInfo(0).length;

    public const string Idle = "Idle";
    public const string Move = "Move";
    public const string Hit = "Hit";
    public const string Dead = "Dead";

    public const string Attack = "Attack";
    public const string SkillCount = "AttackCount";
    public const string Skill1 = "Skill1";
    public const string Skill2 = "Skill2";
    public const string Skill3 = "Skill3";
    public const string Sweep = "Sweep";

    public PlayerAnimation(Animator animator)
    {
        _animator = animator;
    }

    public void PlayAni(string aniName)
    {
        switch(aniName)
        {
            case Idle:
                _animator.SetTrigger(Idle);
                _animator.SetFloat(Move, 0f);
                break;

            case Move:
                _animator.SetFloat(Move, 1f);
                break;

            case Sweep:
                _animator.SetTrigger(Sweep);
                break;

            case Hit:
                _animator.SetTrigger(Hit);
                break;

            case Dead:
                _animator.SetTrigger(Dead);
                break;

            case Skill1:
                OnSkill(0);
                break;

            case Skill2:
                OnSkill(1);
                break;

            case Skill3:
                OnSkill(2);
                break;
        }
    }

    private void OnSkill(int skillNum)
    {
        _animator.SetInteger(Attack, skillNum);
        _animator.SetTrigger(SkillCount);
    }
}

public interface IPlayerAni
{
    void PlayAni(string aniName);
}

