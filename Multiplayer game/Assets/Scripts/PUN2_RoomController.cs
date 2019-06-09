using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PUN2_RoomController : MonoBehaviourPunCallbacks {
    

    public Texture2D cursorTexture;

    public GameObject characterSelectMenu;
    public Text weaponNameText;
    public Image weaponImageRenderer;
    public Text weaponDescriptionText;
    [Header("List of Weapons")]
    public List<WeaponSelectionObject> weaponList = new List<WeaponSelectionObject>();
    private int selectedIndex = 0;
    [Header("List of Characters")]
    public List<GameObject> playerList = new List<GameObject>();

    public InputField nameInput;
    public Text nameErrorText;

    private GameObject currentPlayer = null;

    [SerializeField]
    private List<GameObject> bullets;
    [SerializeField]
    private List<GameObject> spawnedProjectiles;
    private int bulletNum = 0;
    public GameObject[] bubbleTeaObjects = new GameObject[6];
    public GameObject[] FishbowlObjects = new GameObject[9];
    public Transform[] spawnPoints = new Transform[22];

    // Use this for initialization
    void Start() {

        bullets = new List<GameObject>();
        spawnedProjectiles = new List<GameObject>();
        //In case we started this demo with the wrong scene being active, simply load the menu scene
        if (PhotonNetwork.CurrentRoom == null) {
            Debug.Log("Is not in the room, returning back to Lobby");
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameLobby");
            return;
        }

        //We're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
        //PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, Quaternion.identity, 0);

        Camera.main.enabled = false;
        //Cursor.SetCursor(cursorTexture, new Vector2(cursorTexture.width/2, cursorTexture.height/2), CursorMode.Auto);
        //Debug.Log(cursorTexture.width / 2 + " " + cursorTexture.height/2);
        characterSelectMenu.SetActive(true);
        nameErrorText.enabled = false;
        updateWeaponSelect();
    }

    public void LeaveRoom() {
        PhotonNetwork.LeaveRoom();
    }
    
    void OnGUI() {
        if (PhotonNetwork.CurrentRoom == null)
            return;
        
        //Show the Room name
        GUI.Label(new Rect(10, 10, 200, 25), PhotonNetwork.CurrentRoom.Name + " Scoreboard:");

        //Show the list of the players connected to this Room
        /*
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++) {
            //Show if this player is a Master Client. There can only be one Master Client per Room so use this to define the authoritative logic etc.)
            string isMasterClient = (PhotonNetwork.PlayerList[i].IsMasterClient ? ": MasterClient" : "");
            GUI.Label(new Rect(5, 35 + 30 * i, 200, 25), PhotonNetwork.PlayerList[i].NickName + isMasterClient);
        }
        */
        List<GameObject> playerList = UpdateScores();
        for (int i=0; i<playerList.Count; i++) {
            GUI.Label(new Rect(10, 35 + 25 * i, 200, 25), playerList[i].GetComponent<PlayerController>().playerName);
            GUI.Label(new Rect(90, 35 + 25 * i, 200, 25), playerList[i].GetComponent<PlayerController>().kills.ToString());
        }

    }
    
    public override void OnLeftRoom() {
        //We have left the Room, return back to the GameLobby
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameLobby");
    }
    /*
    public void ShootBullet(GameObject bullet, Vector3 position, Vector3 direction) {
        GameObject spawnedBullet = PhotonNetwork.Instantiate(bullet.name, position, Quaternion.Euler(direction));
        spawnedBullet.tag = "MyBullet";
        spawnedBullet.GetComponent<BulletMove>().SetDirection(direction);
        spawnedBullet.GetComponent<BulletMove>().isMoving = true;
    }
    */
    [System.Serializable]
    public class WeaponSelectionObject {
        public string weaponName;
        public Sprite weaponSprite;
        [TextArea(minLines:5, maxLines:10)]
        public string description;
    }

    public void updateWeaponSelect() {
        weaponNameText.text = weaponList[selectedIndex].weaponName;
        weaponImageRenderer.sprite = weaponList[selectedIndex].weaponSprite;
        weaponDescriptionText.text = weaponList[selectedIndex].description;
    }

    public void PickNextWeapon() {
        selectedIndex++;
        if (selectedIndex > weaponList.Count - 1)
            selectedIndex = 0;

        updateWeaponSelect();
    }

    public void PickPreviousWeapon() {
        selectedIndex--;
        if (selectedIndex < 0)
            selectedIndex = weaponList.Count - 1;

        updateWeaponSelect();
    }

    public void SpawnPlayer() {
        string inputName = nameInput.text;
        if (inputName.Equals("")) {
            nameErrorText.enabled = true;
            Debug.Log("called1");
            nameErrorText.text = "Please enter a name!";
            return;
        }
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players) {
            if (inputName.Equals(player.GetComponent<PlayerController>().playerName) && inputName != PhotonNetwork.NickName) {
                nameErrorText.enabled = true;
                Debug.Log("called2");
                nameErrorText.text = "Name already taken!";
                return;
            }
        }
        if (currentPlayer != null) {
            characterSelectMenu.transform.parent = null;
            PhotonNetwork.Destroy(currentPlayer);
        }
        PhotonNetwork.NickName = inputName;
        Transform spawnPosition = spawnPoints[(int)Random.Range(0, spawnPoints.Length)];
        currentPlayer = PhotonNetwork.Instantiate(playerList[selectedIndex].name, spawnPosition.position, Quaternion.identity, 0);
        //currentPlayer.GetComponent<PlayerController>().setPlayerName(inputName);
        nameErrorText.enabled = false;
        characterSelectMenu.transform.parent = currentPlayer.transform;
        characterSelectMenu.transform.localPosition = Vector3.zero;
        characterSelectMenu.SetActive(false);
    }

    public void CallShoot(Vector3 position, Vector3 direction, List<GameObject> bulletList, GameObject player, string playerName) {
        bullets = new List<GameObject>();
        foreach (GameObject bullet in bulletList) {
            bullets.Add(bullet);
        }
        string bulletName = bullets[Random.Range(0, bullets.Count)].name;
        this.photonView.RPC("Shoot", RpcTarget.AllViaServer, new object[] { bulletName, position, direction, bulletNum, playerName });
    }

    [PunRPC]
    public void Shoot(string bulletname, Vector3 position, Vector3 direction, int bulletNumber, string shooter, PhotonMessageInfo info) {
        bulletNum++;
        float angle = Mathf.Atan2(direction.y, direction.x);
        GameObject spawnedBullet = Instantiate(Resources.Load<GameObject>(bulletname), position, Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg - 90));
        //GameObject spawnedBullet = Instantiate(bullets[Random.Range(0, bullets.Count)], position, Quaternion.Euler(direction));
        BulletMove bulletMoveScript = spawnedBullet.GetComponent<BulletMove>();
        bulletMoveScript.setStartPosition(position);
        bulletMoveScript.SetDirection(direction.normalized);
        bulletMoveScript.setPlayer(shooter);
        bulletMoveScript.setBulletNumber(bulletNumber);
        bulletMoveScript.setCreateTime(Mathf.Abs((float)Time.time));//info.SentServerTime
        spawnedProjectiles.Add(spawnedBullet);
    }

    public void projectileHIt(int bulletId) {
        photonView.RPC("OnProjectileHit", RpcTarget.Others, new object[] { bulletId });
    }

    [PunRPC]
    public void OnProjectileHit(int bulletId) {
        spawnedProjectiles.RemoveAll(item => item == null);
        foreach (GameObject projectile in spawnedProjectiles) {
            if (projectile.GetComponent<BulletMove>().bulletNumber == bulletId) {
                Destroy(projectile);
                return;
            }
        }
    }

    public void DestroyPlayer() {
        StartCoroutine(KillPlayerEnumerator());
    }

    IEnumerator KillPlayerEnumerator() {
        characterSelectMenu.transform.parent = null;
        characterSelectMenu.SetActive(true);
        characterSelectMenu.transform.GetChild(1).gameObject.SetActive(false);
        Debug.Log(currentPlayer.GetComponent<PlayerController>().playerName);
        PhotonNetwork.Destroy(currentPlayer);
        yield return new WaitForSeconds(1.3f);
        characterSelectMenu.transform.GetChild(1).gameObject.SetActive(true);
    }

    List<GameObject> UpdateScores() {
        List<GameObject> players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        players.Sort(sortByKills);
        return players;
    }

    static int sortByKills(GameObject p1, GameObject p2) {
        return p1.GetComponent<PlayerController>().kills.CompareTo(p2.GetComponent<PlayerController>().kills);
    }

    public void AddPoint(string shooter) {
        photonView.RPC("AddPointGlobal", RpcTarget.All, new object[] { shooter } );
    }

    [PunRPC]
    void AddPointGlobal(string shooter) {
        List<GameObject> players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        foreach (GameObject player in players) {
            PlayerController playercomponent = player.GetComponent<PlayerController>();
            if (playercomponent.playerName.Equals(shooter)) {
                playercomponent.kills++;
                return;
            }
        }
    }

    public void RemoveBubbleTea(int num) {
        photonView.RPC("RemoveBubbleTeaGlobal", RpcTarget.All, new object[] { num });
    }

    [PunRPC]
    void RemoveBubbleTeaGlobal(int num) {
        StartCoroutine(DeactivateTemporarily(bubbleTeaObjects[num], 15));
    }

    IEnumerator DeactivateTemporarily(GameObject go, int seconds) {
        go.SetActive(false);
        yield return new WaitForSecondsRealtime(seconds);
        go.SetActive(true);
    }

    public void RemoveFishbowl(int num) {
        photonView.RPC("RemoveFishbowlGlobal", RpcTarget.All, new object[] { num });
    }

    [PunRPC]
    void RemoveFishbowlGlobal(int num) {
        StartCoroutine(DeactivateTemporarily(FishbowlObjects[num], 5));
    }
    
    
}
