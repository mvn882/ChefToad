﻿using UnityEngine;
using System.Collections;

public class Death : MonoBehaviour
{
    public Transform TransSpawnpoint;
    public Transform GameController;
    public GameObject SoundDamage;
    public int _toadLives = 2;
    public int _lives;

    void Start()
    {
        _lives = _toadLives;
    }

    void Update()
    {
        if (_lives == 0)
        {
            GetComponent<Transform>().position = TransSpawnpoint.position;
            _lives = _toadLives;
            GameController.GetComponent<GameController>().LifeDown();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            _lives--;
            transform.GetComponent<AudioSource>().Play();
        }

        if (other.gameObject.tag == "Water")
        {
            GetComponent<Transform>().position = TransSpawnpoint.position;
            _lives = _toadLives;
            GameController.GetComponent<GameController>().LifeDown();
            transform.GetComponent<AudioSource>().Play();
        }
    }
}
