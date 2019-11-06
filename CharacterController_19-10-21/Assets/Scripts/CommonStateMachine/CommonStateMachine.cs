using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonStateMachine
{

    public ICommonState curState
    {
        get
        {
            return _curState;
        }
    }

    public string curStateName
    {
        get
        {
            if (_curState != null)
            {
                return string.Empty;
            }
            else
            {
                return _curState.GetName();
            }
        }
    }

    public Dictionary<string, ICommonState> allState
    {
        get
        {
            return _stateDict;
        }
    }

    private ICommonState _curState = null;

    private Dictionary<string, ICommonState> _stateDict = new Dictionary<string, ICommonState>();


    public void Init()
    {

    }


    public void UnInit()
    {
        UnRegisterALLState();

        _curState = null;
    }


    public bool RegisterState(ICommonState IState,object parameters)
    {
        string stateName = IState.GetName();

        if (allState.ContainsKey(stateName))
        {
            return false;
        }

        IState.Init(parameters);

        allState.Add(stateName,IState);

        return true;
    }


    public bool UnRegisterState(string stateName)
    {
        if (!allState.ContainsKey(stateName))
        {
            return false;
        }

        allState[stateName].UnInit();

        return allState.Remove(stateName);
    }


    public void UnRegisterALLState()
    {
        if (_curState != null)
        {
            _curState.OnExit("Clear");
        }

        foreach (ICommonState state in allState.Values)
        {
            state.UnInit();
        }

        allState.Clear();
    }


    public int ChangeState(string targetStateName,object parameters)
    {
        if (!allState.ContainsKey(targetStateName))
        {
            return 1;
        }

        string curStateName = _curState != null ? _curState.GetName() : string.Empty;

        if (curStateName == targetStateName)
        {
            return 2;
        }

        if (_curState != null)
        {
            _curState.OnExit(targetStateName);
        }

        allState[targetStateName].OnEnter(curStateName, parameters);

        _curState = allState[targetStateName];

        return 0;
    }


    public ICommonState GetState(string key)
    {
        ICommonState ret = null;
        allState.TryGetValue(key, out ret);
        return ret;
    }

}
