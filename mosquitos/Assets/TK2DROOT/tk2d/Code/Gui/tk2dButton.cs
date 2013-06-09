using UnityEngine;
using System.Collections;

public class tk2dTouch {
	public TouchPhase phase;
	public Vector2 position;
	public Vector2 deltaPosition;
	
	public tk2dTouch(Vector2 pos) {
		phase = TouchPhase.Began;
		position = pos;
		deltaPosition = Vector2.zero;
	}
	
	public void UpdatePosition(Vector2 pos) {
		deltaPosition = pos - position;
		position = pos;
	}
}

[AddComponentMenu("2D Toolkit/GUI/tk2dButton")]
/// <summary>
/// Simple gui button
/// </summary>
public class tk2dButton : MonoBehaviour 
{
	public tk2dSprite _childSpriteForScale;
	Vector3 _childSpriteDefaultScale;
	
	/// <summary>
	/// The camera this button is meant to be viewed from.
	/// Set this explicitly for best performance.\n
	/// The system will automatically traverse up the hierarchy to find a camera if this is not set.\n
	/// If nothing is found, it will fall back to the active <see cref="tk2dCamera"/>.\n
	/// Failing that, it will use Camera.main.
	/// </summary>
	public Camera viewCamera;
	
	// Button Up = normal state
	// Button Down = held down
	// Button Pressed = after it is pressed and activated
	
	/// <summary>
	/// The button down sprite. This is resolved by name from the sprite collection of the sprite component.
	/// </summary>
	public string buttonDownSprite = "button_down";
	/// <summary>
	/// The button up sprite. This is resolved by name from the sprite collection of the sprite component.
	/// </summary>
	public string buttonUpSprite = "button_up";
	/// <summary>
	/// The button pressed sprite. This is resolved by name from the sprite collection of the sprite component.
	/// </summary>
	public string buttonPressedSprite = "button_up";
	
	int buttonDownSpriteId = -1, buttonUpSpriteId = -1; //, buttonPressedSpriteId = -1;
	
	public AudioPlayer.SoundType buttonSound = AudioPlayer.SoundType.UI_ButtonClick;
	public AudioPlayer.SoundType savedSound = AudioPlayer.SoundType.UI_ButtonClick;
	
	/// <summary>
	/// if you can click and drag this button around, this should be enabled
	/// </summary>
	public bool isDraggable;
	
	/// <summary>
	/// If you can swipe your finger to cause a button start hit (then release your finger while on the button to count as a hit)
	/// </summary>
	public bool isSwipeButton;
	
	/// <summary>
	/// The touch
	/// </summary>
	public tk2dTouch _touch;
	
	// Delegates
	
	/// <summary>
	/// Button event handler delegate.
	/// </summary>
	public delegate void ButtonHandlerDelegate(tk2dButton source);
	
	public event System.Action onEnableCallback;
	
	// Messaging
	
	/// <summary>
	/// Target object to send the message to. The event methods below are significantly more efficient.
	/// </summary>
	public GameObject targetObject = null;
	/// <summary>
	/// The message to send to the object. This should be the name of the method which needs to be called.
	/// </summary>
    public string messageName = "";
	
	/// <summary>
	/// Occurs when button is pressed (tapped, and finger lifted while inside the button)
	/// </summary>
	public event ButtonHandlerDelegate ButtonPressedEvent;
	
	/// <summary>
	/// Occurs every frame for as long as the button is held down.
	/// </summary>
	public event ButtonHandlerDelegate ButtonAutoFireEvent;
	/// <summary>
	/// Occurs when button transition from up to down state
	/// </summary>
	public event ButtonHandlerDelegate ButtonDownEvent;
	/// <summary>
	/// Occurs when button transitions from down to up state
	/// </summary>
	public event ButtonHandlerDelegate ButtonUpEvent;
	
	tk2dBaseSprite sprite;
	bool buttonDown = false;
	
	/// <summary>
	/// How much to scale the sprite when the button is in the down state
	/// </summary>
	public float targetScale = .95f;
	public bool doTargetPos = false;
	public Vector3 targetMovement = Vector3.zero;
	Vector3 currentMovement = Vector3.zero;
	/// <summary>
	/// The length of time the scale operation takes
	/// </summary>
	public float scaleTime = 0.05f;
	/// <summary>
	/// How long to wait before allowing the button to be pressed again, in seconds.
	/// </summary>
	public float pressedWaitTime = 0.3f;
	
	void OnEnable()
	{
		buttonDown = false;
		if (onEnableCallback != null) {
			onEnableCallback();
		}
	}
	
