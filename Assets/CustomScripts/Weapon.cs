using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif
using StarterAssets;
using System.Collections;
using UnityEngine.UI;

public class Weapon : MonoBehaviour, Interactable
{
    //Weapon stats
    public string weaponName;
    
    public bool isEquipped = false;
    public float maxAmmo;
    public float ammoLeft;
    public float damagePerBullet;
    [Tooltip("Rounds per minute")]
    public float fireRate;
    public float weaponRange;
    public float reloadTime;
    public float reloadEmptyTime;

    public bool canBeAutomatic = true;
    public bool isAutomatic;
    public bool isReloading;
    public bool isReadyToShoot = true;

    public bool isTriggerHeld;
    public bool isUsingADS;

    public float fireAnimation = 0f;

    private Transform shootFromWhere;

    public Rigidbody rigidBody;
    public Collider collider;

    //Parents
    private Transform weaponStorage;
    private Transform brandish;
    private Transform currentParent;

    private ThirdPersonController player;
    private Character playerStats;
    private Transform camera;
    public Transform chest;
    //CHILDREN

    private Transform muzzle;

    //LEAVEAIM
    public float secondsUntilInactive = 600;
    public float leaveAimTimer = 0;
    public bool aimAfterFire;

    public bool isCoroutineActive;

    private Coroutine storeWeapon = null;

    //IMAGES

    public Sprite gunSprite;
    public GameObject muzzleFlash;

    //AUDIO

