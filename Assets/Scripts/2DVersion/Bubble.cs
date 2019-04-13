﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bubble : MonoBehaviour, IPointerClickHandler, IDragHandler
{
	public static event Action<int> OnDestroy;

	public int ScoreCount
	{
		get
		{
			int count = 0;

			switch (_state)
			{
				case CoinState.Small:
					count = 50;
					break;
				case CoinState.Medium:
					count = 100;
					break;
				case CoinState.Big:
					count = 200;
					break;
				default:
					break;
			}

			return count;
		}
	}

	private CoinState _state;

	public float bounceRate = 20;
	public float blinkRate = 0.15f;

	public int tubeId;

	public CoinType type;

	Color ColorDummy = Color.white;
	Color ColorA = Color.blue;
	Color ColorB = Color.yellow;
	Color ColorC = Color.green;

	int _clickCount;
	bool _startSelfDestroy;
	float _countdownRate = 4;
	Color _color;

	public bool IsReleased { get; private set; }

	private Renderer _renderer;

	private void Update()
	{
		if (_startSelfDestroy)
		{
			_countdownRate -= Time.fixedDeltaTime;

			if (_countdownRate <= 0)
			{
				SelfDestroy();
			}
		}
	}

	//private void OnMouseDown()
	//{
	//	Debug.Log("sdf");
	//}


	private void Awake()
	{
		_renderer = GetComponent<Renderer>();

		Init();
	}

	private void Start()
	{
		//StartCoroutine(BlinkRoutine());
	}

	public void SetReleased()
	{
		IsReleased = true;
	}



	void Init()
	{
		IsReleased = false;

		bounceRate = GameController.Instance.gameSettings.bounceRate;
		GetComponent<Rigidbody2D>().drag = GameController.Instance.gameSettings.dragRate;


		type = (CoinType)UnityEngine.Random.Range(0, 3);
		_state = CoinState.Small;

		_renderer.material.color = ColorDummy;

		switch (type)
		{
			case CoinType.A:
				_color = ColorA;
				break;
			case CoinType.B:
				_color = ColorB;
				break;
			case CoinType.C:
				_color = ColorC;
				break;
			default:
				break;
		}

		if (GameController.Instance.gameSettings.colorMode == ColorMode.Explicit)
		{
			_renderer.material.color = _color;
		}

		//_renderer.material.color = _color;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (_startSelfDestroy)
		{
			return;
		}

		_clickCount++;

		GetComponent<Rigidbody2D>().AddForce(Vector3.up * bounceRate, ForceMode2D.Impulse);

		Enlarge();

		Debug.Log("click");
	}

	public void OnDrag(PointerEventData eventData)
	{
		////Very nice approach for 2D objects dragging
		//transform.position = eventData.position;


		// Solution #01
		Plane plane = new Plane(Vector3.forward, transform.position);
		Ray ray = eventData.pressEventCamera.ScreenPointToRay(eventData.position);

		if (plane.Raycast(ray, out float distamce))
		{
			transform.position = ray.origin + ray.direction * distamce;
		}

		// Solution #02
		//Ray R = Camera.main.ScreenPointToRay(Input.mousePosition); // Get the ray from mouse position
		//Vector3 PO = transform.position; // Take current position of this draggable object as Plane's Origin
		//Vector3 PN = -Camera.main.transform.forward; // Take current negative camera's forward as Plane's Normal
		//float t = Vector3.Dot(PO - R.origin, PN) / Vector3.Dot(R.direction, PN); // plane vs. line intersection in algebric form. It find t as distance from the camera of the new point in the ray's direction.
		//Vector3 P = R.origin + R.direction * t; // Find the new point.

		//transform.position = P;
	}

	void Enlarge()
	{
		if (_clickCount == GameController.Instance.gameSettings.enlargeSizeClickCount)
		{
			transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

			_state = CoinState.Medium;
		}
		else if (_clickCount == GameController.Instance.gameSettings.enlargeSizeClickCount * 2)
		{
			transform.localScale = new Vector3(.7f, .7f, .7f);

			_state = CoinState.Big;

			_startSelfDestroy = true;

			_renderer.material.color = _color;

			StartCoroutine(BlinkRoutine());
		}
	}

	public void SelfDestroy()
	{
		OnDestroy?.Invoke(tubeId);

		Destroy(gameObject);
	}

	void Blink()
	{

	}

	IEnumerator BlinkRoutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(blinkRate);


			_renderer.material.color = new Color(_color.r, _color.g, _color.b, 0);

			yield return new WaitForSeconds(blinkRate);

			_renderer.material.color = new Color(_color.r, _color.g, _color.b, 100);
		}
	}

}
