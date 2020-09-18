using UnityEngine;
namespace STL2.Events
{
    [CreateAssetMenu(fileName = "New VoidType Event", menuName = "ScriptableObject/Event/Void Event")]
    public class VoidEvent : BaseGameEvent<VoidType>
    {
        public void Raise() => Raise(new VoidType());

    }

}