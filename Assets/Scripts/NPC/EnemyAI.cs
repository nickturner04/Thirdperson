using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Sight),typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    
}


public enum InterruptType
{
    Ignore,Look,Walk,Run
}
