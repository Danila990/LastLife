using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utils
{
    internal sealed class AsyncCustomMessageHandler<T> : IUniTaskSource<T>, IObserver<T>
    {
        int handleCalled = 0;
        IDisposable subscription;
        CancellationToken cancellationToken;
        CancellationTokenRegistration cancellationTokenRegistration;
        UniTaskCompletionSourceCore<T> core;

        static readonly Action<object> cancelCallback = Cancel;

        public AsyncCustomMessageHandler(IReactiveCommand<T> subscriber, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                core.TrySetException(new OperationCanceledException(cancellationToken));
                return;
            }

            try
            {
                subscription = subscriber.Subscribe(this);
            }
            catch (Exception ex)
            {
                core.TrySetException(ex);
                return;
            }

            if (handleCalled != 0)
            {
                subscription?.Dispose();
                return;
            }

            if (cancellationToken.CanBeCanceled)
            {
                this.cancellationToken = cancellationToken;
                cancellationTokenRegistration = cancellationToken.Register(cancelCallback, this, false);
            }
        }

        private static void Cancel(object state)
        {
            var self = (AsyncCustomMessageHandler<T>)state;
            self.subscription?.Dispose();
            self.core.TrySetException(new OperationCanceledException(self.cancellationToken));
        }

        void IUniTaskSource.GetResult(short token) => GetResult(token);
        public UniTaskStatus UnsafeGetStatus() => core.UnsafeGetStatus();
        public UniTaskStatus GetStatus(short token)
        {
            return core.GetStatus(token);
        }

        public void OnCompleted(Action<object> continuation, object state, short token)
        {
            core.OnCompleted(continuation, state, token);
        }

        public T GetResult(short token)
        {
            return core.GetResult(token);
        }
        
        public void OnCompleted()
        {
        }
        
        public void OnError(Exception error)
        {
            core.TrySetException(error);
        }
        
        public void OnNext(T value)
        {
            if (Interlocked.Increment(ref handleCalled) == 1)
            {
                try
                {
                    core.TrySetResult(value);
                }
                finally
                {
                    subscription?.Dispose();
                    cancellationTokenRegistration.Dispose();
                }
            }
        }
    }
    
    
	[Serializable]
    public class ReactiveSortedDictionary<TKey, TValue> : IReactiveDictionary<TKey, TValue>, IDictionary, IDisposable
#if !UNITY_METRO
        , ISerializable, IDeserializationCallback
