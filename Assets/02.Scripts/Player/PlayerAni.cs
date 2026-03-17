using UnityEngine;

class PlayerAni
{
    private Animator _animator;

    public const string Idle = "Idle";
    public const string Move = "Move";
    public const string Hit = "Hit";
    public const string Dead = "Dead";

    public const string Attack = "Attack";
    public const string Skill1 = "Skill1";
    public const string Skill2 = "Skill2";
    public const string Skill3 = "Skill3";
    public const string Sweep = "Sweep";

    public PlayerAni(Animator animator)
    {
        _animator = animator;
    }

    public void PlayAni(string aniName)
    {
        switch(aniName)
        {
            case Idle:
                _animator.SetFloat(Move, 0f);
                break;

            case Move:
                _animator.SetFloat(Move, 1f);
                break;

            case Hit:
                _animator.SetTrigger(Hit);
                break;

            case Dead:
                _animator.SetTrigger(Dead);
                break;

            case Skill1:
                _animator.SetInteger(Attack, 0);
                break;

            case Skill2:
                _animator.SetInteger(Attack, 1);
                break;

            case Skill3:
                _animator.SetInteger(Attack, 2);
                break;

            case Sweep:
                _animator.SetInteger(Attack, 3);
                break;
        }
    }
}

public interface IPlayerAni
{
    void PlayAni(string aniName);
}

