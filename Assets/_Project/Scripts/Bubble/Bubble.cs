﻿using System;
using System.Collections;
using Terminus.Game.Messages;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class Bubble : MonoBehaviour
{
	public BubbleType Type { get; private set; }
	public bool IsReleased { get; private set; }

	//TODO: move to separate class
	public int ScoreCount
	{
		get
		{
			int count = 0;

			switch (_state)
			{
				case BubbleState.Small:
					count = 50;
					break;
				case BubbleState.Medium:
					count = 100;
					break;
				case BubbleState.Big:
					count = 200;
					break;
				default:
					break;
			}

			return count;
		}
	}

	//TODO: move to separate class
	[SerializeField] private Sounds soundName01, explosionSound;

	[SerializeField] private float _spoeedReflection;

	[SerializeField] private GameObject _explosion;

	private int _parentTubeID;
	private BubbleState _state;
	private int _clickCount;
	private bool _selfDestroyStarted;

	private Color _color;
	private Renderer _renderer;
	private Rigidbody2D _rigidbody2D;
	private GameSettings _gameSettings;
	private GameObject _view;
	private float _selfDestroyTimeRate;

	private Food _childFood;

	//----------------------------------------------------------------

	public void SetParentTubeID(int value, Food childFood)
	{
		_parentTubeID = value;
		_childFood = childFood;
	}

	public void AddForce(float value)
	{
		_rigidbody2D.AddForce(Vector3.up * value, ForceMode2D.Impulse);
	}

	public void SetReleased()
	{
		IsReleased = true;
	}

	public void SelfDestroy(bool isReqiredExplosion = false, bool isRequiredBadSound = false)
	{
		if (isRequiredBadSound)
		{
			GameController.Instance.sound.PlaySound(explosionSound);
		}

		if (isReqiredExplosion)
		{
			SpawnExplosion();
		}

		MessageBus.OnBubbleDestroy.Send(_parentTubeID);

		Destroy(gameObject);
	}

	private void SpawnExplosion()
	{
		var go = Instantiate(_explosion, transform.position, Quaternion.identity);

		//Vector3 vec = new Vector3(transform.position.x, transform.position.y, -1);		

		//go.transform.SetPositionAndRotation(vec, Quaternion.identity);
	}

	//----------------------------------------------------------------

	[Inject]
	private void Construct(GameSettings gameSettings)
	{
		_gameSettings = gameSettings;
	}

	private void Awake()
	{
		_renderer = GetComponentInChildren<Renderer>();
		_rigidbody2D = GetComponentInChildren<Rigidbody2D>();
		_view = _renderer.gameObject;

		_selfDestroyTimeRate = _gameSettings.SelfDestroyTime;

		Init();
	}

	private void Update()
	{
		if (_selfDestroyStarted)
		{
			_selfDestroyTimeRate -= Time.fixedDeltaTime;

			if (_selfDestroyTimeRate <= 0)
			{
				SelfDestroy();
			}
		}
	}

	private void FixedUpdate()
	{
		//GetComponent<Rigidbody2D>().velocity = transform.up * (GameController.Instance.gameSettings.moveUpSpeed /** _baseSpeedTimer*/) * Time.deltaTime;
		transform.Translate(-transform.up * (_gameSettings.BubbleMoveSpeed /** _baseSpeedTimer*/) * 0.1f * Time.deltaTime);
	}

	public void OnClick()
	{
		if (_selfDestroyStarted)
		{
			return;
		}

		if (_clickCount >= _gameSettings.EnlargeSizeClickCount * 2 && !_gameSettings.DestroyBigBubbleClick)
		{
			return;
		}

		GameController.Instance.sound.PlaySound(soundName01);

		_clickCount++;

		AddForce(_gameSettings.BounceRate);

		//Enlarge();
		Diffuse();

		Debug.Log("click");
	}

	private void Init()
	{
		IsReleased = false;

		_rigidbody2D.drag = _gameSettings.DragRate;

		var spawnPointsLength = GameController.Instance.fishSpawner.SpawnPoints.Length;

		Type = (BubbleType)UnityEngine.Random.Range(0, spawnPointsLength);
		_state = BubbleState.Small;

		_renderer.material.color = _gameSettings.ColorDummy;
		_renderer.material.color = new Color(_renderer.material.color.a, _renderer.material.color.g, _renderer.material.color.b, .85f);

		SetColor(Type);

		if (_gameSettings.ColorMode == BubbleColorMode.Explicit)
		{
			_renderer.material.color = _color;
		}
	}

	private void SetColor(BubbleType bubbleType)
	{
		switch (bubbleType)
		{
			case BubbleType.A: _color = _gameSettings.ColorA; break;
			case BubbleType.B: _color = _gameSettings.ColorB; break;
			case BubbleType.C: _color = _gameSettings.ColorC; break;
			case BubbleType.D: _color = _gameSettings.ColorD; break;
			case BubbleType.E: _color = _gameSettings.ColorE; break;
		}
	}

	private void Enlarge()
	{
		if (_clickCount == _gameSettings.EnlargeSizeClickCount)
		{
			_view.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

			_state = BubbleState.Medium;
		}
		else if (_clickCount == _gameSettings.EnlargeSizeClickCount * 2)
		{
			_view.transform.localScale = new Vector3(.7f, .7f, .7f);

			_state = BubbleState.Big;

			if (_gameSettings.BigBubbleSelfDestroy)
			{
				_selfDestroyStarted = true;
			}

			_renderer.material.color = _color;

			if (_selfDestroyStarted)
			{
				StartCoroutine(BlinkRoutine());
			}
		}
		else if (_gameSettings.DestroyBigBubbleClick && _clickCount > _gameSettings.EnlargeSizeClickCount * 2)
		{
			SelfDestroy();
		}
	}

	private void Diffuse()
	{
		if (_clickCount == _gameSettings.EnlargeSizeClickCount)
		{
			var size = _gameSettings.ClickEnlargeSizePairs[0].sizeRate;
			_view.transform.localScale = new Vector3(size, size, size);

			_renderer.material.color = new Color(_renderer.material.color.r, _renderer.material.color.g, _renderer.material.color.b, .7f);

			_state = BubbleState.Medium;
		}
		else if (_clickCount == _gameSettings.EnlargeSizeClickCount * 2)
		{
			var size = _gameSettings.ClickEnlargeSizePairs[1].sizeRate;
			_view.transform.localScale = new Vector3(size, size, size);

			_renderer.material.color = new Color(_renderer.material.color.r, _renderer.material.color.g, _renderer.material.color.b, .0f);

			_state = BubbleState.Big;


			_childFood.RevealColor();
			Type = _childFood.Type;


			if (_gameSettings.BigBubbleSelfDestroy)
			{
				_selfDestroyStarted = true;
			}

			if (_selfDestroyStarted)
			{
				StartCoroutine(BlinkRoutine());
			}
		}
		else if (_gameSettings.DestroyBigBubbleClick && _clickCount > _gameSettings.EnlargeSizeClickCount * 2)
		{
			SelfDestroy();
		}
	}

	private IEnumerator BlinkRoutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(_gameSettings.BlinkRate);

			_renderer.material.color = new Color(_color.r, _color.g, _color.b, 0);

			yield return new WaitForSeconds(_gameSettings.BlinkRate);

			_renderer.material.color = new Color(_color.r, _color.g, _color.b, 100);
		}
	}

	public void AddForceDirection(Vector2 _dir/*, float _speed*/)
	{
		_dir.Normalize();
		_rigidbody2D.AddForce(_dir * _spoeedReflection, ForceMode2D.Impulse);
	}

	//----------------------------------------------------------------

	public class BubbleDIFactory : PlaceholderFactory<Bubble> { }
}