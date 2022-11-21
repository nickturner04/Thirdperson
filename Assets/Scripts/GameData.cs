using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public struct EnemyData
{
    public EnemyData(V3Surrogate position, V3Surrogate rotation, EnemyStateController.EnemyState state, InterruptSurrogate interrupt, int health, int stamina, float knockoutTime, int nextPoint, V3Surrogate[] patrolPoints)
    {
        this.position = position;
        this.rotation = rotation;
        this.state = state;
        this.interrupt = interrupt;
        this.health = health;
        this.stamina = stamina;
        this.knockoutTime = knockoutTime;
        this.nextPoint = nextPoint;
        this.patrolPoints = patrolPoints;
    }

    public V3Surrogate position;
    public V3Surrogate rotation;
    public EnemyStateController.EnemyState state;
    public InterruptSurrogate interrupt;
    public int health;
    public int stamina;
    public float knockoutTime;
    public int nextPoint;
    public V3Surrogate[] patrolPoints;
}
[System.Serializable]
public struct RoomData
{
    public RoomData(EnemyData[] enemies, bool entered)
    {
        this.entered = entered;
        this.enemies = enemies;
    }
    public EnemyData[] enemies;
    public bool entered;
}
[System.Serializable]
public struct PlayerData
{
    public PlayerData(V3Surrogate position, V3Surrogate rotation, PlayerController.Mode mode, bool isCrouching, int health, int[] ammo, int[] ammoReserve, int currentWeapon)
    {
        this.position = position;
        this.rotation = rotation;
        this.mode = mode;
        this.isCrouching = isCrouching;
        this.health = health;
        this.ammo = ammo;
        this.ammoReserve = ammoReserve;
        this.currentWeapon = currentWeapon;
    }

    public V3Surrogate position;
    public V3Surrogate rotation;
    public PlayerController.Mode mode;
    public bool isCrouching;

    public int health;
    public int[] ammo;
    public int[] ammoReserve;
    public int currentWeapon;
}
[System.Serializable]
public struct SaveData
{
    public SaveData(PlayerData player, RoomData[] rooms, int room)
    {
        this.player = player;
        this.rooms = rooms;
        this.room = room;
    }
    public PlayerData player;
    public RoomData[] rooms;
    public int room;
}

public static class GameData
{
    public static int chosencharacter = 1;
    //public static SaveData defaultSave = new(new PlayerData(new(0,0,0),new(0,0,0),PlayerController.Mode.NORMAL,500,new int[]))
}
