using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BowController : MonoBehaviour
{
    //accesor arco
    static public BowController BowInstance;
    [SerializeField]
    private float _atackS;
    private float _atCD = 0;
    private bool _canStillShoot = true;
    [SerializeField]
    private float _PredictionCoef;   
    private float _force = 0;


    private GameObject _arrow;

    [SerializeField]
    public GameObject _arrowTP;

    [SerializeField]
    public GameObject _arrowDamage;

    private int _multiplier=1;

    [SerializeField]
    private GameObject _point;

    private GameObject[] Points;

    [SerializeField]
    private int _numPoints;

    [SerializeField]
    private GameObject ShotPoint;

    [SerializeField]
    private GameObject _pDamage;
    [SerializeField]
    private GameObject _pTP;

    [SerializeField]
    public HUD_BarraTensado BarraTensado;

    void Start()
    {
        Points = new GameObject[_numPoints];
        for (int i = 0; i < _numPoints; i++)
        {
            Points[i] = Instantiate(_point, transform.position, Quaternion.identity);
        }
        _arrow = _arrowTP;
        BowInstance = this;
        BarraTensado.AjustarMaximo(15f);
        _pDamage.SetActive(false);_pTP.SetActive(false);
    }

    public bool MaxForce = false;
    // Update is called once per frame
    void Update()
    {
        Vector2 bPos = transform.position;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 Dir = mousePos - bPos;
        transform.up = Dir; 
        if (Input.GetMouseButton(0) && _atCD <= 1.5f && _canStillShoot)
        {
            Time.timeScale = 0.5f;
            _point.SetActive(true);
            _atCD += Time.deltaTime * _atackS;
            _force += Time.deltaTime * _atackS*10;
            BowANIM.isBow = true;
            BarraTensado.AjustarDimensiones(_force);
            if(_arrow == _arrowDamage) { _pDamage.SetActive(true); }
            else { _pTP.SetActive(true);}
            for (int i = 0; i < _numPoints; i++)
            {
                Points[i].SetActive(true);
                Points[i].transform.position = PointPosition(0.1f * i, Dir, _force, ShotPoint.transform.position);
            }
        }
        
        else if (_atCD >=1.5f && Input.GetMouseButton(0))
        {
            Time.timeScale = 0.5f;
            MaxForce = true;
            for (int i = 0; i < _numPoints; i++)
            {
                Points[i].transform.position = PointPosition(0.1f * i, Dir, _force, ShotPoint.transform.position);
            }
            if (_arrow == _arrowDamage) { _pDamage.SetActive(true); }
            else { _pTP.SetActive(true); }
        }
        else if (!Input.GetMouseButton(0))
        {
            Time.timeScale = 1;
            if (_atCD > 0) Shoot(Dir, _force);
            for (int i = 0; i < _numPoints; i++)
            {
                Points[i].SetActive(false);
            }
            BowANIM.isBow = false;
            MaxForce = false;
            _canStillShoot = true;
            _atCD = 0;
        }
    }
    public void CambioDeFlecha()
    {
        if(_arrow == _arrowTP)
        {
            _arrow = _arrowDamage;
            _PredictionCoef = 0f;
            _pTP.SetActive(false);
            _multiplier = _arrowDamage.GetComponent<Arrow2>().multiplier;
            BarraTensado.AjustarColor(true);
        }
        else
        {
            _arrow = _arrowTP;
            _PredictionCoef = 0.5f;
            _pDamage.SetActive(false);
            _multiplier = _arrowTP.GetComponent<Arrow>().multiplier;
            BarraTensado.AjustarColor(false);
        }
    }
    public void CancelacionDisparo()
    {
        FinDisparo();
        _canStillShoot = false;
    }
    private void FinDisparo()
    {
        Time.timeScale = 1f;
        _atCD = 0;
        _force = 0;
        BarraTensado.AjustarDimensiones(_force);
        BowANIM.isBow = false;
        _pDamage.SetActive(false); _pTP.SetActive(false);
        for (int i = 0; i < _numPoints; i++)
        {
            Points[i].SetActive(false);
        }
    }
    private void Shoot(in Vector2 Dir, float force)
    {
        if (shotPointDetector.canShootArrow)
        {
            GameObject newArrow = Instantiate(_arrow, ShotPoint.transform.position, ShotPoint.transform.rotation);
            newArrow.GetComponent<Rigidbody2D>().velocity =_multiplier* _force * Dir.normalized;
        }
        FinDisparo();
        
    }
   
    private Vector2 PointPosition(float t, Vector2 Direction, float force, Vector2 bPos)
    {
        Vector2 currentPosition = bPos + (Direction.normalized * force * t) + _PredictionCoef * Physics2D.gravity * (t * t);
        return currentPosition;
    }
}
