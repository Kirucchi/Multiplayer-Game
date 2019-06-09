using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerController : MonoBehaviour {

    private PUN2_RoomController roomController;

    public float speed;
    public Camera playerCamera;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private PhotonView photonView;
    [HideInInspector]
    public string playerName;
    private string shotByPlayer;
    
    public GameObject weaponPivot;
    public GameObject weaponObject;
    public GameObject bulletSpawnPoint;
    public Weapon weapon;
    private List<GameObject> bullets;
    private float radius;
    private float shootDelay;
    private float reloadTime;
    private int maxAmmo;
    private int maxClip;

    private int currentMaxAmmo;
    private int currentClip;
    private float timePassed;

    public Text ammoText;
    public Text reloadingText;
    private GameObject pauseMenu;

    private bool isPaused = false;
    private bool isReloading = false;

    private Vector3 currentMousePosition;

    //health stuff
    public float maxHP;
    private float HP;
    public GameObject healthbar;
    private RectTransform healthTransform;
    private float minHealthbar;
    private float maxHealthbar;

    public int kills;

    public Text nameText;

	// Use this for initialization
	void Awake () {
        animator = gameObject.GetComponent<Animator>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        pauseMenu = GameObject.Find("PlayerSelect");
        roomController = GameObject.Find("_RoomController").GetComponent<PUN2_RoomController>();
        photonView = gameObject.GetComponent<PhotonView>();
        //playerName = GetComponent<PhotonView>().Owner.NickName;

        bullets = new List<GameObject>();
        foreach (GameObject projectile in weapon.bullets) {
            bullets.Add(projectile);
        }
        radius = weapon.radius;
        shootDelay = weapon.shootDelay;
        reloadTime = weapon.reloadTime;
        maxAmmo = weapon.maxAmmo;
        maxClip = weapon.maxClip;
        currentMaxAmmo = maxAmmo;
        currentClip = maxClip;

        reloadingText.text = "";
        ammoText.text = maxClip + " / " + maxAmmo;
        timePassed = shootDelay;

        healthTransform = healthbar.GetComponent<RectTransform>();
        minHealthbar = healthTransform.localPosition.x - healthTransform.rect.width;
        maxHealthbar = healthTransform.localPosition.x;
        HP = maxHP;

        playerName = GetComponent<PhotonView>().Owner.NickName;
        nameText.text = playerName;
	}

    public PhotonView getPhotonView() {
        return photonView;
    }
    /*
    public void setPlayerName(string name) {
        playerName = name;
        nameText.text = playerName;
    }
    */
    // Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Escape)) {
            TogglePause();
        }

        if (!isPaused) {
            timePassed += Time.deltaTime;

            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            if (x < 0)
                spriteRenderer.flipX = true;
            if (x > 0)
                spriteRenderer.flipX = false;
         
            if (x != 0 || y != 0)
                animator.SetFloat("Speed", 1f);
            else
                animator.SetFloat("Speed", 0f);

            rb.velocity = new Vector3(x, y, 0).normalized * speed;
            //Vector3 newMousePosition = playerCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 newMousePosition = Input.mousePosition;
            if (newMousePosition.x > 0 && newMousePosition.x < Screen.width && newMousePosition.y > 0 && newMousePosition.y < Screen.height) {
                currentMousePosition = playerCamera.ScreenToWorldPoint(Input.mousePosition);
                currentMousePosition.z = 0;
                Vector3 weaponRotation = (currentMousePosition - weaponPivot.transform.position).normalized;

                currentMousePosition.x = currentMousePosition.x + Random.Range(-radius, radius);
                currentMousePosition.y = currentMousePosition.y + Random.Range(-radius, radius);
                Vector3 direction = (currentMousePosition - bulletSpawnPoint.transform.position).normalized;



                weaponPivot.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(weaponRotation.x, weaponRotation.y) * Mathf.Rad2Deg);
                if (Input.GetButton("Fire1")) {
                    if (timePassed >= shootDelay && currentClip > 0 && !isReloading) {
                        roomController.CallShoot(bulletSpawnPoint.transform.position, direction, bullets, gameObject, playerName);
                        timePassed = 0f;
                        currentClip--;
                        UpdateAmmoUI();
                    }
                }
            }
            
            if (Input.GetKeyDown(KeyCode.R)) {
                if (currentClip < maxClip && currentMaxAmmo > 0 && !isReloading) {
                    isReloading = true;
                    StartCoroutine(Reload());
                }
            }
        }
        
    }

    void TogglePause() {
        if (!isPaused) {
            rb.velocity = Vector3.zero;
            animator.SetFloat("Speed", 0f);
            pauseMenu.SetActive(true);
            isPaused = true;
        }
        else if (isPaused) {
            pauseMenu.SetActive(false);
            isPaused = false;
        }
    }
    /*
    void badShoot(Vector3 direction) {
        roomController.ShootBullet(bullets[(int)Random.Range(0, bullets.Count)], bulletSpawnPoint.transform.position, direction);
        currentClip--;
        UpdateAmmoUI();
    }
    */
    void UpdateAmmoUI() {
        ammoText.text = currentClip + " / " + currentMaxAmmo;
    }

    IEnumerator Reload() {
        reloadingText.enabled = true;
        float timeElapsed = 0;
        for (int i=1; i<=reloadTime; i++) {
            if (timeElapsed >= 0.4)
                timeElapsed = 0;
            //weaponObject.transform.localRotation = Quaternion.Euler(0, 0, 18/reloadTime * i);
            weaponObject.transform.Rotate(Vector3.forward, 1080/reloadTime);
            if (timeElapsed == 0)
                reloadingText.text = "Reloading";
            if (timeElapsed >= 0.1f)
                reloadingText.text = "Reloading.";
            if (timeElapsed >= 0.2f)
                reloadingText.text = "Reloading..";
            if (timeElapsed >= 0.3f)
                reloadingText.text = "Reloading...";
            timeElapsed += 0.01f;
            yield return new WaitForSecondsRealtime(0.01f);
        }
        reloadingText.text = "";
        reloadingText.enabled = false;
        int difference = maxClip - currentClip;
        if (difference <= currentMaxAmmo) {
            currentClip += difference;
            currentMaxAmmo -= difference;
        }
        else {
            currentClip += currentMaxAmmo;
            currentMaxAmmo = 0;
        }
        isReloading = false;
        UpdateAmmoUI();
    }
    
    void UpdateHealthbar() {
        healthTransform.localPosition = Vector3.Lerp(new Vector3(minHealthbar, healthTransform.position.y, 0), new Vector3(maxHealthbar, healthTransform.position.y, 0), HP / maxHP);
    }

    public void TakeDamage(float damage, string name) {
        HP -= damage;
        UpdateHealthbar();
        roomController.AddPoint(name);
        if (HP <= 0) {
            roomController.DestroyPlayer();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("BubbleTea") && HP < maxHP) {
            HP += 100;
            if (HP > maxHP)
                HP = maxHP;
            UpdateHealthbar();
            roomController.RemoveBubbleTea(collision.GetComponent<BubbleTea>().number);
        }
        if (collision.CompareTag("Fishbowl") && currentMaxAmmo < maxAmmo) {
            currentMaxAmmo += maxAmmo / 2;
            if (currentMaxAmmo > maxAmmo)
                currentMaxAmmo = maxAmmo;
            UpdateAmmoUI();
            roomController.RemoveFishbowl(collision.GetComponent<Fishbowl>().number);
        }
    }
}
