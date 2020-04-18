using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

namespace DestroyableObject
{

	//KAKAAAAAAAA
	[Serializable] public class DestroyableListEvent : UnityEvent<List<Destroyable>> {}
	
	[Serializable] public class DestroyableCollision2DEvent : UnityEvent<Collision2D> {}
	
	[Serializable] public class DestroyableFloatFloatEvent : UnityEvent<float, float> {}
	
	[Serializable] public class DestroyableVector2Event : UnityEvent<Vector2> {}
	
	[Serializable] public class DestroyableRectEvent : UnityEvent<DestroyableRect> {}
	
	[Serializable] public class DestroyableEvent : UnityEvent {}
}