	// Use this for initialization
	void Start () 
	{
		if (_childSpriteForScale != null) {
			_childSpriteDefaultScale = _childSpriteForScale.scale;
		}
		
		if (viewCamera == null)
		{
			// Find a camera parent 
            Transform node = transform;
            while (node && node.camera == null)
            {
                node = node.parent;
            }
            if (node && node.camera != null) 
			{
				viewCamera = node.camera;
			}
			
			// See if a tk2dCamera exists
			if (viewCamera == null && tk2dCamera.inst)
			{
				viewCamera = tk2dCamera.inst.mainCamera;
			}
			
			// ...otherwise, use the main camera
			if (viewCamera == null)
			{
				viewCamera = Camera.main;
			}
		}
		
		sprite = GetComponent<tk2dBaseSprite>();
		
		// Further tests for sprite not being null aren't necessary, as the IDs will default to -1 in that case. Testing them will be sufficient
		if (sprite)
		{
			// Change this to use animated sprites if necessary
			// Same concept here, lookup Ids and call Play(xxx) instead of .spriteId = xxx
			UpdateSpriteIds();
		}
		
		if (collider == null)
		{
			//Debug.Log("creating collider on " + gameObject.name);
			BoxCollider newCollider = gameObject.AddComponent<BoxCollider>();
			Vector3 colliderExtents = newCollider.extents;
			colliderExtents.z = 0.2f;
			newCollider.extents = colliderExtents;
		}
	}
	
	/// <summary>
	/// Call this when the sprite names have changed
	/// </summary>
	public void UpdateSpriteIds()
	{
		buttonDownSpriteId 		= (buttonDownSprite.Length > 0)?sprite.GetSpriteIdByName(buttonDownSprite):-1;
		buttonUpSpriteId 		= (buttonUpSprite.Length > 0)?sprite.GetSpriteIdByName(buttonUpSprite):-1;
		//buttonPressedSpriteId 	= (buttonPressedSprite.Length > 0)?sprite.GetSpriteIdByName(buttonPressedSprite):-1;
	}
	
	public void UpdateSpriteIds(int spriteId, string spriteName) {
		buttonDownSpriteId = spriteId;
		buttonUpSpriteId = spriteId;
		//buttonPressedSpriteId = spriteId;
		
		buttonDownSprite = spriteName;
		buttonUpSprite = spriteName;
		buttonPressedSprite = spriteName;
	}
	
	public void PlaySound()
	{
		if (buttonSound != AudioPlayer.SoundType.None)
		{
			AudioPlayer.instance.PlayFirstSound(1f, buttonSound);
			//Debug.Log(gameObject);
		}
	}
	
	public void PlaySound(AudioPlayer.SoundType soundType)
	{
		if (soundType != AudioPlayer.SoundType.None)
		{
			AudioPlayer.instance.PlayFirstSound(1f, soundType);
			//Debug.Log(gameObject);
		}
	}
	
	Transform _t;
	bool isMoving;
	bool stopMoving;
	IEnumerator coMove(bool towardsTarget) {
		if (isMoving) {
			stopMoving = true;
			yield return 1;
		}
		
		isMoving = true;
		stopMoving = false;
		float time = 0f;
		float t = time / scaleTime;
		Vector3 startMovement, endMovement;
		_t = transform;
		Transform childT = null;
		if (_childSpriteForScale != null) {
			childT = _childSpriteForScale.transform;
		}
		if (towardsTarget) {
			startMovement = Vector3.zero;
			endMovement = targetMovement;
		} else {
			startMovement = currentMovement;
			endMovement = Vector3.zero;
		}
		Vector3 lastMovement = startMovement;
		while (t <= 1f && stopMoving == false) {
			time += Time.deltaTime;
			t = time / scaleTime;
			Vector3 thisMovement = Vector3.Lerp(startMovement, endMovement, t);
			Vector3 diffMovement = thisMovement - lastMovement;
			_t.Translate(diffMovement, Space.World);
			if (childT != null) {
				childT.Translate(diffMovement, Space.World);
			}
			currentMovement += diffMovement;
			lastMovement = thisMovement;
			yield return 1;
		}
		//Debug.Log(currentMovement);
		isMoving = false;
	}
	
	bool isScaling;
	float masterEndScale;
	float masterStartScale;
	float masterTime;
	Vector3 masterDefaultScale;
	
