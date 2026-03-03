using UnityEngine;


public abstract class SpendableItem : MonoBehaviour
{
    [field: SerializeField] public int ID {  get; private set; }
    [field: SerializeField] public TYPE type {  get; private set; }
    [field: SerializeField] public int MaxStack { get; private set; } = 1;
    [field: SerializeField] public int Grade {  get; private set; }
    [field: SerializeField] public string Name {  get; private set; }

    public abstract void Use();

    public enum TYPE { }
}