#endif
    {
        [NonSerialized]
        bool isDisposed = false;

#if !UniRxLibrary
        [SerializeField]
#endif
        private readonly SortedDictionary<TKey, TValue> _inner;

        public ReactiveSortedDictionary()
        {
            _inner = new SortedDictionary<TKey, TValue>();
        }

        public ReactiveSortedDictionary(IComparer<TKey> comparer)
        {
            _inner = new SortedDictionary<TKey, TValue>(comparer);
        }

        public ReactiveSortedDictionary(SortedDictionary<TKey, TValue> innerDictionary)
        {
            _inner = innerDictionary;
        }

        public TValue this[TKey key]
        {
            get
            {
                return _inner[key];
            }

            set
            {
                if (TryGetValue(key, out var oldValue))
                {
                    _inner[key] = value;
                    _dictionaryReplace?.OnNext(new DictionaryReplaceEvent<TKey, TValue>(key, oldValue, value));
                }
                else
                {
                    _inner[key] = value;
                    dictionaryAdd?.OnNext(new DictionaryAddEvent<TKey, TValue>(key, value));
                    countChanged?.OnNext(Count);
                }
            }
        }

        public int Count
        {
            get
            {
                return _inner.Count;
            }
        }

        public SortedDictionary<TKey, TValue>.KeyCollection Keys
        {
            get
            {
                return _inner.Keys;
            }
        }

        public SortedDictionary<TKey, TValue>.ValueCollection Values
        {
            get
            {
                return _inner.Values;
            }
        }

        public void Add(TKey key, TValue value)
        {
            _inner.Add(key, value);

            dictionaryAdd?.OnNext(new DictionaryAddEvent<TKey, TValue>(key, value));
            countChanged?.OnNext(Count);
        }

        public void Clear()
        {
            var beforeCount = Count;
            _inner.Clear();

            collectionReset?.OnNext(Unit.Default);
            if (beforeCount > 0)
            {
                countChanged?.OnNext(Count);
            }
        }

        public bool Remove(TKey key)
        {
            if (_inner.TryGetValue(key, out var oldValue))
            {
                var isSuccessRemove = _inner.Remove(key);
                if (isSuccessRemove)
                {
                    if (_dictionaryRemove != null) _dictionaryRemove.OnNext(new DictionaryRemoveEvent<TKey, TValue>(key, oldValue));
                    if (countChanged != null) countChanged.OnNext(Count);
                }
                return isSuccessRemove;
            }
            else
            {
                return false;
            }
        }

        public bool ContainsKey(TKey key)
        {
            return _inner.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _inner.TryGetValue(key, out value);
        }

        public SortedDictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        void DisposeSubject<TSubject>(ref Subject<TSubject> subject)
        {
            if (subject != null)
            {
                try
                {
                    subject.OnCompleted();
                }
                finally
                {
                    subject.Dispose();
                    subject = null;
                }
            }
        }

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisposeSubject(ref countChanged);
                    DisposeSubject(ref collectionReset);
                    DisposeSubject(ref dictionaryAdd);
                    DisposeSubject(ref _dictionaryRemove);
                    DisposeSubject(ref _dictionaryReplace);
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion


        #region Observe

        [NonSerialized]
        Subject<int> countChanged = null;
        public IObservable<int> ObserveCountChanged(bool notifyCurrentCount = false)
        {
            if (isDisposed) return Observable.Empty<int>();

            var subject = countChanged ??= new Subject<int>();
            if (notifyCurrentCount)
            {
                return subject.StartWith(() => Count);
            }
            else
            {
                return subject;
            }
        }

        [NonSerialized]
        Subject<Unit> collectionReset = null;
        public IObservable<Unit> ObserveReset()
        {
            if (isDisposed) return Observable.Empty<Unit>();
            return collectionReset ??= new Subject<Unit>();
        }

        [NonSerialized]
        Subject<DictionaryAddEvent<TKey, TValue>> dictionaryAdd = null;
        public IObservable<DictionaryAddEvent<TKey, TValue>> ObserveAdd()
        {
            if (isDisposed) return Observable.Empty<DictionaryAddEvent<TKey, TValue>>();
            return dictionaryAdd ??= new Subject<DictionaryAddEvent<TKey, TValue>>();
        }

        [NonSerialized]
        Subject<DictionaryRemoveEvent<TKey, TValue>> _dictionaryRemove = null;
        public IObservable<DictionaryRemoveEvent<TKey, TValue>> ObserveRemove()
        {
            if (isDisposed) return Observable.Empty<DictionaryRemoveEvent<TKey, TValue>>();
            return _dictionaryRemove ??= new Subject<DictionaryRemoveEvent<TKey, TValue>>();
        }

        [NonSerialized]
        Subject<DictionaryReplaceEvent<TKey, TValue>> _dictionaryReplace = null;
        public IObservable<DictionaryReplaceEvent<TKey, TValue>> ObserveReplace()
        {
            if (isDisposed) return Observable.Empty<DictionaryReplaceEvent<TKey, TValue>>();
            return _dictionaryReplace ??= new Subject<DictionaryReplaceEvent<TKey, TValue>>();
        }

        #endregion

        #region implement explicit

        object IDictionary.this[object key]
        {
            get
            {
                return this[(TKey)key];
            }

            set
            {
                this[(TKey)key] = (TValue)value;
            }
        }


        bool IDictionary.IsFixedSize
        {
            get
            {
                return ((IDictionary)_inner).IsFixedSize;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return ((IDictionary)_inner).IsReadOnly;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return ((IDictionary)_inner).IsSynchronized;
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return ((IDictionary)_inner).Keys;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return ((IDictionary)_inner).SyncRoot;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return ((IDictionary)_inner).Values;
            }
        }


        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return ((ICollection<KeyValuePair<TKey, TValue>>)_inner).IsReadOnly;
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                return _inner.Keys;
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                return _inner.Values;
            }
        }

        void IDictionary.Add(object key, object value)
        {
            Add((TKey)key, (TValue)value);
        }

        bool IDictionary.Contains(object key)
        {
            return ((IDictionary)_inner).Contains(key);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((IDictionary)_inner).CopyTo(array, index);
        }