	IEnumerator coScale(Vector3 defaultScale, float startScale, float endScale)
    {
		masterStartScale = startScale;
		masterEndScale = endScale;
		masterTime = Time.realtimeSinceStartup;
		masterDefaultScale = defaultScale;
		
		if (isScaling) {
			yield break;
		}
		isScaling = true;
		//Debug.Log("coScale: " + defaultScale + ", " + masterStartScale + " -> " + masterEndScale);
		
		Vector3 scale = defaultScale;
		float s = 0.0f;
		while (s < scaleTime)
		{
			float t = Mathf.Clamp01(s / scaleTime);
			float scl = Mathf.Lerp(masterStartScale, masterEndScale, t);
			scale = defaultScale * scl;
			sprite.scale = scale;
			if (_childSpriteForScale != null) {
				_childSpriteForScale.scale = _childSpriteDefaultScale * scl;
			}
			//transform.localScale = scale;
			
			yield return 0;
			s = (Time.realtimeSinceStartup - masterTime);
		}
		
		sprite.scale = defaultScale * masterEndScale;
		if (_childSpriteForScale != null) {
			_childSpriteForScale.scale = _childSpriteDefaultScale * masterEndScale;
		}
		//Debug.Log("final scale: " + sprite.scale + ", " + masterEndScale);
		isScaling = false;
		//transform.localScale = defaultScale * endScale;
    }
	
	IEnumerator LocalWaitForSeconds(float seconds)
	{
		if (seconds > 0f) {
			yield return new WaitForSeconds(seconds);
		} else {
			yield break;
		}
//		float t0 = Time.realtimeSinceStartup;
//		float s = 0.0f;
//		while (s < seconds)
//		{
//			yield return 0;
//			s = (Time.realtimeSinceStartup - t0);
//		}
	}
	
