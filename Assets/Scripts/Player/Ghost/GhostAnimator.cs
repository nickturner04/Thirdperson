using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAnimator : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    public GhostController controller;
    [SerializeField] private GameObject[] hitboxes;
    [SerializeField] private Transform hitbox1;
    [SerializeField] private Transform hitbox2;
    [SerializeField] private Transform hitbox3;

    [SerializeField] private int punchDamage;
    [SerializeField] private int kickDamage;

    private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void StartAttack()
    {
        controller.SetOccupied(true);
    }

    public void EndAttack()
    {
        foreach (var item in hitboxes)
        {
            item.SetActive(false);
        }
        controller.SetOccupied(false);
    }

    public void TakedownAttack()
    {
        audioSource.PlayOneShot(hitSound);
        controller.targetEnemy.TakeStaminaDamage(100f);
    }

    public void EndTakedown()
    {
        controller.occupied = false;
        controller.takedown = false;
        controller.gAttach = controller.gAttachAttack;
    }

    public void SpawnPunch1()
    {
        hitboxes[0].SetActive(true);
        
    }
    public void SpawnPunch2()
    {
        hitboxes[1].SetActive(true);
    }
    public void SpawnKick()
    {
        hitboxes[2].SetActive(true);
    }
}