    public AudioClip shotFiredSound;
    public AudioClip lastBulletFiredSound;
    public AudioClip reloadSound;
    public AudioClip reloadEmptySound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ammoLeft = maxAmmo;
        isReadyToShoot = true;
        isReloading = false;
        muzzle = FindDescendants(transform, "Muzzle");
        IfOwnerNPC();
        rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Pause.isGamePaused)
        {
            if (leaveAimTimer < secondsUntilInactive)
            {
                leaveAimTimer++;
            }

            if (isEquipped && leaveAimTimer == secondsUntilInactive)
            {
                gameObject.SetActive(false);
            }

            FindCamera();
            if (shootFromWhere != null)
            {
                Debug.DrawRay(shootFromWhere.position, shootFromWhere.forward * weaponRange, Color.green);
            }

            if (storeWeapon == null)
            {
                isCoroutineActive = false;
            }
            else if (storeWeapon != null)
            {
                isCoroutineActive = true;
            }
        }
    }

    void OnDisable()
    {
        fireAnimation = 0;
        StopAllCoroutines(); // stop ResetShot
        isReadyToShoot = true;
    }

    //Assuming weapon is on the ground or not used by anyone else
    public void Interact(GameObject o)
    {
        if (!isEquipped)
        {
            player = o.GetComponent<ThirdPersonController>();
            playerStats = o.GetComponent<Character>();
            weaponStorage = FindDescendants(o.transform, "StorageEmpty");
            brandish = FindDescendants(o.transform, "BrandishEmpty");

            //Drops current weapon to make space for this one
            foreach(Transform weapon in weaponStorage)
            {
                Weapon weaponToBeReplaced = weapon.GetComponent<Weapon>();
                if(weaponToBeReplaced !=null && weaponToBeReplaced != this)
                {
                    weaponToBeReplaced.Drop();
                }
            }

            foreach (Transform weapon in brandish)
            {
                Weapon weaponToBeReplaced = weapon.GetComponent<Weapon>();
                if (weaponToBeReplaced != null && weaponToBeReplaced != this)
                {
                    weaponToBeReplaced.Drop();
                }
            }

            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            gameObject.SetActive(true);
            HideItem(false);
            ChangeParent(weaponStorage);
            rigidBody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            collider.enabled = false;
            isEquipped = true;

        }
    }

    public string Description()
    {
        return "Switch to " + weaponName+" ("+ammoLeft+"/"+maxAmmo+")";
    }

    public bool CanHoldInteract()
    {
        return false;
    }

    public bool Release()
    {
        return true;
    }

    public void ReleaseAction()
    {

    }

    public void IfOwnerNPC()
    {
        if (transform.root.name != "PlayerArmature" && transform.parent!=null)
        {
           
            weaponStorage = FindDescendants(transform.root, "StorageEmpty");
            brandish = FindDescendants(transform.root, "BrandishEmpty");
            playerStats = transform.root.gameObject.GetComponent<Character>();
            //Debug.Log(playerStats);
            SetAimFromChest();
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            gameObject.SetActive(true);
            HideItem(false);
            ChangeParent(weaponStorage);
            rigidBody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            collider.enabled = false;
            isEquipped = true;
        }
    }

    public void Drop()
    {
        if (isEquipped)
        {
            gameObject.layer = LayerMask.NameToLayer("Ignore Camera");
            gameObject.SetActive(true);
            transform.SetParent(null);
            /*
            transform.position = brandish.position;
            transform.rotation = brandish.rotation;
            */
            transform.rotation = Quaternion.identity;

            rigidBody.constraints = RigidbodyConstraints.None;
            rigidBody.isKinematic = false;
            collider.enabled = true;
            rigidBody.useGravity = true;
            isEquipped = false;
            player = null;
        }
    }

    public Transform FindDescendants(Transform parent, string name)
    {
        foreach(Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }
            else if (FindDescendants(child, name) != null)
            {
                return FindDescendants(child, name);
            }
        }
        return null;
    }

    //PARENT SYSTEM
    public void ChangeParent(Transform t)
    {
        transform.SetParent(t);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    //WEAPON SYSTEM
    public void Aim()
    {
        gameObject.SetActive(true);
        ChangeParent(brandish);
        HideItem(false);
    }

    public void LeaveAim()
    {
        if (!aimAfterFire)
        {
            ChangeParent(weaponStorage);
            HideItem(true);
        }
    }

    public void Fire()
    {
        if (!isReloading)
        {
            if(ammoLeft>0 && isReadyToShoot)
            {
                //Debug.Log("Shooting");
                ChangeParent(brandish);
                gameObject.SetActive(true);
                HideItem(false);
                Instantiate(muzzleFlash, muzzle.position, Quaternion.Euler(0, 180, 0));
                fireAnimation = 1;
                if (!isUsingADS)
                {
                    if (storeWeapon != null) StopCoroutine(storeWeapon);
                    storeWeapon = StartCoroutine(StoreWeapon());
                }

                AudioSource.PlayClipAtPoint(shotFiredSound, transform.position, 1f);
                if (lastBulletFiredSound != null && ammoLeft == 1)
                {
                    AudioSource.PlayClipAtPoint(lastBulletFiredSound, transform.position, 1f);
                }

                HitTarget();
                isReadyToShoot = false;
                ammoLeft--;
                StartCoroutine(ResetShot());
            }
            else if (ammoLeft == 0 && isReadyToShoot)
            {
                //Debug.Log("Calling ReloadEmpty After Attempting to Shoot");
                isReloading = true;
                ReloadEmpty();
            }
        }
    }

    public void Reload()
    {
        if (ammoLeft < maxAmmo && !isReloading)
        { 
            if (ammoLeft == 0)
            {
                ReloadEmpty();
                return;
            }
            else
            {
                //Debug.Log("Calling Normal Reload");
                isReloading = true;
                AudioSource.PlayClipAtPoint(reloadSound, transform.position, 1f);
                ChangeParent(weaponStorage);
                if (storeWeapon != null) StopCoroutine(storeWeapon);
                storeWeapon = null;
                Invoke("RefillAmmo", reloadTime);
            }
        }
    }

    public void ReloadEmpty()
    {
        //Debug.Log("Calling ReloadEmpty");
        isReloading = true;
        if (reloadEmptySound != null)
        {
            AudioSource.PlayClipAtPoint(reloadEmptySound, transform.position, 1f);
        }
        else
        {
            AudioSource.PlayClipAtPoint(reloadSound, transform.position, 1f);
        }
        ChangeParent(weaponStorage);
        if(storeWeapon!=null) StopCoroutine(storeWeapon);
        storeWeapon = null;
        aimAfterFire = false;
        Invoke("RefillAmmo", reloadEmptyTime);
    }

    public void RefillAmmo()
    {
        //Debug.Log("Refilling Ammo");
        ammoLeft = maxAmmo;
        isReloading = false;
    }

    public void SwitchFireMode()
    {
        if (canBeAutomatic)
        {
            isAutomatic = !isAutomatic;
        }
    }

    public IEnumerator ResetShot()
    {
        if (isAutomatic)
        {
            //Debug.Log("Loading Another Shot");
            yield return new WaitForSeconds(1/fireRate);
            fireAnimation = 0;
            hitTarget = false;
            yield return new WaitForSeconds(59/fireRate);
            isReadyToShoot = true;
        }
        else if (!isAutomatic)
        {
            //Debug.Log("Waiting for releasing trigger");
            yield return new WaitForSeconds(1 / fireRate);
            fireAnimation = 0;
            hitTarget = false;
            yield return new WaitForSeconds(59 /fireRate);
            yield return new WaitUntil(() => !isTriggerHeld);
            isReadyToShoot = true;
        }
    }

    public void HideItem(bool startTimer)
    {
        if (startTimer)
        {
            if (leaveAimTimer == secondsUntilInactive)
            {
                gameObject.SetActive(false);
                leaveAimTimer = 0;
            }
        }
        else
        {
            leaveAimTimer = 0;
        }
    }

    public IEnumerator StoreWeapon()
    {
        aimAfterFire = true;
        yield return new WaitForSeconds(2);
        yield return new WaitUntil(() => !isUsingADS);
        LeaveAim();
        aimAfterFire = false;
        storeWeapon = null;
    }

    public void SetAimFromBarrel()
    {
        shootFromWhere = muzzle;
    }

    public void SetAimFromCamera()
    {
        shootFromWhere = camera;
    }

    public void SetAimFromChest()
    {
        shootFromWhere = chest;
    }

    public Transform GetWeaponStorage()
    {
        return weaponStorage;
    }

    public Transform GetShootFromWhere()
    {
        return shootFromWhere;
    }

    public void FindCamera()
    {
        Transform owner = transform.root;
        camera = FindDescendants(owner, "MainCamera");
    }

    public bool hitTarget = false;

    public void HitTarget()
    {
        Ray r = new Ray(shootFromWhere.position, shootFromWhere.forward);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, weaponRange))
        {
            Debug.DrawRay(shootFromWhere.position, shootFromWhere.forward * weaponRange);

            if (hit.collider != null)
            {
                /*
                if (shootFromWhere == camera)
                {
                    Debug.Log("Hit something while aiming");
                }
                else if (shootFromWhere == muzzle)
                {
                    Debug.Log("Hit something while blind firing");
                }
                */
                //Debug.Log("Hit " + hit.collider);
                if (hit.collider.TryGetComponent<Character>(out Character c))
                {
                    if(c.hitParticle!=null)
                    {
                        Instantiate(c.hitParticle, hit.point, Quaternion.Euler(0, 180, 0));
                    }
                    
                    //This is to avoid friendly fire
                    if (c.faction != playerStats.faction || (c.faction==playerStats.faction && Pause.allowFriendlyFire))
                    {
                        c.decreaseHealthAndArmor(damagePerBullet/2,damagePerBullet);
                        hitTarget = true;

                        if (c.health <= 0)
                        {
                            playerStats.increaseCurrency(c.currency);
                            c.setCurrency(0f);
                        }

                    }
                }
            }
            else
            {
                Debug.Log("Hit nothing");
            }
        }
    }
}
