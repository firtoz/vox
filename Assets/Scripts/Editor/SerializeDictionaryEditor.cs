using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public abstract class SerializeDictionaryEditor<TKey, TValue> : PropertyDrawer {
	private bool _wantsAdd;

	protected abstract TKey DoNewKeyField(Rect rect, TKey currentKey);
	protected abstract TValue DoNewValueField(Rect rect, TValue currentValue);

	protected virtual TKey CreateNewKey() {
		return default(TKey);
	}

	protected virtual TValue CreateNewValue() {
		return default(TValue);
	}

	private TKey _newKey;
	private TValue _newValue;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
//		base.OnGUI(position, property, label);

		// Using BeginProperty / EndProperty on the parent property means that
		// prefab override logic works on the entire property.
		EditorGUI.BeginProperty(position, label, property);
//
//		// Draw label

		var keys = property.FindPropertyRelative("_keys");
		var values = property.FindPropertyRelative("_values");

		EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, 16f), keys, label);
		if (keys.isExpanded) {
			for (var i = 0; i < keys.arraySize; ++i)
			{
				var boxWidth = 20;
				var width = (position.width - boxWidth);
				var x = position.x + boxWidth;

				var leftRect = new Rect(x, position.y + (i + 1) * 16f, width * 0.5f, 16f);
				var rightRect = new Rect(x + width * 0.5f, position.y + (i + 1) * 16f, width* 0.5f, 16f);
				
				EditorGUI.PropertyField(leftRect, keys.GetArrayElementAtIndex(i), GUIContent.none);
				EditorGUI.PropertyField(rightRect, values.GetArrayElementAtIndex(i), GUIContent.none);

				var buttonRect = new Rect(leftRect.x - boxWidth, leftRect.y, boxWidth, 16f);

				if (GUI.Button(buttonRect, "x")) {
					keys.DeleteArrayElementAtIndex(i);
					values.DeleteArrayElementAtIndex(i);
					break;
				}
			}

			if (_wantsAdd)
			{
				var leftRect = new Rect(position.x, position.y + (keys.arraySize + 1) * 16f, position.width * 0.5f, 16f);
				var rightRect = new Rect(position.x + position.width * 0.5f, position.y + (keys.arraySize + 1) * 16f, position.width * 0.5f, 16f);

				_newKey = DoNewKeyField(leftRect, _newKey);
				_newValue = DoNewValueField(rightRect, _newValue);

				position.y += 16f;
			}


			if (!_wantsAdd) {
				var newRect = new Rect(position.x, position.y + (keys.arraySize + 1) * 16f, position.width, 16f);

				if (GUI.Button(newRect, "Add new item")) {
					_newKey = CreateNewKey();
					_newValue = CreateNewValue();

					_wantsAdd = true;
				}
			} else
			{
				var leftRect = new Rect(position.x, position.y + (keys.arraySize + 1) * 16f, position.width * 0.5f, 16f);
				var rightRect = new Rect(position.x + position.width * 0.5f, position.y + (keys.arraySize + 1) * 16f, position.width * 0.5f, 16f);

				if (GUI.Button(leftRect, "Cancel"))
				{
					_wantsAdd = false;
				}

				if (GUI.Button(rightRect, "Save"))
				{
					_wantsAdd = false;

					keys.InsertArrayElementAtIndex(keys.arraySize);
					SetKey(keys.GetArrayElementAtIndex(keys.arraySize - 1), _newKey);

					values.InsertArrayElementAtIndex(values.arraySize);
					SetValue(values.GetArrayElementAtIndex(values.arraySize - 1), _newValue);
				}
			}
		}
//
//		// Calculate rects
//		var amountRect = new Rect(position.x, position.y, position.width, position.height);
		//		var unitRect = new Rect(position.x + 35, position.y, 50, position.height);
		//		var nameRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);
		//
		//		// Draw fields - passs GUIContent.none to each so they are drawn without labels
		var firstProperty = keys.GetArrayElementAtIndex(0);

//		EditorGUI.PropertyField(amountRect, firstProperty, GUIContent.none, true);

		EditorGUI.EndProperty();

		property.serializedObject.ApplyModifiedProperties();
	}

	protected abstract void SetKey(SerializedProperty serializedProperty, TKey newKey);
	protected abstract void SetValue(SerializedProperty serializedProperty, TValue newValue);

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		var keys = property.FindPropertyRelative("_keys");

		var size = 0f;
		if (keys.isExpanded)
		{
			size += 16f * (keys.arraySize + 2);

			if (_wantsAdd)
			{
				size += 16f;
			}
		}
		else {
			size += 16f;
		}

		return size;
	}
}
