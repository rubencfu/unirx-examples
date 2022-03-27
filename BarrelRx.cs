using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class BarrelRx : MonoBehaviour
{
    private ReactiveProperty<float> health;

    public float initialHealth = 50f;

    public GameObject explosion;
    public GameObject explosionRadius;

    void Start()
    {
        health = new ReactiveProperty<float>(initialHealth);

        health
        .Where(hp => hp < 1)
        .Take(1)
        .Subscribe(_ => Explode())
        .AddTo(this);
    }

    public void Explode()
    {
        Instantiate(explosion, transform.position, transform.rotation);
        Instantiate(explosionRadius, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    public void AddDamage(float damage)
    {
        health.Value -= damage;
    }
}
