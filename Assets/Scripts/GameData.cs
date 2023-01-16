using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyData
{
    public EnemyData(Vector3 position, Vector3 rotation, EnemyStateController.EnemyState state, Interrupt interrupt, int health, int stamina, float knockoutTime, int nextPoint, Vector3[] patrolPoints)
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

    public Vector3 position;
    public Vector3 rotation;
    public EnemyStateController.EnemyState state;
    public Interrupt interrupt;
    public int health;
    public int stamina;
    public float knockoutTime;
    public int nextPoint;
    public Vector3[] patrolPoints;
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
    public PlayerData(Vector3 position, Vector3 rotation, PlayerController.Mode mode, bool isCrouching, int health, int[] ammo, int[] ammoReserve, int[] inventory, int currentWeapon)
    {
        this.position = position;
        this.rotation = rotation;
        this.mode = mode;
        this.isCrouching = isCrouching;
        this.health = health;
        this.ammo = ammo;
        this.ammoReserve = ammoReserve;
        this.inventory = inventory;
        this.currentWeapon = currentWeapon;
    }

    public Vector3 position;
    public Vector3 rotation;
    public PlayerController.Mode mode;
    public bool isCrouching;

    public int health;
    public int[] ammo;
    public int[] ammoReserve;
    public int[] inventory;
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
