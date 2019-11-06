using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICommonState
{
    void Init(object parameters);
    void UnInit();
    string GetName();
    void OnEnter(string previousState, object parameters);
    void OnExit(string nextState);
}
