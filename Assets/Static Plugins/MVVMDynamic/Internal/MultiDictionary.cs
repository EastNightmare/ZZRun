using System;
using System.Collections;
using System.Collections.Generic;

namespace MVVMDynamic.Internal
{
	public class MultiDictionary<TKey,TValue>:IEnumerable<KeyValuePair<TKey, List<TValue>>>
	{
		Dictionary<TKey,List<TValue>> _dictionary = new Dictionary<TKey, List<TValue>>();

	    public void Add (TKey key, TValue value)
		{
			List<TValue> items;
			if (!_dictionary.TryGetValue (key, out items)) 
			{
				items = new List<TValue>();
				_dictionary.Add(key, items);
			}

			items.Add (value);
		}

		public bool ContainsKey (TKey key)
		{
			return _dictionary.ContainsKey (key);
		}

		public bool Remove (TKey key, TValue value)
		{
			return _dictionary [key].Remove (value);
		}

		public bool TryGetValue (TKey key, out List<TValue> value)
		{
			return _dictionary.TryGetValue (key, out value);
		}

		public List<TValue> this [TKey key] {
			get
			{
			    if(_dictionary.ContainsKey(key))
				{
					return _dictionary[key];
				}
			    return new List<TValue>();
			}
		    set {
				_dictionary[key] = value;
			}
		}

		public ICollection<TKey> Keys {
			get {
				throw new NotImplementedException ();
			}
		}

		public void Clear ()
		{
			_dictionary.Clear ();
		}


		#region IEnumerable implementation

		public IEnumerator<KeyValuePair<TKey, List<TValue>>> GetEnumerator ()
		{
			return _dictionary.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return _dictionary.GetEnumerator ();
		}

		#endregion

	}
}

