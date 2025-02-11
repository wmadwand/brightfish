﻿using UnityEngine;
using UnityEngine.EventSystems;

namespace BrightFish
{
	public class FishMovement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
	{
		public bool _isCollided;
		public Vector2 _originPosition;
		public bool _isDraggable;
		private Transform _parent;

		private Vector2 _currentPointerPos;
		private Vector3 _offset;

		private void Awake()
		{
			_parent = transform.root;
		}

		void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
		{
			_currentPointerPos = Camera.main.ScreenToWorldPoint(eventData.position);
			_offset = (Vector3)_currentPointerPos - transform.position;

			_isCollided = false;
			_isDraggable = true;
			_originPosition = _parent.position;
		}

		void IDragHandler.OnDrag(PointerEventData eventData)
		{
			if (!_isDraggable)
			{
				return;
			}

			if (_isCollided)
			{
				_isCollided = false;
			}

			// The best solution with the offset so as not to center fish view to the pointer position
			//
			var spawnPointsLength = GameController.Instance.fishSpawner.SpawnPoints.Length;
			var xMin = GameController.Instance.fishSpawner.SpawnPoints[0].x;
			var xMax = GameController.Instance.fishSpawner.SpawnPoints[spawnPointsLength - 1].x;

			var resultPos = Camera.main.ScreenToWorldPoint(eventData.position) - _offset;
			_parent.position = new Vector2(Mathf.Clamp(resultPos.x, xMin, xMax), _parent.position.y);

			#region Alternative drag solutions
			////Very nice approach for 2D objects dragging
			//transform.position = eventData.position;

			// Solution #01
			//Plane plane = new Plane(Vector3.forward, _parent.position);
			//Ray ray = eventData.pressEventCamera.ScreenPointToRay(eventData.position);

			//if (plane.Raycast(ray, out float distance))
			//{
			//	var vec = ray.origin + ray.direction * distance;

			//	var spawnPointsLength = GameController.Instance.fishSpawner.SpawnPoints.Length;

			//	var xMin = GameController.Instance.fishSpawner.SpawnPoints[0].x;
			//	var xMax = GameController.Instance.fishSpawner.SpawnPoints[spawnPointsLength - 1].x;
			//	_parent.position = new Vector2(Mathf.Clamp(vec.x, xMin, xMax), _parent.position.y);
			//}

			// Solution #02
			//Ray R = Camera.main.ScreenPointToRay(Input.mousePosition); // Get the ray from mouse position
			//Vector3 PO = transform.position; // Take current position of this draggable object as Plane's Origin
			//Vector3 PN = -Camera.main.transform.forward; // Take current negative camera's forward as Plane's Normal
			//float t = Vector3.Dot(PO - R.origin, PN) / Vector3.Dot(R.direction, PN); // plane vs. line intersection in algebric form. It find t as distance from the camera of the new point in the ray's direction.
			//Vector3 P = R.origin + R.direction * t; // Find the new point.

			//transform.position = P; 
			#endregion
		}

		void IEndDragHandler.OnEndDrag(PointerEventData eventData)
		{
			if (!_isCollided)
			{
				_parent.position = _originPosition;
			}

			_isDraggable = false;

			GameController.Instance.sound.PlaySound(Sounds.fishSwap);
		}
	}
}