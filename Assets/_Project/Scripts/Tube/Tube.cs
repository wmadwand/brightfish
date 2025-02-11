﻿using PathCreation;
using System;
using Terminus.Extensions;
using Terminus.Game.Messages;
using UnityEngine;
using Zenject;

namespace BrightFish
{
	public sealed class Tube : MonoBehaviour
	{
		[SerializeField] private Transform _bubbleSpawnPoint;

		private int _id;
		private float _randomBounceRate;
		private Bubble.BubbleDIFactory _bubbleDIFactory;
		private Food.FoodDIFactory _foodDIFactory;
		private GameSettings _gameSettings;
		private Bubble _bubble;
		private Food _food;

		private float _currentBounceRateStep = 0;
		private TubeSettings _settings;
		private GameObject _path;

		//----------------------------------------------------------------

		public void Init(int id, TubeSettings tubeItem)
		{
			_id = id;
			_settings = tubeItem;

			_path = Instantiate(_settings.pathCreator);
			_path.transform.SetPositionAndRotation(new Vector2(this.transform.position.x, 0), Quaternion.identity);
		}

		public void SelfDestroy()
		{
			Destroy(_bubble.gameObject);
			Destroy(gameObject);
		}

		//----------------------------------------------------------------

		[Inject]
		private void Construct(Bubble.BubbleDIFactory bubbleDIFactory, Food.FoodDIFactory foodDIFactory, GameSettings gameSettings)
		{
			_bubbleDIFactory = bubbleDIFactory;
			_foodDIFactory = foodDIFactory;
			_gameSettings = gameSettings;
		}

		private void Awake()
		{
			MessageBus.OnBubbleDestroy.Receive += Bubble_OnDestroy;
			MessageBus.OnFoodDestroy.Receive += Bubble_OnDestroy;
		}

		private void Start()
		{
			RunAfterDelay(MakeShell);
		}

		private void OnDestroy()
		{
			MessageBus.OnBubbleDestroy.Receive -= Bubble_OnDestroy;
			MessageBus.OnFoodDestroy.Receive -= Bubble_OnDestroy;
		}

		//TODO: consider collision ignore during initialization and skip after trigger exit
		private void OnTriggerEnter2D(Collider2D other)
		{
			if (other is BoxCollider2D && other.GetComponentInParent<Bubble>())
			{
				var bubble = other.GetComponentInParent<Bubble>();

				if (bubble.IsReleased)
				{
					bubble.SelfDestroy(true, true);
				}
			}
			else if (other is BoxCollider2D && other.GetComponentInParent<Food>())
			{
				var food = other.GetComponentInParent<Food>();

				if (food.IsReleased)
				{
					food.SelfDestroy(true, true);
				}
			}
		}

		private void OnTriggerExit2D(Collider2D other)
		{
			if (other is BoxCollider2D && other.GetComponentInParent<Bubble>())
			{
				var bubble = other.GetComponentInParent<Bubble>();
				bubble.SetReleased();

				bubble.GetComponentInChildren<Food>().SetCollidersActive(true);

			}
			else if (other is BoxCollider2D && other.GetComponentInParent<Food>())
			{
				var food = other.GetComponentInParent<Food>();
				food.SetReleased();
			}
		}

		private void Bubble_OnDestroy(int id)
		{
			if (_id != id)
			{
				return;
			}

			RunAfterDelay(MakeShell);
		}

		private void MakeShell()
		{
			MakeFood(true);

			MakeBubble();

			_food.transform.SetParent(_bubble.transform);
			_food.Init(_bubble.GetComponent<Rigidbody2D>());
		}

		private void MakeBubble()
		{
			_bubble = _bubbleDIFactory.Create();
			_bubble.Init(_bubbleSpawnPoint.position, _id, _food, _path.GetComponentInChildren<PathCreator>(), _settings);

			_randomBounceRate = UnityEngine.Random.Range(_settings.bounceRateMin, _settings.bounceRateMax);
			_bubble.AddBounceForce(.2f, (-_randomBounceRate + _currentBounceRateStep), false);

			GameController.Instance.sound.PlaySound(Sounds.tubeProduceBubble);
		}

		private void IncreaseBounceRate()
		{
			_currentBounceRateStep += _settings.bounceRateGrowthStep;
		}

		private void MakeFood(bool asChild)
		{
			_food = _foodDIFactory.Create();

			_food.transform.SetPositionAndRotation(_bubbleSpawnPoint.position, Quaternion.identity);
			_food.SetParentTubeID(_id);

			if (!asChild)
			{
				_randomBounceRate = UnityEngine.Random.Range(_settings.bounceRateMin, _settings.bounceRateMax/* _gameSettings.BubbleInitialBounceRate, _gameSettings.BubbleInitialBounceRate * 1.7f*/);
				_food.AddForce((_randomBounceRate + _currentBounceRateStep) * -1);
			}
		}

		private void RunAfterDelay(Action callback)
		{
			float delay = _settings.bubbleThrowDelay > 0 ? UnityEngine.Random.Range(.5f, _settings.bubbleThrowDelay) : 0;

			this.AfterSeconds(delay, MakeShell);
		}

		//----------------------------------------------------------------

		public class TubeDIFactory : PlaceholderFactory<Tube> { }
	}
}