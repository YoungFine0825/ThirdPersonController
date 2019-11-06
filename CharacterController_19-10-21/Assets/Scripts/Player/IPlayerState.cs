using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerState : ICommonState
{
    void OnUpdate();
    void OnLateUpdate();
    void OnDestroy();
}
