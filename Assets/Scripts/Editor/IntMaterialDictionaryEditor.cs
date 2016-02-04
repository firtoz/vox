using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof (IntMaterial))]
public class IntMaterialDictionaryEditor : SerializeDictionaryEditor<int, Material> {
	protected override int DoNewKeyField(Rect rect, int currentValue) {
		return EditorGUI.IntField(rect, currentValue);
	}

	protected override Material DoNewValueField(Rect rect, Material currentValue) {
		return (Material) EditorGUI.ObjectField(rect, currentValue, typeof(Material), true);
	}

	protected override int CreateNewKey() {
		return 0;
	}

	protected override Material CreateNewValue() {
		return null;
	}

	protected override void SetKey(SerializedProperty serializedProperty, int newKey) {
		serializedProperty.intValue = newKey;
	}

	protected override void SetValue(SerializedProperty serializedProperty, Material newValue) {
		serializedProperty.objectReferenceValue = newValue;
	}
}