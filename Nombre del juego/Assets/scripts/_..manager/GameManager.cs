using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region instance
    static private GameManager _instance;
    static public GameManager Instance
    {
        get
        {
            return _instance;
        }
    }
    #endregion

    #region components
    public GameObject _player;

    private GameObject _bow;

    private GameObject _enemyMov;

    private GameObject _enemyDisp;

    public GameObject _Camera;

    private CamaraMovement _camMov;

    private bool _arcade = false;

    private Player_Life_Component _myPlayer_Life_Component;

    public LevelManager _levelManager;

    public Transform _boss;

    #endregion



    public string gameId;
    private void Awake()
    {
       

        if (_instance == null)
        {
            _instance = this; 
            Debug.Log("Inicio de sesion");
			SessionStartEvent sessionStartEvent = new SessionStartEvent();
			Tracker.Instance.SendEvent(sessionStartEvent);
		}
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void StartMatch()
    {
        _arcade = false;
        //Al darle al botón carga la escena principal

        Debug.Log("Juego Principal");

        gameId = Guid.NewGuid().ToString();

		GameStartEvent gameStartEvent = new GameStartEvent(gameId);
        Tracker.Instance.SendEvent(gameStartEvent);

        SceneManager.LoadScene("SampleScene");
        AudioManager.Instance.Stop("Menu");
        AudioManager.Instance.Play("Main");
        AudioManager.Instance.Stop("Win");
        AudioManager.Instance.Stop("Lose");
        AudioManager.Instance.Stop("Tutorial");
    }
    public void StartMatch2()
    {
        _arcade = false;
        //Al darle al botón carga el tutorial
        SceneManager.LoadScene("Tutorial");
        AudioManager.Instance.Stop("Menu");
        AudioManager.Instance.Play("Tutorial");
    }
    public void StartArcade()
    {
        _arcade = true;
        //Al darle al botón carga el modo arcade
        SceneManager.LoadScene("ArcadeScene");
        AudioManager.Instance.Stop("Menu");
        AudioManager.Instance.Play("Arcade");
    }
    public void RestartMatch()
    {
		Debug.Log("Fin partida");
		GameEndEvent gameEndEvent = new GameEndEvent(gameId);
		Tracker.Instance.SendEvent(gameEndEvent);

		SceneManager.LoadScene("Main Menu");
        AudioManager.Instance.Stop("Win");
        AudioManager.Instance.Stop("Lose");
        AudioManager.Instance.Stop("Main");
        AudioManager.Instance.Stop("Arcade");
        AudioManager.Instance.Stop("Tutorial");
        AudioManager.Instance.Play("Menu");

    }
    public void QuitGame()
    {
        Debug.Log("Fin de la sesion");
        SessionEndEvent sessionEndEvent = new SessionEndEvent();
        Tracker.Instance.SendEvent(sessionEndEvent);
		Tracker.Instance.DestroyTracker();
#if UNITY_EDITOR

		EditorApplication.isPlaying = false;
#else
        
        Application.Quit();
#endif
	}

	void Start()
    {
        Time.timeScale = 0.0f;
        AudioManager.Instance.Play("Menu");
        _arcade = false;
    }
	public void PlayerDies()
    {
        Debug.Log("Fin partida");
        GameEndEvent gameEndEvent = new GameEndEvent(GameManager.Instance.gameId);
        Tracker.Instance.SendEvent(gameEndEvent);

        Player_Life_Component.instance.isAlive = false;
        _bow.SetActive(false);
        OnPlayerDefeat();
    }
    public void OnPlyerDamage(int damage)
    {
        _myPlayer_Life_Component.Damage(damage);

    }
    public void EnemyDamage(int Damage, GameObject enemy)
    {
        enemy.GetComponent<Enemy_Life_Component>().Damage(Damage);
    }
    public void pause()
    {
        Time.timeScale = 0.0f;
        _levelManager._bow.SetActive(false);
        _levelManager._enemyDisp.SetActive(false);
        _levelManager._enemyMov.SetActive(false);
        UIManager.Instance.SetPauseMenu(true);
        GetComponent<SliderVolume>();
        if (_levelManager._Camera.GetComponent<CamaraMovement>()) _levelManager._Camera.GetComponent<CamaraMovement>().enabled = false;
        else _levelManager._Camera.GetComponent<CameraArcade>().enabled = false;


    }
    public void Restore()
    {
        UIManager.Instance.SetPauseMenu(false);
        Time.timeScale = 1.0f;
        _levelManager._bow.SetActive(true);
        _levelManager._enemyDisp.SetActive(true);
        _levelManager._enemyMov.SetActive(true);
        if (_levelManager._Camera.GetComponent<CamaraMovement>()) _levelManager._Camera.GetComponent<CamaraMovement>().enabled = true;
        else _levelManager._Camera.GetComponent<CameraArcade>().enabled = true;

    }

    public void OnPlayerVictory()
    {
        Time.timeScale = 0.0f;
        _bow.SetActive(false);
        _enemyDisp.SetActive(false);
        _enemyMov.SetActive(false);
        AudioManager.Instance.Stop("Main");
        AudioManager.Instance.Stop("Tutorial");
        AudioManager.Instance.Stop("Arcade");
        AudioManager.Instance.Play("Win");
        UIManager.Instance.SetVictoryMenu(true);
    }

    private void OnLevelWasLoaded(int level)  //cada vez que se cargue la escena principal
    {
        if (level != 0)
        {
            _levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
            if (level == 3)
            {
                //en caso de ser el arcade
                _player = _levelManager._player;
                _bow = _levelManager._bow;
                _Camera = _levelManager._Camera;
                _player.SetActive(true);
                _bow.SetActive(true);
                _myPlayer_Life_Component = _player.GetComponent<Player_Life_Component>();
                UIManager.Instance.UpdateScore(true);
            }
            else
            {
                _player = _levelManager._player;
                _bow = _levelManager._bow;
                _Camera = _levelManager._Camera;
                _enemyDisp = _levelManager._enemyDisp;
                _enemyMov = _levelManager._enemyMov;
                if (level == 1)
                {
                    _boss = _levelManager._boss;
                }
                else
                {
                    _boss = _player.transform;
                }
                _player.SetActive(true);
                _bow.SetActive(true);
                _enemyDisp.SetActive(true);
                _enemyMov.SetActive(true);

                _myPlayer_Life_Component = _player.GetComponent<Player_Life_Component>();
                UIManager.Instance.UpdateScore(true);
            }
        }
    }

    public void OnPlayerDefeat()
    {
        if (_arcade) { _Camera.GetComponent<CameraArcade>().enabled = false; }
        else { _Camera.GetComponent<CamaraMovement>().enabled = false; }
        AudioManager.Instance.Stop("Main");
        AudioManager.Instance.Stop("Tutorial");
        AudioManager.Instance.Stop("Arcade");
        AudioManager.Instance.Play("Lose");
        UIManager.Instance.SetLoseMenu(true);
        _bow.SetActive(false);
        _player.SetActive(false);
        Time.timeScale = 0.0f;
    }
}
