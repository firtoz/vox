using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
	public readonly Dictionary<TKey, TValue> value;

	[SerializeField]
	private List<TKey> _keys;
	[SerializeField]
	private List<TValue> _values;  

	protected SerializableDictionary() {
		value = new Dictionary<TKey, TValue>();
	}

	public void OnBeforeSerialize() {
		_keys = value.Keys.ToList();
		_values = value.Values.ToList();
	}

	public void OnAfterDeserialize() {
		value.Clear();
		for (var i = 0; i < _keys.Count; i++) {
			value.Add(_keys[i], _values[i]);
		}
	}
}