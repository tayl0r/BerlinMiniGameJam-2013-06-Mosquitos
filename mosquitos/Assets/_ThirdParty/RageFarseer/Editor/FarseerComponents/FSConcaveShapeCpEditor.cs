using Microsoft.Xna.Framework;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Category = FarseerPhysics.Dynamics.Category;

[CustomEditor(typeof (FSConcaveShapeComponent))]
public class FSConcaveShapeCpEditor : Editor
{
	public SerializedObject EditTarget;
	protected FSConcaveShapeComponent ConcaveShape;
	protected FSCategorySettings categorySettings;
	public static float GizmoScale;

	public virtual void OnEnable() {
		ConcaveShape = target as FSConcaveShapeComponent;
		EditTarget = new SerializedObject(ConcaveShape);
		FSSettings.Load();
		categorySettings = FSSettings.CategorySettings;
	}

	public override void OnInspectorGUI() {
		EditorGUIUtility.LookLikeControls();
		EditTarget.Update();
		EditorGUILayout.BeginVertical();
		EditorGUILayout.Separator();
		ConcaveShape.PointInput = (FSShapePointInput) EditorGUILayout.EnumPopup("Define points by", ConcaveShape.PointInput);
		EditorGUIUtility.LookLikeInspector();
		EditorGUILayout.Separator();
		if (ConcaveShape.PointInput == FSShapePointInput.Transform) {
			if (ConcaveShape.PointsTransforms == null)
				ConcaveShape.PointsTransforms = new Transform[0];

			EditorGUILayout.BeginVertical(GUI.skin.box);
			SerializedProperty tp0 = EditTarget.FindProperty("PointsTransforms"); // name
			EditorGUILayout.PropertyField(tp0);
			EditorGUILayout.BeginVertical(GUI.skin.box);
				tp0.isExpanded = EditorGUILayout.Toggle("show", tp0.isExpanded);
				if (tp0.isExpanded) {
					tp0.Next(true);
					//EditorGUILayout.PropertyField(tp0);
					tp0.Next(true);
					EditorGUILayout.PropertyField(tp0);
					EditorGUILayout.Separator();
					while (tp0.Next(true)) {
						//EditorGUILayout.LabelField(tp0.name);// m_FileID m_PathID data
						if (!tp0.propertyPath.StartsWith("PointsTransforms"))
							break;
						if (tp0.name == "data")
							EditorGUILayout.PropertyField(tp0);
					}
				}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndVertical();
			//
			EditorGUILayout.Separator();
		}
		else
		{
			if (ConcaveShape.PointsCoordinates == null)
				ConcaveShape.PointsCoordinates = new Vector2[0];

			EditorGUILayout.BeginVertical(GUI.skin.box);
			SerializedProperty cp0 = EditTarget.FindProperty("PointsCoordinates");
			EditorGUILayout.PropertyField(cp0);
			EditorGUILayout.BeginVertical(GUI.skin.box);
			cp0.isExpanded = EditorGUILayout.Toggle("show", cp0.isExpanded);
			if (cp0.isExpanded)
			{
				if (cp0.Next(true))
				{
					//EditorGUILayout.PropertyField(cp0);
					if (cp0.Next(true))
						EditorGUILayout.PropertyField(cp0);
					EditorGUILayout.Separator();
				}
				while (cp0.Next(true))
				{
					//EditorGUILayout.LabelField(cp0.name); m_FileID m_PathID data
					if (!cp0.propertyPath.StartsWith("PointsCoordinates"))
						break;
					//if(cp0.name == "data")
					EditorGUILayout.PropertyField(cp0);
				}
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();
		}
		ConcaveShape.Density = EditorGUILayout.FloatField("Density", ConcaveShape.Density);
		ConcaveShape.Friction = EditorGUILayout.FloatField("Friction", ConcaveShape.Friction);
		ConcaveShape.Restitution = EditorGUILayout.FloatField("Restitution", ConcaveShape.Restitution);

		EditorGUILayout.Separator();
		ConcaveShape.LocalGizmoScale = EditorGUILayout.FloatField("Gizmo Scale", ConcaveShape.LocalGizmoScale);
		//ConcaveShape.UseCollisionGroups = EditorGUILayout.Toggle("Use Collision Groups", ConcaveShape.UseCollisionGroups);
		ConcaveShape.CollisionFilter =
			(CollisionGroupDef) EditorGUILayout.EnumPopup("Filter Collision", ConcaveShape.CollisionFilter);
		if (ConcaveShape.CollisionFilter == CollisionGroupDef.Manually)
		{
			bool flag0;
			bool flag1;

			ConcaveShape.BelongsToFold = EditorGUILayout.Foldout(ConcaveShape.BelongsToFold, "Belongs To");
			if (ConcaveShape.BelongsToFold)
			{
				flag1 = (ConcaveShape.BelongsTo & Category.All) == Category.All;
				flag0 = EditorGUILayout.Toggle("All", flag1);
				if (flag0 != flag1)
				{
					if (flag0)
						ConcaveShape.BelongsTo = Category.All;
					else
						ConcaveShape.BelongsTo = Category.None;
				}
				//Cat1 to Cat31
				for (int i = 0; i < categorySettings.Cat131.Length; i++)
				{
					flag1 = ((int) ConcaveShape.BelongsTo & (int) Mathf.Pow(2f, (float) i)) != 0;
					flag0 = EditorGUILayout.Toggle(categorySettings.Cat131[i], flag1);

					// something changed
					if (flag0 != flag1)
					{
						if (flag0)
							ConcaveShape.BelongsTo |= (Category) ((int) Mathf.Pow(2f, (float) i));
						else
							ConcaveShape.BelongsTo ^= (Category) ((int) Mathf.Pow(2f, (float) i));
					}
				}
			}

			EditorGUILayout.Space();

			ConcaveShape.CollidesWithFold = EditorGUILayout.Foldout(ConcaveShape.CollidesWithFold, "Collides With");
			if (ConcaveShape.CollidesWithFold)
			{
				flag1 = (ConcaveShape.CollidesWith & Category.All) == Category.All;
				flag0 = EditorGUILayout.Toggle("All", flag1);
				if (flag0 != flag1)
				{
					if (flag0)
						ConcaveShape.CollidesWith = Category.All;
					else
						ConcaveShape.CollidesWith = Category.None;
				}
				//Cat1 to Cat31
				for (int i = 0; i < categorySettings.Cat131.Length; i++)
				{
					flag1 = ((int) ConcaveShape.CollidesWith & (int) Mathf.Pow(2f, (float) i)) != 0;
					flag0 = EditorGUILayout.Toggle(categorySettings.Cat131[i], flag1);

					// something changed
					if (flag0 != flag1)
					{
						if (flag0)
							ConcaveShape.CollidesWith |= (Category) ((int) Mathf.Pow(2f, (float) i));
						else
							ConcaveShape.CollidesWith ^= (Category) ((int) Mathf.Pow(2f, (float) i));
					}
				}
			}
			/*SerializedProperty colcats = EditTarget.FindProperty("CollisionCategories");
			EditorGUILayout.PropertyField(colcats);
			if(colcats.isExpanded)
			{
				if(colcats.Next(true))
				{
					if(colcats.Next(true))
						EditorGUILayout.PropertyField(colcats);
				}
				while(colcats.Next(true))
				{
					if(!colcats.propertyPath.StartsWith("CollisionCategories"))
						break;
					EditorGUILayout.PropertyField(colcats);
				}
			}
			//
			SerializedProperty colwith = EditTarget.FindProperty("CollidesWith");
			EditorGUILayout.PropertyField(colwith);
			if(colwith.isExpanded)
			{
				if(colwith.Next(true))
				{
					if(colwith.Next(true))
						EditorGUILayout.PropertyField(colwith);
				}
				while(colwith.Next(true))
				{
					if(!colwith.propertyPath.StartsWith("CollidesWith"))
						break;
					EditorGUILayout.PropertyField(colwith);
				}
			}*/
		}
		else if (ConcaveShape.CollisionFilter == CollisionGroupDef.PresetFile)
		{
			ConcaveShape.CollisionGroup =
				(FSCollisionGroup)
				EditorGUILayout.ObjectField("Group Preset File", ConcaveShape.CollisionGroup, typeof (FSCollisionGroup), true);
		}
		EditorGUILayout.Separator();
		bool convert = GUILayout.Button("Generate convex shapes");

		EditorGUILayout.EndVertical();
		//base.OnInspectorGUI ();
		EditTarget.ApplyModifiedProperties();

		if (convert) ConcaveShape.ConvertToConvex();
		Rect r = EditorGUILayout.BeginVertical();
		var offs = new Vector3(r.x + 50f, r.y + 50f, 0f);
		//Handles.BeginGUI(r);
		Transform prev;
		if (ConcaveShape.PointsTransforms != null)
		{
			if (ConcaveShape.PointsTransforms.Length > 2)
			{
				prev = ConcaveShape.PointsTransforms[ConcaveShape.PointsTransforms.Length - 1];
				for (int i = 0; i < ConcaveShape.PointsTransforms.Length; i++)
				{
					if (ConcaveShape.PointsTransforms[i] != null && prev != null)
						Handles.DrawLine(offs + prev.localPosition*10f, offs + ConcaveShape.PointsTransforms[i].localPosition*10f);
					prev = ConcaveShape.PointsTransforms[i];
				}
			}
		}
		//Handles.EndGUI();
		EditorGUILayout.EndVertical();
	}

}
