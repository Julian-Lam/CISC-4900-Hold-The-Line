using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif
using StarterAssets;
using System.Collections;

public class Weapon : MonoBehaviour, Interactable
{
    //Weapon stats
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

    private Transform shootFromWhere;

    public Rigidbody rigidBody;
    public Collider collider;

    //Parents
    private Transform weaponStorage;
    private Transform brandish;
    private Transform currentParent;

    private ThirdPersonController player;
    private Transform camera;

    //CHILDREN

    private Transform muzzle;

    //LEAVEAIM
    public float secondsUntilInactive = 600;
    public float leaveAimTimer = 0;
    public bool aimAfterFire;

    private Coroutine storeWeapon = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ammoLeft = maxAmmo;
        isReadyToShoot = true;
        isReloading = false;
        muzzle = FindDescendants(transform, "Muzzle");
    }

    // Update is called once per frame
    void Update()
    {
        if (leaveAimTimer < secondsUntilInactive)
        {
            leaveAimTimer++;
        }
        FindCamera();
        if (shootFromWhere != null)
        {
            Debug.DrawRay(shootFromWhere.position, shootFromWhere.forward * weaponRange, Color.green);
        }
    }

    //Assuming weapon is on the ground or not used by anyone else
    public void Interact(GameObject o)
    {
        if (!isEquipped)
        {
            player = o.GetComponent<ThirdPersonController>();
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
        if (storeWeapon != null)
        {
            StopCoroutine(StoreWeapon());
        }
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
                //Debug.Log("Calling Shoot");
                ChangeParent(brandish);
                gameObject.SetActive(true);
                HideItem(false);

                if (!isUsingADS)
                {
                    if (storeWeapon == null)
                    {
                        storeWeapon = StartCoroutine(StoreWeapon());
                    }
                    else if (storeWeapon != null)
                    {
                        StopCoroutine(StoreWeapon());
                        storeWeapon = StartCoroutine(StoreWeapon());
                    }
                }
                HitTarget();
                isReadyToShoot = false;
                ammoLeft--;
                StartCoroutine(ResetShot());
            }
            else if (ammoLeft == 0)
            {
                //Debug.Log("Calling ReloadEmpty After Attempting to Shoot");
                Invoke("ReloadEmpty", 2);
            }
        }
    }

    public void Reload()
    {
        if (ammoLeft < maxAmmo && !isReloading)
        { 
            //Debug.Log("Calling Normal Reload");
            if (ammoLeft == 0)
            {
                ReloadEmpty();
                return;
            }
            else
            {
                ChangeParent(weaponStorage);
                if (storeWeapon != null) StopCoroutine(storeWeapon);
                storeWeapon = null;
                isReloading = true;
                Invoke("RefillAmmo", reloadTime);
            }
        }
    }

    public void ReloadEmpty()
    {
        //Debug.Log("Calling ReloadEmpty");
        if (!isReloading)
        {
            ChangeParent(weaponStorage);
            if(storeWeapon!=null) StopCoroutine(storeWeapon);
            storeWeapon = null;
            isReloading = true;
            Invoke("RefillAmmo", reloadTime);

        }
    }

    public void RefillAmmo()
    {
        //Debug.Log("Refilling Ammo");
        ammoLeft = maxAmmo;
        isReloading = false;
    }

    public void SwitchFireMode()
    {
        isAutomatic = !isAutomatic;
    }

    public IEnumerator ResetShot()
    {
        if (isAutomatic)
        {
            //Debug.Log("Loading Another Shot");
            yield return new WaitForSeconds(60 / fireRate);
            isReadyToShoot = true;
        }
        else if (!isAutomatic)
        {
            //Debug.Log("Waiting for releasing trigger");
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

    public void FindCamera()
    {
        Transform owner = transform.root;
        camera = FindDescendants(owner, "MainCamera");
    }

    public void HitTarget()
    {
        Ray r = new Ray(shootFromWhere.position, shootFromWhere.forward);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, weaponRange))
        {
            Debug.DrawRay(shootFromWhere.position, shootFromWhere.forward * weaponRange);

            if (hit.collider != null)
            {
                if (shootFromWhere == camera)
                {
                    Debug.Log("Hit something while aiming");
                }
                else if (shootFromWhere == muzzle)
                {
                    Debug.Log("Hit something while blind firing");
                }
            }
            else
            {
                Debug.Log("Hit nothing");
            }
        }
    }
}
