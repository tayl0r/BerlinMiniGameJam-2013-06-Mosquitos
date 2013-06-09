namespace Com.Freakow.BooInspector

macro EasyCol:
	yield [|		
		try:
			EditorGUILayout.BeginVertical()
			$(EasyCol.Body)
		ensure:
			EditorGUILayout.EndVertical()
	|]


macro EasyRow:
	case [| EasyRow $spacer |]:
		yield [|
			try:
				EditorGUILayout.BeginHorizontal()
				EasySpacer $spacer
				$(EasyRow.Body)
			ensure:
				EditorGUILayout.EndHorizontal()
		|]
	otherwise:
		yield [|
			try:
				EditorGUILayout.BeginHorizontal()
				$(EasyRow.Body)
			ensure:
				EditorGUILayout.EndHorizontal()
		|]

macro EasySettings:	
	
	macro EasyRow:
		yield [|
			try:
				EditorGUILayout.BeginHorizontal()
				GUILayout.Label("", EditorStyles.label, MaxWidth(8f), MinWidth(8f));
				$(EasyRow.Body)
			ensure:
				EditorGUILayout.EndHorizontal()
		|]	

	yield [|
		_settings = EditorGUILayout.Foldout(_settings, "Settings", EasyStyles.Settings())		
		if _settings:
			$(EasySettings.Body)
	|]			


macro EasyFoldout:
	case [| EasyFoldout $label, $expression |]:					
		yield [|
			$expression = EditorGUILayout.Foldout($expression, $label, EasyStyles.Foldout())		
			if $expression:
				$(EasyFoldout.Body)
		|]
	case [| EasyFoldout $label, $expression, $pixels |]:					
		yield [|
			GUILayout.Label("", EditorStyles.label, GUILayout.MaxHeight($pixels), GUILayout.MinHeight($pixels))
			$expression = EditorGUILayout.Foldout($expression, $label, EasyStyles.Foldout())		
			if $expression:
				$(EasyFoldout.Body)
		|]

macro EasyLine:
	case [| EasyLine |]:
		yield [|
			splitter as GUIStyle = GUIStyle( GUI.skin.button )
			splitter.stretchWidth = true
			splitter.border = RectOffset(1, 1, 1, 1)
			splitter.margin = RectOffset(0, 0, 7, 7)

			EasyRow:
				GUILayout.Box("", splitter, GUILayout.Height(1F))
		|]
	case [| EasyLine $pixels |]:
		yield [|
			splitter as GUIStyle = GUIStyle( GUI.skin.button )
			splitter.stretchWidth = true
			splitter.border = RectOffset(1, 1, 1, 1)
			splitter.margin = RectOffset(0, 0, 7, 7)

			EasyRow:
				EasySpacer $pixels
				GUILayout.Box("", splitter, GUILayout.Height(1F))
		|]

macro EasySpacer:
	case [| EasySpacer $pixels |]:
		yield [| GUILayout.Label("", EditorStyles.label, MaxWidth($pixels), MinWidth($pixels)) |]