	IEnumerator coHandleButtonPress(int fingerId)
	{
		buttonDown = true; // inhibit processing in Update()
		//Debug.Log("marking button as down: " + transform.parent.name);
		bool buttonPressed = true; // the button is currently being pressed
		
		if (isSwipeButton) {
			PlaySound();
		}
		
		Vector3 defaultScale = Vector3.zero;
		if (sprite != null) { defaultScale = sprite.scale; }
		if (isScaling) {
			defaultScale = masterDefaultScale;
		}
		//Debug.Log("captured default scale: " + defaultScale);
		
		// Button has been pressed for the first time, cursor/finger is still on it
		if (targetScale != 1.0f)
		{
			// Only do this when the scale is actually enabled, to save one frame of latency when not needed
			//yield return StartCoroutine( coScale(defaultScale, 1.0f, targetScale) );
			StartCoroutine( coScale(defaultScale, 1.0f, targetScale) );
		}
		if (doTargetPos) {
			StartCoroutine(coMove(true));
		}
		//PlaySound();
		if (buttonDownSpriteId != -1)
			sprite.spriteId = buttonDownSpriteId;
		
		if (ButtonDownEvent != null)
			ButtonDownEvent(this);
		
		while (true)
		{
			Vector3 cursorPosition = Vector3.zero;
			bool cursorActive = true;

			// slightly akward arrangement to keep exact backwards compatibility
#if !UNITY_FLASH
			if (Input.multiTouchEnabled)
			{
				bool found = false;
				for (int i = 0; i < Input.touchCount; ++i)
				{
					Touch touch = Input.GetTouch(i);
					if (touch.fingerId == fingerId)
					{
						if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
							break; // treat as not found
						cursorPosition = touch.position;
						_touch.UpdatePosition(touch.position);
						found = true;
					}
				}

				if (!found) cursorActive = false;
			} 			
			else
#endif
			{
				if (!Input.GetMouseButton(0)) {
					cursorActive = false;
				}
				
				cursorPosition = Input.mousePosition;
				_touch.UpdatePosition(new Vector2(cursorPosition.x, cursorPosition.y));
			}

			// user is no longer pressing mouse or no longer touching button
			if (!cursorActive)
				break;

            Ray ray = viewCamera.ScreenPointToRay(cursorPosition);

            RaycastHit hitInfo;
			bool colliderHit = true;
			if (buttonPressed == false || isDraggable == false) {
				colliderHit = collider.Raycast(ray, out hitInfo, 1.0e8f);
			}
            if (buttonPressed && !colliderHit)
			{
				if (targetScale != 1.0f)
				{
					// Finger is still on screen / button is still down, but the cursor has left the bounds of the button
					//yield return StartCoroutine( coScale(defaultScale, targetScale, 1.0f) );
					StartCoroutine( coScale(defaultScale, targetScale, 1.0f) );
				}
				if (doTargetPos) {
					StartCoroutine(coMove(false));
				}
				//PlaySound(buttonUpSound);
				if (buttonUpSpriteId != -1)
					sprite.spriteId = buttonUpSpriteId;
				
				if (ButtonUpEvent != null) {
					_touch.phase = TouchPhase.Ended;
					ButtonUpEvent(this);
					_touch.phase = TouchPhase.Moved;
				}

				buttonPressed = false;
			}
			else if (!buttonPressed & colliderHit)
			{
				if (targetScale != 1.0f)
				{
					// Cursor had left the bounds before, but now has come back in
					//yield return StartCoroutine( coScale(defaultScale, 1.0f, targetScale) );
					StartCoroutine( coScale(defaultScale, 1.0f, targetScale) );
				}
				if (doTargetPos) {
					StartCoroutine(coMove(true));
				}
				if (isSwipeButton) {
					PlaySound();
				}
				if (buttonDownSpriteId != -1)
					sprite.spriteId =  buttonDownSpriteId;
				
				if (ButtonDownEvent != null) {
					_touch.phase = TouchPhase.Began;
					ButtonDownEvent(this);
					_touch.phase = TouchPhase.Ended;
				}

				buttonPressed = true;
			}
			
			if (buttonPressed && ButtonAutoFireEvent != null)
			{
				ButtonAutoFireEvent(this);
			}
			
			yield return 0;
		}
		
		if (buttonPressed)
		{
			_touch.phase = TouchPhase.Ended;

			//Debug.Log("buttonPressed " + Time.frameCount + ", " + Time.realtimeSinceStartup);
			if (targetScale != 1.0f)
			{
				// Handle case when cursor was in bounds when the button was released / finger lifted
				//yield return StartCoroutine( coScale(defaultScale, targetScale, 1.0f) );
				StartCoroutine( coScale(defaultScale, targetScale, 1.0f) );
			}
			if (doTargetPos) {
				StartCoroutine(coMove(false));
			}
			if (isSwipeButton == false) {
				PlaySound();
			}
//			if (buttonPressedSpriteId != -1)
//				sprite.spriteId = buttonPressedSpriteId;
				
			if (targetObject && messageName.Length > 0)
			{
				//Debug.Log("SendMessage " + Time.frameCount + ", " + Time.realtimeSinceStartup);
				targetObject.SendMessage(messageName);
			}

			if (ButtonUpEvent != null)
				ButtonUpEvent(this);
			
			if (ButtonPressedEvent != null)
				ButtonPressedEvent(this);
			
			// Button may have been deactivated in ButtonPressed / Up event
			// Don't wait in that case
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
			if (gameObject.active)
#else
			if (gameObject.activeInHierarchy)
#endif
			{
				//yield return StartCoroutine(LocalWaitForSeconds(pressedWaitTime));
			}
			
			if (buttonUpSpriteId != -1)
				sprite.spriteId = buttonUpSpriteId;
		}
		
		buttonDown = false;
	}

	
	// Update is called once per frame
	void Update ()
	{
		if (buttonDown) // only need to process if button isn't down
		{
			//Debug.Log("buttonDown " + Time.frameCount + ", " + Time.realtimeSinceStartup);
			//Debug.Log("button is down: " + transform.parent.name + ", " + Time.frameCount);
			return;
		}

#if !UNITY_FLASH
		if (Input.multiTouchEnabled)
		{
			for (int i = 0; i < Input.touchCount; ++i)
			{
				Touch touch = Input.GetTouch(i);
				if (touch.phase == TouchPhase.Began || (isSwipeButton && touch.phase == TouchPhase.Moved))
				{
		            Ray ray = viewCamera.ScreenPointToRay(touch.position);
		            RaycastHit hitInfo;
		            if (collider.Raycast(ray, out hitInfo, 1.0e8f))
		            {
						if (!Physics.Raycast(ray, hitInfo.distance - 0.01f))
						{
							//Debug.Log("coHandleButtonPress " + Time.frameCount + ", " + Time.realtimeSinceStartup);
							_touch = new tk2dTouch(touch.position);
							StartCoroutine(coHandleButtonPress(touch.fingerId));
							break; // only one finger on a buton, please.
						}
		            }	            
				}
			}
		}
		else
#endif
		{
			if (Input.GetMouseButtonDown(0) || (isSwipeButton && Input.GetMouseButton(0)))
	        {
				//Debug.Log("checking button press: " + transform.parent.name);
	            Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
	            RaycastHit hitInfo;
	            if (collider.Raycast(ray, out hitInfo, 1.0e8f))
	            {
					//Debug.Log(gameObject.name + ": " + (hitInfo.distance-0.01f));
					if (!Physics.Raycast(ray, hitInfo.distance - 0.01f)) {
						//Debug.Log(gameObject.name + ": coHandleButtonPress " + Time.frameCount + ", " + Time.realtimeSinceStartup);
						_touch = new tk2dTouch(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
						StartCoroutine(coHandleButtonPress(-1));
					}
	            }
	        }
		}
	}
}
