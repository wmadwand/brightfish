﻿using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class Fish : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
	public static event Action<int> OnBubbleColorMatch;
	public static event Action<BubbleType, Vector3> OnDeath;
	public static event Action<BubbleType, Vector3> OnHappy;

	[SerializeField] private Sprite[] _sprites;
	[SerializeField] private Sounds feedFishGood, feedFishBad, fishDead, fishHappy;
	[SerializeField] private GameObject _enemyHealthBarPref;
	[SerializeField] private Transform _healthbarPoint;
	[SerializeField] private Transform _scoreTextSpawnPoint;

	private FishHealthBar _healthBar;
	private BubbleType _type;
	private bool _isDead;
	private bool _isCollided;
	private Color _color;
	private SpriteRenderer _spriteRenderer;
	private Vector2 _originPosition;
	private bool isDraggable;
	private FishHealth _fishHealth;
	private GameSettings _gameSettings;

	//----------------------------------------------------------------

	public void Setup(BubbleType bubbleType)
	{
		_type = bubbleType;

		switch (bubbleType)
		{
			case BubbleType.A: _color = _gameSettings.colorA; break;
			case BubbleType.B: _color = _gameSettings.colorB; break;
			case BubbleType.C: _color = _gameSettings.colorC; break;
		}

		_spriteRenderer.material.color = _color;
	}

	public void ChangePosition(Vector3 value)
	{
		gameObject.transform.position = value;
	}

	public void UpdateHealthBar(int value)
	{
		_healthBar.UpdateState(value);
	}

	public void Destroy()
	{
		Destroy(_healthBar.gameObject);
		Destroy(gameObject);
	}

	//----------------------------------------------------------------

	[Inject]
	private void Construct(GameSettings gameSettings)
	{
		_gameSettings = gameSettings;
	}

	private void Awake()
	{
		_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		_fishHealth = GetComponent<FishHealth>();
	}

	private void Start()
	{
		_healthBar = Instantiate(_enemyHealthBarPref, GameController.Instance.canvas.transform).GetComponent<FishHealthBar>();
		_healthBar.Init(_healthbarPoint);

		UpdateHealthBar(_fishHealth.value);
	}

	private void Update()
	{
		if (_isDead)
		{
			return;
		}

		if (_fishHealth.IsDead)
		{
			GameController.Instance.sound.PlaySound(fishDead);			
			_isDead = true;			

			SOmeDelayBeforeHide(() =>
			{
				OnDeath?.Invoke(_type, transform.position);
				Destroy();
			});			
		}
		else if (_fishHealth.IsFedup)
		{
			_isDead = true;

			SOmeDelayBeforeHide(() =>
			{
				OnHappy?.Invoke(_type, transform.position);
				Destroy();
			});
		}

		UpdateSprite();

	}

	private async void SOmeDelayBeforeHide(Action callback)
	{
		await Task.Delay(TimeSpan.FromSeconds(1));

		_spriteRenderer.DOFade(0, 1);
		_healthBar.GetComponent<CanvasGroup>().DOFade(0, 1);

		await Task.Delay(TimeSpan.FromSeconds(1));

		callback();
	}

	private void UpdateSprite()
	{
		if (_fishHealth.IsFedup)
		{
			_spriteRenderer.sprite = _sprites[1];
		}
		else if (_fishHealth.IsDead)
		{
			_spriteRenderer.sprite = _sprites[2];
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.GetComponent<Bubble>())
		{
			if (other.GetComponent<Bubble>() && other.GetComponent<Bubble>().Type == _type)
			{
				OnBubbleColorMatch?.Invoke(other.GetComponent<Bubble>().ScoreCount);

				_fishHealth.ChangeHealth(30);
				UpdateHealthBar(_fishHealth.value);

				SpawnCoinScroreText(other.GetComponent<Bubble>().ScoreCount);

				GameController.Instance.sound.PlaySound(feedFishGood);
			}
			else if (other.GetComponent<Bubble>() && other.GetComponent<Bubble>().Type != _type)
			{
				OnBubbleColorMatch?.Invoke(-other.GetComponent<Bubble>().ScoreCount);

				SpawnCoinScroreText(other.GetComponent<Bubble>().ScoreCount, true);

				_fishHealth.ChangeHealth(-30);
				UpdateHealthBar(_fishHealth.value);

				GameController.Instance.sound.PlaySound(feedFishBad);
			}

			other.GetComponent<Bubble>().SelfDestroy();
		}
		else if (other.GetComponent<Fish>())
		{
			if (!isDraggable)
			{
				return;
			}

			_isCollided = true;

			//isDraggable = false;
			transform.position = other.GetComponent<Fish>().transform.position;
			other.GetComponent<Fish>().transform.position = _originPosition;
			_originPosition = transform.position;
		}
	}

	private void OnTriggerStay(Collider other)
	{
		//if
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (!other.GetComponent<Bubble>())
		{
			return;
		}

		other.GetComponent<Bubble>().SelfDestroy();
	}

	private void SpawnCoinScroreText(int scoreCount, bool wrongCoin = false)
	{
		Vector3 pos = Camera.main.WorldToScreenPoint(_scoreTextSpawnPoint.position);

		var scoreGO = Instantiate(GameController.Instance.coinScoreTextPref, GameController.Instance.canvas.transform);
		scoreGO.transform.position = pos;

		scoreGO.GetComponent<CoinScoreText>().SetScore(scoreCount, wrongCoin);
	}

	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
	{
		_isCollided = false;
		isDraggable = true;
		_originPosition = transform.position;
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		if (!isDraggable)
		{
			return;
		}

		if (_isCollided)
		{
			_isCollided = false;
		}

		//return;

		////Very nice approach for 2D objects dragging
		//transform.position = eventData.position;


		// Solution #01
		Plane plane = new Plane(Vector3.forward, transform.position);
		Ray ray = eventData.pressEventCamera.ScreenPointToRay(eventData.position);

		if (plane.Raycast(ray, out float distamce))
		{
			var vec = ray.origin + ray.direction * distamce;
			transform.position = new Vector2(vec.x, transform.position.y);
		}

		// Solution #02
		//Ray R = Camera.main.ScreenPointToRay(Input.mousePosition); // Get the ray from mouse position
		//Vector3 PO = transform.position; // Take current position of this draggable object as Plane's Origin
		//Vector3 PN = -Camera.main.transform.forward; // Take current negative camera's forward as Plane's Normal
		//float t = Vector3.Dot(PO - R.origin, PN) / Vector3.Dot(R.direction, PN); // plane vs. line intersection in algebric form. It find t as distance from the camera of the new point in the ray's direction.
		//Vector3 P = R.origin + R.direction * t; // Find the new point.

		//transform.position = P;
	}

	void IEndDragHandler.OnEndDrag(PointerEventData eventData)
	{
		if (!_isCollided)
		{
			transform.position = _originPosition;
		}

		isDraggable = false;
	}

	//----------------------------------------------------------------

	public class FishDIFactory : PlaceholderFactory<Fish> { }
}
