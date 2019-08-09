/*
 * Copyright 2019, Digi International Inc.
 * Copyright 2014, 2015, Sébastien Rault.
 * 
 * Permission to use, copy, modify, and/or distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 * WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 * ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 * WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 * ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 * OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

namespace System.Collections.Generic
{
	public class LinkedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		IList<TKey> _keys = new List<TKey>();
		IDictionary<TKey, TValue> _datas = new Dictionary<TKey, TValue>();

		struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
		{
			LinkedDictionary<TKey, TValue> _dictionary;
			IEnumerator<TKey> _enumerator;

			internal Enumerator(LinkedDictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
			{
				_dictionary = dictionary;
				_enumerator = dictionary._keys.GetEnumerator();
			}

			public KeyValuePair<TKey, TValue> Current
			{
				get { return new KeyValuePair<TKey, TValue>(_enumerator.Current, _dictionary[_enumerator.Current]); }
			}

			public void Dispose()
			{
				_enumerator.Dispose();
			}

			object System.Collections.IEnumerator.Current
			{
				get { return Current; }
			}

			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}

			public void Reset()
			{
				_enumerator.Reset();
			}
		}

		#region IDictionary<TKey, TValue> implementation
		public void Add(TKey key, TValue value)
		{
			if (_keys.Contains(key))
				_datas[key] = value;
			else
			{
				_keys.Add(key);
				_datas.Add(key, value);
			}
		}

		public bool ContainsKey(TKey key)
		{
			return _keys.Contains(key);
		}

		public ICollection<TKey> Keys
		{
			get { return _datas.Keys; }
		}

		public bool Remove(TKey key)
		{
			_keys.Remove(key);
			return _datas.Remove(key);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return _datas.TryGetValue(key, out value);
		}

		public ICollection<TValue> Values
		{
			get { return _datas.Values; }
		}

		public TValue this[TKey key]
		{
			get
			{
				return _datas[key];
			}
			set
			{
				_datas[key] = value;
			}
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			if (_keys.Contains(item.Key))
				_datas[item.Key] = item.Value;
			else
			{
				_keys.Add(item.Key);
				_datas.Add(item);
			}
		}

		public void Clear()
		{
			_keys.Clear();
			_datas.Clear();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return _datas.Contains(item);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			foreach (var key in _keys)
			{
				array[arrayIndex++] = new KeyValuePair<TKey, TValue>(key, _datas[key]);
			}
		}

		public int Count
		{
			get { return _keys.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			_keys.Remove(item.Key);
			return _datas.Remove(item);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return new Enumerator(this, 0);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion
	}
}
