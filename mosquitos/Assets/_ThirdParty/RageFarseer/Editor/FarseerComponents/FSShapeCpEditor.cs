using UnityEngine;
using UnityEditor;
using Category = FarseerPhysics.Dynamics.Category;

[CustomEditor(typeof(FSShapeComponent))]
public class FSShapeCpEditor : Editor
{
	private FSShapeComponent fsShape;
	protected FSCategorySettings categorySettings;
	
	public virtual void OnEnable()
	{
		fsShape = target as FSShapeComponent;
		FSSettings.Load();
		categorySettings = FSSettings.CategorySettings;
	}
	
	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI();
		
		// draw collision filtering options
		EditorGUILayout.BeginVertical();
		
		fsShape.CollisionFilter = (CollisionGroupDef)EditorGUILayout.EnumPopup("Filter Collision", fsShape.CollisionFilter);
		
		if(fsShape.CollisionFilter == CollisionGroupDef.Manually) {
			bool flag0;
			bool flag1;
		
			fsShape.BelongsToFold = EditorGUILayout.Foldout(fsShape.BelongsToFold, "Belongs To");
			if(fsShape.BelongsToFold)
			{
				flag1 = (fsShape.BelongsTo & Category.All) == Category.All;
				flag0 = EditorGUILayout.Toggle("All", flag1);
				if(flag0 != flag1)
				{
					if(flag0)
						fsShape.BelongsTo = Category.All;
					else
						fsShape.BelongsTo = Category.None;
				}
				//Cat1 to Cat31
				for(int i = 0; i < categorySettings.Cat131.Length; i++)
				{
					flag1 = ((int)fsShape.BelongsTo & (int)Mathf.Pow(2f, (float)i)) != 0;
					flag0 = EditorGUILayout.Toggle(categorySettings.Cat131[i], flag1);
					
					// something changed
					if(flag0 != flag1)
					{
						if(flag0)
							fsShape.BelongsTo |= (Category)((int)Mathf.Pow(2f, (float)i));
						else
							fsShape.BelongsTo ^= (Category)((int)Mathf.Pow(2f, (float)i));
					}
				}
			}
			
			EditorGUILayout.Space();
			
			fsShape.CollidesWithFold = EditorGUILayout.Foldout(fsShape.CollidesWithFold, "Collides With");
			if(fsShape.CollidesWithFold)
			{
				flag1 = (fsShape.CollidesWith & Category.All) == Category.All;
				flag0 = EditorGUILayout.Toggle("All", flag1);
				if(flag0 != flag1)
				{
					if(flag0)
						fsShape.CollidesWith = Category.All;
					else
						fsShape.CollidesWith = Category.None;
				}
				//Cat1 to Cat31
				for(int i = 0; i < categorySettings.Cat131.Length; i++)
				{
					flag1 = ((int)fsShape.CollidesWith & (int)Mathf.Pow(2f, (float)i)) != 0;
					flag0 = EditorGUILayout.Toggle(categorySettings.Cat131[i], flag1);
					
					// something changed
					if(flag0 != flag1)
					{
						if(flag0)
							fsShape.CollidesWith |= (Category)((int)Mathf.Pow(2f, (float)i));
						else
							fsShape.CollidesWith ^= (Category)((int)Mathf.Pow(2f, (float)i));
					}
				}
			}
		}
		else if(fsShape.CollisionFilter == CollisionGroupDef.PresetFile)
		{
			fsShape.CollisionGroup = (FSCollisionGroup)EditorGUILayout.ObjectField("Group Preset File", fsShape.CollisionGroup, typeof(FSCollisionGroup), true);
		}
		
		EditorGUILayout.EndVertical();
	}
}
