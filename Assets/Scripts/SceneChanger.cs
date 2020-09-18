using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneChanger : MonoBehaviour
{
    public KeyCode ReloadSceneKey, LoadNextLevelKey, closeGameKey;

    [Header("Kind-of a game manager now.. :')")]

    [SerializeField]
    private IntVariable amountOfEnemiesInScene;

    private int amountOfEnemiesLocal = 0;
    private int amountOfEnemiesMax;

    [SerializeField]
    private TextMeshProUGUI text;

    private string baseString = "Enemies left: ";

    private bool isFirstUpdate = true;
    private bool isLoadingScene = false;

    [SerializeField]
    private float waitBeforeLoad;

    [SerializeField]
    private SoundPlayer soundPlayer;

    [SerializeField]
    private bool isEndScene; //Quick and dirty for demo.. 

    private void Awake()
    {
        amountOfEnemiesInScene.amount = 0;
        if (text == null)
        {
            GameObject go = GameObject.Find("EnemiesText");
            if (go == null)
            {
                return;
            }
            text = go.GetComponent<TextMeshProUGUI>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isFirstUpdate)
        {
            amountOfEnemiesMax = amountOfEnemiesInScene.amount;
            isFirstUpdate = false;
        }

        //For testing purposes only:
        if (Input.GetKeyDown(ReloadSceneKey))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        if (Input.GetKeyDown(LoadNextLevelKey))
        {
            SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex + 1) % (SceneManager.sceneCountInBuildSettings));
        }
        if (Input.GetKeyDown(closeGameKey))
        {
            Application.Quit();
        }







        if (amountOfEnemiesInScene.amount <= 0 && !isEndScene)
        {
            StartCoroutine(LoadNextScene());
        }

        if (text == null)
        {
            Debug.Log("No TMPro-object named EnemyText in scene.");
            return;
        }
        if (amountOfEnemiesInScene.amount != amountOfEnemiesLocal)
        {
            amountOfEnemiesLocal = amountOfEnemiesInScene.amount;
            string str = amountOfEnemiesLocal + " / " + amountOfEnemiesMax;
            text.text = baseString + str;
        }

    }

    IEnumerator LoadNextScene()
    {
        if (isLoadingScene == true)
        {
            yield break;
        }
        isLoadingScene = true;
        //Play some sound or something.
        soundPlayer.PlaySound();

        yield return new WaitForSeconds(waitBeforeLoad);
        SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex + 1) % (SceneManager.sceneCountInBuildSettings));
    }
}
