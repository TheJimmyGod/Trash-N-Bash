using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackState : MonoBehaviour, IState
{
    private Enemy _agent;

    public BackState(Enemy agent) { this._agent = agent; }

    public void Enter()
    {
        throw new System.NotImplementedException();
    }

    public void Execute()
    {
        throw new System.NotImplementedException();
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }
}
