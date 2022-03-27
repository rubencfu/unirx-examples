
# UniRx Examples

Some Unity scripts made whit UniRx, feel free to use them in your projects!


![Logo](https://i.imgur.com/edlSj9x.png)


## 🚀 About Me
I'm a frontend developer caught by the videogame world and its development, this way I started to learn videogame development and try to teach others about the cool things I learn!


## Related

[UniRx Github Project](https://github.com/neuecc/UniRx)

[Microsoft Reactive Extensions Documentation](https://docs.microsoft.com/en-us/previous-versions/dotnet/reactive-extensions/hh212048(v=vs.103))

[UniRx Unity Asset Store](https://assetstore.unity.com/packages/tools/integration/unirx-reactive-extensions-for-unity-17276)
## Scripts Index

- **BarrelRx** - 3D Explosive barrel
- **BulletRx** - 3D Bullet with impact
- **PersuaderRx** - FPS Weapon script
- **PlayerMovementRx** - FPS Controller (inspired by DOOM Eternal)
- **PlayerViewRx** - FPS Controller mouse view section
- **RecoilRx** - Procedural Recoil script
- **SpectatorCameraRx** - Simple spectator camera controller


## BarrelRx
This example shows how an explosive barrel works with UniRx reactive properties

```c#
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
```
When the barrel suffers damage, it will emit a value, but we only want one value under 1, so, when its health goes under 1, the barrel will explode.

## BulletRx
This example shows how a bullet works and how to handle an impact depending which surface or object hits

```c#
public class BulletRx : MonoBehaviour
{
    public float damage = 35f;
    [Space]
    public GameObject woodHole;
    public GameObject stoneHole;
    public GameObject metalHole;
    public GameObject sandHole;
    //We create a dictionary for surface impact particles
    private IDictionary<string, GameObject> holes = new Dictionary<string, GameObject>();

    void Start()
    {
        holes["Stone"] = stoneHole;
        holes["Wood"] = woodHole;
        holes["Metal"] = metalHole;
        holes["Sand"] = sandHole;

        IObservable<Collision> collisionEnterObservable = this.OnCollisionEnterAsObservable();

        //This stream handles impacts
        collisionEnterObservable
        .Subscribe(Impact);

        //This stream handles explosive barrels impacts
        collisionEnterObservable
        .Where(IsBarrel)
        .Subscribe(DamageBarrel);

        Destroy(gameObject, 3f);
    }

    private void Impact(Collision collision)
    {
        //Tags is just a component with a string list
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
```
As we can see, we have two main streams that handles impacts and barrel impacts. When they emit a value, the correct function is called.

## PersuaderRx

This script was called after a [free 3D asset from Sketchfab](https://sketchfab.com/3d-models/persuader-animated-c915c024e4e44d9fad250242d094d4e2)

It shows a First Person Shooter weapon, in this case, an assault rifle.

```c#
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
```
As it is a long file, I won't paste it completely, but [you can check the full file here](https://github.com/rubencfu/unirx-examples/blob/main/PersuaderRx.cs)

## PlayerMovementRx
This is a FPS controller inspired by DOOM Eternal, although is not finished, it still works and is a great example about high legibility of UniRx

```c#
    void Start() 
    {
        IObservable<Unit> update = this.UpdateAsObservable();

        //Movement
        update.Subscribe(_ => HandleMovement());

        //Jump
        update
        .Where(_ => canJump())
        .Subscribe(_ => Jump());

        //Crouch
        update
        .Where(_ => Input.GetButtonDown("Crouch"))
        .Subscribe(_ => Crouch());

        //Dash
        update
        .Where(_ => canDash())
        .Subscribe(_ => Dash());

        //Flamethrower (It's useless but it's cool)
        update
        .Where(_ => Input.GetButtonDown("Flamethrower"))
        .Subscribe(_ => Flamethrower());
    }
```

[Full file here](https://github.com/rubencfu/unirx-examples/blob/main/PlayerMovementRx.cs)

## PlayerViewRx
Simple script that handles player mouse view 

```c#
public class PlayerViewRx : MonoBehaviour
{
    public float sensitivity = 100;

    public Transform playerBody;

    private float rotationX;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Observable.EveryLateUpdate()
        .Select(mouseAxes => 
            new MouseAxes(
                Input.GetAxis("Mouse X"), 
                Input.GetAxis("Mouse Y")
            )
        )
        .Subscribe(mouseAxes => HandleSubscription(mouseAxes));
    }
    
    private void HandleSubscription(MouseAxes mouseAxes)
    {
        float mouseX = mouseAxes.X * sensitivity * Time.deltaTime;
        float mouseY = mouseAxes.Y * sensitivity * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90);

        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
```
[Full file here](https://github.com/rubencfu/unirx-examples/blob/main/PlayerViewRx.cs)

## RecoilRx
Procedural recoil script made with UniRx, but is simpler than it sounds

```c#
public class RecoilRx : MonoBehaviour
{
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;

    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    void Start()
    {
        Observable.EveryUpdate()
        .Subscribe(_ => CalcRotations());
    }

    private void CalcRotations()
    {
        targetRotation = GetTargetRotation();
        currentRotation = GetCurrentRotation();
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    private Vector3 GetTargetRotation() => Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);

    private Vector3 GetCurrentRotation() => Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);

    public void RecoilFire()
    {
        targetRotation += new Vector3(
            recoilX, 
            Random.Range(-recoilY, recoilY), 
            Random.Range(-recoilZ, recoilZ)
        );
    }
}
```
It must be called from the weapon that is shooting, and should be attached to a parent object of the camera

## SpectatorCameraRx
Very simple spectator camera, seriously, it's pretty simple.

```c#
public class spectatorCameraRX : MonoBehaviour
{
    [Header("Look Sensitivity")]
    public float sensitivity = 6;

    [Header("Clamping")]
    public float minY = -90;
    public float maxY = 90;

    [Header("Spectator")]
    public float speed = 10;

    private float rotationX;
    private float rotationY;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Observable.EveryLateUpdate()
        .Select(axes => 
            new Axes(
                Input.GetAxis("Mouse X"), 
                Input.GetAxis("Mouse Y"), 
                Input.GetAxis("Horizontal"), 
                Input.GetAxis("Vertical")
            )
        )
        .Subscribe(HandleSubscription);
    }

    private void HandleSubscription(Axes axes) 
    {
        rotationX += axes.X * sensitivity;
        rotationY += axes.Y * sensitivity;

        rotationY = Mathf.Clamp(rotationY, minY, maxY);

        transform.rotation = Quaternion.Euler(-rotationY, rotationX, 0);

        float x = axes.Horizontal;
        float y = 0;
        float z = axes.Vertical;

        Vector3 direction = transform.right * x + transform.up * y + transform.forward * z;
        transform.position += direction * speed * Time.deltaTime;
    }
}
```
[Full file here](https://github.com/rubencfu/unirx-examples/blob/main/spectatorCameraRX.cs)