macro EasyToggleButton:
	case [| EasyToggleButton $controlTexture, $textureOn, $textureOff, $toggleVariable |]:
		yield [|
			if $toggleVariable:
				$controlTexture = $textureOn
			else:
				$controlTexture = $textureOff
			if GUILayout.Button($controlTexture, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
				if $controlTexture == $textureOff:
					$controlTexture = $textureOn
				else:
					$controlTexture = $textureOff
				$toggleVariable = not $toggleVariable
		|]
	case [| EasyToggleButton $controlTexture, $textureOn, $textureOff, $toggleVariable, $tooltip |]:
		yield [|
			if $toggleVariable:
				$controlTexture = $textureOn
			else:
				$controlTexture = $textureOff
			guiContent = GUIContent($controlTexture, $tooltip)
			if GUILayout.Button(guiContent, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
				if $controlTexture == $textureOff:
					$controlTexture = $textureOn
				else:
					$controlTexture = $textureOff
				$toggleVariable = not $toggleVariable
		|]

macro EasyList:
	case [| EasyList $title, $list, $objectType, $newVar, $deleteButton |]:
		yield [|
			LookLikeControls(110f, 50f)
			EasyRow 5f:
				GUILayout.Label ($title + " (" + $list.Count + "):")
			EasyRow 10f:
				EasyCol:
					LookLikeControls(110f, 100f)
					for i in range(0, $list.Count):		
						EasyRow:
							EasyObjectField	"", $list[i], typeof($objectType)
							if GUILayout.Button($deleteButton, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
								$list.RemoveAt(i)
								break
					EasyRow:
						LookLikeControls(15f, 10f)
						EasyObjectField	"+", $newVar, typeof($objectType)
				
			if $newVar != null:			
				if $list.Contains($newVar):
					$newVar = null
					Separator()
					return			
				$list.Add($newVar);
				$newVar = null
				Separator()
		|]


macro EasyUndo:
	case [| EasyUndo $label, $current, $lastValue |]:
		yield [| $current = RegisterUndo($lastValue, $current, $label) |]

macro EasyVector3Field:
	case [| EasyVector3Field $label,  $expression |]:
		yield [| $expression = UndoableVector3Field($label, $expression) |]	
	
	case [| EasyVector3Field $label,  $expression, $opt1 |]:
		yield [| $expression = UndoableVector3Field($label, $expression, $opt1) |]	
	
	case [| EasyVector3Field $label,  $expression, $opt1, $opt2 |]:
		yield [| $expression = UndoableVector3Field($label, $expression, $opt1, $opt2) |]	
	
	case [| EasyVector3Field $label,  $expression, $opt1, $opt2, $opt3 |]:
		yield [| $expression = UndoableVector3Field($label, $expression, $opt1, $opt2, $opt3) |]	 
	   
	   

macro EasyGameObjectField:
   case [| EasyGameObjectField $label,  $expression |]:
	   yield [| $expression = UndoableGameObjectField($label, $expression) |]	
	   
   case [| EasyGameObjectField $label,  $expression, $opt1 |]:
	   yield [| $expression = UndoableGameObjectField($label, $expression, $opt1) |]	
	   
   case [| EasyGameObjectField $label,  $expression, $opt1, $opt2 |]:
	   yield [| $expression = UndoableGameObjectField($label, $expression, $opt1, $opt2) |]	
	   
   case [| EasyGameObjectField $label,  $expression, $opt1, $opt2, $opt3 |]:
	   yield [| $expression = UndoableGameObjectField($label, $expression, $opt1, $opt2, $opt3) |]	  
   
		   

macro EasyObjectField:
   case [| EasyObjectField $label,  $expression |]:
	   yield [| $expression = UndoableObjectField($label, $expression) |]	
	   
   case [| EasyObjectField $label,  $expression, $type |]:
	   yield [| $expression = UndoableObjectField($label, $expression, $type) |]
	   
   case [| EasyObjectField $label,  $expression, $type, $opt1 |]:
	   yield [| $expression = UndoableObjectField($label, $expression, $type, $opt1) |]		   	



macro EasyPopup:
   case [| EasyPopup $label,  $expression |]:
	   yield [| $expression = UndoablePopup($label, $expression) |]	
	   
   case [| EasyPopup $label,  $expression, $opt1|]:
	   yield [| $expression = UndoablePopup($label, $expression, $opt1) |]	
	   
   case [| EasyPopup $label,  $expression, $opt1, $opt2 |]:
	   yield [| $expression = UndoablePopup($label, $expression, $opt1, $opt2) |]	
	   
   case [| EasyPopup $label,  $expression, $opt1, $opt2, $opt3 |]:
	   yield [| $expression = UndoablePopup($label, $expression, $opt1, $opt2, $opt3) |]	  



macro EasyIntField:
   case [| EasyIntField $label,  $expression |]:
	   yield [| $expression = UndoableIntField($label, $expression) |]	
	   
   case [| EasyIntField $label,  $expression, $opt1 |]:
	   yield [| $expression = UndoableIntField($label, $expression, $opt1) |]	
	   
   case [| EasyIntField $label,  $expression, $opt1, $opt2 |]:
	   yield [| $expression = UndoableIntField($label, $expression, $opt1, $opt2) |]	
	   
   case [| EasyIntField $label,  $expression, $opt1, $opt2, $opt3 |]:
	   yield [| $expression = UndoableIntField($label, $expression, $opt1, $opt2, $opt3) |]	  



macro EasyTextField:
   case [| EasyTextField $label,  $expression |]:
	   yield [| $expression = UndoableTextField($label, $expression) |]	
	   
   case [| EasyTextField $label,  $expression, $opt1 |]:
	   yield [| $expression = UndoableTextField($label, $expression, $opt1) |]	
	   
   case [| EasyTextField $label,  $expression, $opt1, $opt2 |]:
	   yield [| $expression = UndoableTextField($label, $expression, $opt1, $opt2) |]	
	   
   case [| EasyTextField $label,  $expression, $opt1, $opt2, $opt3 |]:
	   yield [| $expression = UndoableTextField($label, $expression, $opt1, $opt2, $opt3) |]	



macro EasyFloatField:
   case [| EasyFloatField $label,  $expression |]:
	   yield [| $expression = UndoableFloatField($label, $expression) |]	
	   
   case [| EasyFloatField $label,  $expression, $opt1 |]:
	   yield [| $expression = UndoableFloatField($label, $expression, $opt1) |]	
	   
   case [| EasyFloatField $label,  $expression, $opt1, $opt2 |]:
	   yield [| $expression = UndoableFloatField($label, $expression, $opt1, $opt2) |]	
	   
   case [| EasyFloatField $label,  $expression, $opt1, $opt2, $opt3 |]:
	   yield [| $expression = UndoableFloatField($label, $expression, $opt1, $opt2, $opt3) |]	



macro EasyPercent:
   case [| EasyPercent $label,  $expression, $maxValue |]:
	   yield [| $expression = UndoablePercent($label, $expression, $maxValue) |]
   case [| EasyPercent $label,  $expression, $maxValue, $opt1 |]:
	   yield [| $expression = UndoablePercent($label, $expression, $maxValue, $opt1) |]


macro EasyToggle:
   case [| EasyToggle $label,  $expression |]:
	   yield [| $expression = UndoableToggle($label, $expression) |]	
	   
   case [| EasyToggle $label,  $expression, $opt1 |]:
	   yield [| $expression = UndoableToggle($label, $expression, $opt1) |]	       
	   
   case [| EasyToggle $label,  $expression, $opt1, $opt2 |]:
	   yield [| $expression = UndoableToggle($label, $expression, $opt1, $opt2) |]	       
			  
   case [| EasyToggle $label,  $expression, $opt1, $opt2, $opt3 |]:
	   yield [| $expression = UndoableToggle($label, $expression, $opt1, $opt2, $opt3) |]	       