#if !UNITY_METRO

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ((ISerializable)_inner).GetObjectData(info, context);
        }

        public void OnDeserialization(object sender)
        {
            ((IDeserializationCallback)_inner).OnDeserialization(sender);
        }

#endif

        void IDictionary.Remove(object key)
        {
            Remove((TKey)key);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add((TKey)item.Key, (TValue)item.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_inner).Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_inner).CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_inner).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            if (TryGetValue(item.Key, out var v))
            {
                if (EqualityComparer<TValue>.Default.Equals(v, item.Value))
                {
                    Remove(item.Key);
                    return true;
                }
            }

            return false;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)_inner).GetEnumerator();
        }

        #endregion
    }
	
	public static class UniRxUtils
	{
		public static IDisposable SubscribeToText(this IObservable<string> source, TMP_Text text)
		{
			return source.SubscribeWithState(text, (x, t) => t.text = x);
		}

		public static IDisposable SubscribeToText(this IObservable<int> source, TMP_Text text)
		{
			return source.SubscribeWithState(text, (x, t) => t.text = x.ToString());
		}
		
		public static IDisposable SubscribeToSlider(this IObservable<int> source, Slider slider)
		{
			return source.SubscribeWithState(slider, (x, t) => t.value = x);
		}
		
		public static IDisposable SubscribeToSlider(this IObservable<float> source, Slider slider)
		{
			return source.SubscribeWithState(slider, (x, t) => t.value = x);
		}
		
		public static IDisposable SubscribeToText<T>(this IObservable<T> source, TMP_Text text)
		{
			return source.SubscribeWithState(text, (x, t) => t.text = x.ToString());
		}

		public static IDisposable SubscribeToText<T>(this IObservable<T> source, TMP_Text text, Func<T, string> selector)
		{
			return source.SubscribeWithState2(text, selector, (x, t, s) => t.text = s(x));
		}
		
		public static ObservableDragTriggers OnDragsEvents(this UIBehaviour component)
		{
			if (component == null || component.gameObject == null) 
				throw new NullReferenceException(component.ToString());
			return GetOrAddComponent<ObservableDragTriggers>(component.gameObject);
		}
		
		private static T GetOrAddComponent<T>(GameObject gameObject)
			where T : Component
		{
			var component = gameObject.GetComponent<T>();
			if (component == null)
			{
				component = gameObject.AddComponent<T>();
			}

			return component;
		}
		
        static readonly Action<object> Callback = CancelCallback;

        static void CancelCallback(object state)
        {
            var tuple = (Tuple<ICancelPromise, IDisposable>)state;
            tuple.Item2.Dispose();
            tuple.Item1.TrySetCanceled();
        }

        public static UniTask<T> WaitUntilValueChangedAsyncUniTask<T>(this IReadOnlyReactiveProperty<T> source, CancellationToken cancellationToken = default)
        {
	        var tcs = new UniTaskCompletionSource<T>();
            var disposable = new SingleAssignmentDisposable();
            if (source.HasValue)
            {
                // Skip first value
                var isFirstValue = true;
                disposable.Disposable = source.Subscribe(x =>
                {
                    if (isFirstValue)
                    {
                        isFirstValue = false;
                    }
                    else
                    {
                        disposable.Dispose(); // finish subscription.
                        tcs.TrySetResult(x);
                    }
                }, ex => tcs.TrySetException(ex), () => tcs.TrySetCanceled());
            }
            else
            {
                disposable.Disposable = source.Subscribe(x =>
                {
                    disposable.Dispose(); // finish subscription.
                    tcs.TrySetResult(x);
                }, ex => tcs.TrySetException(ex), () => tcs.TrySetCanceled());
            }

            cancellationToken.Register(Callback, Tuple.Create((ICancelPromise)tcs, disposable.Disposable));

            return tcs.Task;
        }
	}
}