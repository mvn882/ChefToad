﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{
    //PROPERTIES
    public float Speed { get { return _speed; } set { _speed = value; } }//value is created as magic variable entered by outside

    //FIELDS
    private Vector3 _moveVector = Vector3.zero;
    private CharacterController _characterController;
    public GameObject mainCamera;
    public GameObject Toad;
    [SerializeField]
    private float _speed = 5.0f;
    private float _rotateSpeed = 0.03f;
    private bool _onLadder = false;
    private float _climbspeed = 3.0f;
    private float _slidespeed = 10.0f;
    private bool _onslope = false;
    private Collider _slope;
    Vector3 _ladderDir;
    private int _lives = 2;
    private int _prevlives = 2;
    private bool _falling = false;
    private bool _stopFallMovement = false;
    public Material DirtyToadMaterial;
    public Material cleanToadMaterial;
    public Material CustomToadMaterial;
    public Material CustomDirtyToadMaterial;
    private bool _customshaderOn = false;
    private bool _dirtytoad = false;

    //METHODS
    void Awake()
    {
        _characterController = this.GetComponent<CharacterController>();
        Toad.GetComponent<Animation>()["walk"].speed = 2.0f;
        Toad.GetComponent<Animation>()["climb"].speed = 2.0f;
        Toad.GetComponent<Animation>()["idle"].speed = 2.0f;
    }
    void Update()
    {
        _lives = GetComponent<Death>()._lives;
        //input
        float hInput = Input.GetAxisRaw("Horizontal");
        float vInput = Input.GetAxisRaw("Vertical");

        //scale camera forward to only have x and z axis to prevent character from angle-ing upwards/downwards
        var camforward = Vector3.Scale(mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        //movement when falling/ climbing... or not...
            //set the movevector, relative to the camera direction
            _moveVector = (vInput * camforward + hInput * mainCamera.transform.right);

            //set the speed
            _moveVector = Speed * _moveVector.normalized;

            if (!_onLadder)
            {
                //rotate character in the direction it's moving in
                if (_moveVector != Vector3.zero)
                {
                    //viewvector =
                    Quaternion targetRotation = Quaternion.LookRotation(_moveVector);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.time * _rotateSpeed);
                }
            }



        //ladder
        if (_onLadder )
        {
            Debug.Log("ladder movement active");
            Debug.Log(vInput);
            var inputVector = camforward * vInput + mainCamera.transform.right * hInput;
            inputVector.Normalize();
            if (inputVector.magnitude >0.01f)
            {
                if (!_characterController.isGrounded)
                {
                    if (vInput > 0)
                    {
                        _moveVector = Vector3.up * _climbspeed;
                    }
                    else
                    {
                        _moveVector = Vector3.down * _climbspeed;
                        Toad.GetComponent<Animation>()["climb"].time = -1;
                        transform.rotation = Quaternion.LookRotation(_ladderDir);
                    }

                }
            }
            else
            {
                Toad.GetComponent<Animation>()["climb"].time = 0;
            }
           
        }

        if (_onslope)
        {
            //dirty af but it works,
            var left = -_slope.GetComponent<Transform>().right;
            _moveVector += left * _slidespeed;
        }
        

        //animations
        if (_prevlives != _lives && _prevlives == 2 && _lives ==1)
        {
            StartCoroutine(playanimation());
            _falling = true;
        }
        if (!_falling)
        {
            if (Mathf.Abs(hInput) > 0 || Mathf.Abs(vInput) > 0)
            {
                if (_onLadder)
                {
                    //play climb
                    Toad.GetComponent<Animation>().Play("climb");
                }
                else
                {
                    //play walk
                    Toad.GetComponent<Animation>().Play("walk");
                }
            }
            else if (_onLadder)
            {
                //play climb
                Toad.GetComponent<Animation>().Play("climb");
            }
            else
            {
                //play idle
                Toad.GetComponent<Animation>().Play("idle");
            }
        }

        if (_falling && !_stopFallMovement)
        {
            _moveVector = (-transform.forward);

            //set the speed
            _moveVector =  2 * _moveVector.normalized;
        }

        //Gravity
        _moveVector += Physics.gravity;// * Time.deltaTime;//makes it fall too darn slowly

        //pass movement to char controller
        _characterController.Move(_moveVector * Time.deltaTime);

        _onLadder = false;
        _onslope = false;
        Debug.Log( _lives);
        if (_lives == 2 && _prevlives !=2)
        {
            _dirtytoad = false;
            if (_customshaderOn)
            {
                transform.GetChild(0).transform.GetChild(0).transform.GetComponent<Renderer>().material = CustomToadMaterial;
            }
            else
            {
                transform.GetChild(0).transform.GetChild(0).transform.GetComponent<Renderer>().material = cleanToadMaterial;
            }

        }
        if (_lives == 1 && _prevlives == 2)
        {
            _dirtytoad = true;
            if (_customshaderOn)
            {
                transform.GetChild(0).transform.GetChild(0).transform.GetComponent<Renderer>().material = CustomDirtyToadMaterial;
            }
            else
            {
                transform.GetChild(0).transform.GetChild(0).transform.GetComponent<Renderer>().material = DirtyToadMaterial;
            }
        }

        if (Input.GetButtonDown("Switch"))
        {
            SwitchMaterial();
        }
        _prevlives = GetComponent<Death>()._lives;
    }

    void OnTriggerStay(Collider other)
    {
        Debug.Log("triggered");
        //ladder
        if (other.tag == "Ladder")
        {
            Debug.Log("enterladder");
            _onLadder = true;

            //calculate vector from player to ladder.
            _ladderDir = new Vector3(other.transform.position.x - transform.position.x, 0, other.transform.position.z - transform.position.z);
            _ladderDir = _ladderDir.normalized;

            //obviously not the best way but itll do for now
            Physics.gravity = Vector3.zero;
        }
        if (other.tag == "Slope")
        {
            _onslope = true;
            Debug.Log("Slope");
            _slope = other;

        }
    }

    void OnTriggerExit(Collider other)
    {
        Physics.gravity = new Vector3 ( 0, -9.81f,0);
    }

    private IEnumerator playanimation()
    {   
        Toad.GetComponent<Animation>()["Fall"].time = 0;
        Toad.GetComponent<Animation>().Play("Fall");
        yield return new WaitForSeconds(0.5f);
        _stopFallMovement = true;
        yield return new WaitForSeconds(0.5f);
        _stopFallMovement = false;
        _falling = false;
    }

    public void SwitchMaterial()
    {
        _customshaderOn = !_customshaderOn;
        if (_customshaderOn)
        {
            if (_dirtytoad)
                transform.GetChild(0).transform.GetChild(0).transform.GetComponent<Renderer>().material = CustomDirtyToadMaterial;
            else
            transform.GetChild(0).transform.GetChild(0).transform.GetComponent<Renderer>().material = CustomToadMaterial;
        }
        else
        {
            if (_dirtytoad)
                transform.GetChild(0).transform.GetChild(0).transform.GetComponent<Renderer>().material = DirtyToadMaterial;
            else
                transform.GetChild(0).transform.GetChild(0).transform.GetComponent<Renderer>().material = cleanToadMaterial;
        }
    }
}