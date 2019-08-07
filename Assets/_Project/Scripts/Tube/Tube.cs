﻿using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

using Terminus.Extensions;
using Terminus.Game.Messages;

namespace BrightFish
{
	public sealed class Tube : MonoBehaviour
	{
		//[SerializeField] private GameObject _bubblePrefab;
		[SerializeField] private Transform _bubbleSpawnPoint;

		private int _id;
		private float _randomBounceRate;
		private Bubble.BubbleDIFactory _bubbleDIFactory;
		private Food.FoodDIFactory _foodDIFactory;
		private GameSettings _gameSettings;
		private Bubble _bubble;
		private Food _food;

		//----------------------------------------------------------------

		public void SetTubeID(int value)
		{
			_id = value;
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
			MakeShell();
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

			MakeShell();
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

			_bubble.transform.SetPositionAndRotation(_bubbleSpawnPoint.position, Quaternion.identity);
			_bubble.SetParentTubeID(_id, _food);

			_randomBounceRate = UnityEngine.Random.Range(_gameSettings.BubbleInitialBounceRate, _gameSettings.BubbleInitialBounceRate * 1.7f);
			_bubble.AddForce(_randomBounceRate);
		}

		private void MakeFood(bool asChild)
		{
			_food = _foodDIFactory.Create();

			//_food.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
			//_food.GetComponent<Rigidbody2D>().Sleep();
			////_food.GetComponent<Rigidbody2D>().simulated = false;

			_food.transform.SetPositionAndRotation(_bubbleSpawnPoint.position, Quaternion.identity);
			_food.SetParentTubeID(_id);

			if (!asChild)
			{
				_randomBounceRate = UnityEngine.Random.Range(_gameSettings.BubbleInitialBounceRate, _gameSettings.BubbleInitialBounceRate * 1.7f);
				_food.AddForce(_randomBounceRate);
			}
		}

		private void RunAfterDelay(Action callback)
		{
			float delayRate = _gameSettings.TubeBubbleThrowDelay ? UnityEngine.Random.Range(.5f, 1.5f) : 0;

			this.AfterSeconds(delayRate, MakeShell);
		}

		//----------------------------------------------------------------

		public class TubeDIFactory : PlaceholderFactory<Tube> { }
	} 
}