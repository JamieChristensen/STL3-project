using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace STL2.Events
{
    public interface IGameEventListener<T>
    {
        void OnEventRaised(T item);

    }
}
