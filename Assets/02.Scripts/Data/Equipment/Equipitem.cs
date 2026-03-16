
using System;


public enum PART
{
    Weapon = 0, //무기
    Helmet = 1, //모자
    Cloak = 2, //망토
    Shoes = 3 //신발
}

[Serializable]
public class Equipitem
{
    public int id;
    public string desc;
    public PART part;
    public int group_id;
    public string name;
    public string script;
    public string sprite;
    public string effect;
    public string sound;
}
