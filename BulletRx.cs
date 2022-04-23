using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class BulletRx : MonoBehaviour
{
    public float damage = 35f;
    [Space]
    public GameObject woodHole;
    public GameObject stoneHole;
    public GameObject metalHole;
    public GameObject sandHole;
    //Creamos un diccionario para las particulas de impacto
    private IDictionary<string, GameObject> holes = new Dictionary<string, GameObject>();

    void Start()
    {
        holes["Stone"] = stoneHole;
        holes["Wood"] = woodHole;
        holes["Metal"] = metalHole;
        holes["Sand"] = sandHole;

        IObservable<Collision> collisionEnterObservable = this.OnCollisionEnterAsObservable();

        //Este flujo controla todos los impactos
        collisionEnterObservable
        .Subscribe(Impact);

        //Este flujo controla solo los impactos contra barriles
        collisionEnterObservable
        .Where(IsBarrel)
        .Subscribe(DamageBarrel);
        
        //We also could merge both subscriptions into one using:
        /*
        collisionEnterObservable
        .Do(Impact)
        .Where(IsBarrel)
        .Subscribe(DamageBarrel);
        */

        Destroy(gameObject, 3f);
    }

    private void Impact(Collision collision)
    {
        Tags gameObjectTags = collision.gameObject.GetComponent<Tags>();
        ContactPoint contact = collision.contacts[0];

        string tag = gameObjectTags.getHoleTag();
        GameObject holeToInstantiate = tag != null ? holes[tag] : sandHole;
        
        Instantiate(
            holeToInstantiate, 
            contact.point, 
            Quaternion.LookRotation(contact.normal)
        );
        Destroy(gameObject);
    }

    private bool IsBarrel(Collision collision)
    {
        Tags gameObjectTags = collision.gameObject.GetComponent<Tags>();
        return gameObjectTags != null ? gameObjectTags.HasTag("Barrel") : false;
    }

    private void DamageBarrel(Collision collision)
    {
        BarrelRx barrel = collision.gameObject.GetComponent<BarrelRx>();
        barrel.AddDamage(damage);
    }
}
