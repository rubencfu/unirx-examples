using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class PersuaderRx : MonoBehaviour
{
    private float speed = 2f;
    private Animator animator;

    public GameObject player;
    public float maxAmmo = 30f;
    private float ammo;
    private PlayerMovementRx playerScript;

    private bool isReloading = false;
    public float reloadTime = 4f;

    public float fireRate = 0.1f;
    public float bulletSpeed = 140f;
    private float nextFire = 0f;

    public GameObject round;
    public ParticleSystem muzzle;

    private RecoilRx recoilScript;
    public GameObject cameraRecoil;

    void Start()
    {
        ammo = maxAmmo;
        animator = GetComponent<Animator>();
        playerScript = player.GetComponent<PlayerMovementRx>();
        recoilScript = cameraRecoil.GetComponent<RecoilRx>();
        IObservable<Unit> update = this.UpdateAsObservable();

        update.Subscribe(_ => HandleAnimation());

        update
        .Where(_ => CanShoot())
        .Subscribe(_ => Fire("Fire"));

        update
        .Where(_ => CanShootAndIsLastBullet())
        .Subscribe(_ => FireLast());

        update
        .Where(_ => CanReload())
        .Subscribe(_ => StartCoroutine(Reload()));
    }

    private bool CanShoot() => Input.GetButton("Fire1") && ammo > 1 && Time.time > nextFire && !isReloading;

    private bool CanShootAndIsLastBullet() => Input.GetButton("Fire1") && ammo == 1 && Time.time > nextFire && !isReloading;

    private bool CanReload() => Input.GetButtonDown("Reload") && ammo < maxAmmo && !isReloading;

    IEnumerator Reload()
    {
        animator.SetBool("isReloading", true);
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        ammo = maxAmmo;
        isReloading = false;
        animator.SetBool("isReloading", false);
    }

    private void Fire(string animation)
    {
        AudioSource audio = GetComponent<AudioSource>();
        muzzle.Play();
        audio.PlayOneShot(audio.clip);
        GameObject spawnedRound = Instantiate(
            round,
            muzzle.transform.position,
            muzzle.transform.rotation
        );
        Rigidbody rigidBody = spawnedRound.GetComponent<Rigidbody>();
        rigidBody.velocity = spawnedRound.transform.forward * bulletSpeed;
        animator.Play(animation);
        recoilScript.RecoilFire();
        nextFire = Time.time + fireRate;
        ammo--;
    }

    private void FireLast()
    {
        Fire("FireLast");
        StartCoroutine(Reload());
    }

    private void HandleAnimation()
    {
        Vector3 velocity = 
            Vector3.forward 
            * (Input.GetAxis("Vertical") + Input.GetAxis("Horizontal")) 
            * speed;
        float animationVelocity = playerScript.isPlayerGrounded() ? velocity.magnitude : 0;
        animator.SetFloat("Speed", animationVelocity);
    }
}
