﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyAbilities
{
    void PoisonAOE(GameObject player);
    void GroupAttack();
}