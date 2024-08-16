using System;
using System.Collections.Generic;
using Core.AnimationRigging;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils.Constants;

namespace Core.Inventory
{
    public class ItemAnimationBehaviour : IDisposable
    {
        public string OverrideIdle;
        public AnimationBehaviour[] Behaviours;
        private IReadOnlyDictionary<int, AnimationBehaviour> _handsBehaviour;
        private IReadOnlyDictionary<int, AnimationBehaviour> _itemBehaviour;
        private AnimationBehaviour _nextHandAnimationBehaviour;
        private AnimationBehaviour _nextItemAnimationBehaviour;
        private AnimationBehaviour _currentHandAnimationBehaviour;
        private AnimationBehaviour _currentItemAnimationBehaviour;
        private readonly Animation _itemAnimator;
        private readonly Animator _handAnimator;
        private readonly Animator _legAnimator;
        private bool _isInited;
        private bool _isPlaying;
        private readonly MonoRigProvider _rigProvider;
        private readonly AnimatorObserver _handAnimatorObserver;
        private readonly AnimatorObserver _legAnimatorObserver;
        private int _currentHash;

        public readonly struct ItemAnimData
        {
            public readonly Animation ItemAnimator;
            public readonly Animator HandAnimator;
            public readonly Animator LegAnimator;
            public readonly MonoRigProvider RigProvider;
            
            public ItemAnimData(Animation itemAnimator, Animator handAnimator, Animator legAnimator, MonoRigProvider rigProvider)
            {
                ItemAnimator = itemAnimator;
                HandAnimator = handAnimator;
                LegAnimator = legAnimator;
                RigProvider = rigProvider;
            }
        }
        
        public readonly struct RuntimeItemData
        {
            public readonly string OverrideIdle;
            public readonly bool NeedShowHands;
            public readonly AnimationBehaviour[] Behaviours;
            public readonly IReadOnlyDictionary<int, AnimationBehaviour> HandsBehaviour;
            public readonly IReadOnlyDictionary<int, AnimationBehaviour> ItemBehaviour;
            
            public RuntimeItemData(
                string overrideIdle, 
                bool needShowHands, 
                AnimationBehaviour[] behaviours, 
                IReadOnlyDictionary<int, AnimationBehaviour> handsBehaviour, 
                IReadOnlyDictionary<int, AnimationBehaviour> itemBehaviour)
            {
                OverrideIdle = overrideIdle;
                NeedShowHands = needShowHands;
                Behaviours = behaviours;
                HandsBehaviour = handsBehaviour;
                ItemBehaviour = itemBehaviour;
            }
        }

        public ItemAnimationBehaviour(ItemAnimData itemAnimData)
        {
            _itemAnimator = itemAnimData.ItemAnimator;
            _handAnimator = itemAnimData.HandAnimator;
            _legAnimator = itemAnimData.LegAnimator;
            _rigProvider = itemAnimData.RigProvider;
            
            _handAnimatorObserver = new AnimatorObserver(_handAnimator);
            _legAnimatorObserver = new AnimatorObserver(_legAnimator);
            _isInited = true;
        }

        public void SetupFromData(ref RuntimeItemData runtimeItemData)
        {
            OverrideIdle = runtimeItemData.OverrideIdle;
            Behaviours = runtimeItemData.Behaviours;
            _handsBehaviour = runtimeItemData.HandsBehaviour;
            _itemBehaviour = runtimeItemData.ItemBehaviour;

            foreach (var behaviour in Behaviours)
            {
                //behaviour.CurrentExitTime = 0;
                if (behaviour.AnimatorType == AnimatorSelect.ItemAnimator)
                {
                    _itemAnimator.AddClip(behaviour.Clip, behaviour.Key);
                }
            }
            
            _isPlaying = true;
        }

        public void SetFloat(int hash, float value)
        {
            _handAnimator.SetFloat(hash, value);
        }
        
        public int GetIdleHash()
            => string.IsNullOrEmpty(OverrideIdle)
            ? AHash.IdleParameterHash
            : Animator.StringToHash(OverrideIdle);

        public UniTask<(bool status,string result)> PlayAnimAsync(int hash, string eventKey)
        {
            PlayAnim(hash);
            var timeout = 3f;
            if(_handsBehaviour is null) return _legAnimatorObserver.TryAwaitTrigger(eventKey, timeout);
            if (_handsBehaviour.TryGetValue(hash, out var anim))
            {
                timeout = anim.ExitTime;
            }

            if (_handAnimatorObserver.HasBehaviour(eventKey))
            {
                return _handAnimatorObserver.TryAwaitTrigger(eventKey, timeout);
            }
            return _legAnimatorObserver.TryAwaitTrigger(eventKey, timeout);
        }
        
        public void PlayAnim(int hash)
        {
            if(_handsBehaviour is null) return;
            if (_handsBehaviour.TryGetValue(hash, out var value))
            {
                _nextHandAnimationBehaviour = value;
            }
            if (_itemBehaviour.TryGetValue(hash, out var value1))
            {
                _nextItemAnimationBehaviour = value1;
            }

            _isPlaying = true;
        }
        
        private void PlayNext(AnimationBehaviour next)
        {
            next.CurrentExitTime = next.ExitTime;
            switch (next.AnimatorType)
            {
                case AnimatorSelect.HandsAnimator:
                    ChangeHandAnim(next);
                    break;
                case AnimatorSelect.ItemAnimator:
                    ChangeItemAnim(next);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ChangeItemAnim(AnimationBehaviour next)
        {
            var current = _currentItemAnimationBehaviour;
            _currentItemAnimationBehaviour = next;
            _itemAnimator.clip = _currentItemAnimationBehaviour.Clip;
            _itemAnimator.Play(_currentItemAnimationBehaviour.Key,PlayMode.StopAll);
            _nextItemAnimationBehaviour = null;
            _rigProvider.SetRigStatus(_currentItemAnimationBehaviour.EnableIK,"firstGrip");
            _rigProvider.SetRigStatus(_currentItemAnimationBehaviour.EnableIK,"secondGrip");
            if (_currentItemAnimationBehaviour.ReturnToIdle)
            {
                if (current == next)
                {
                    PlayAnim(GetIdleHash());
                    return;
                }
                _nextItemAnimationBehaviour = current;
            }
        }
        
        private void ChangeHandAnim(AnimationBehaviour next)
        {
            var current = _currentHandAnimationBehaviour;
            _currentHandAnimationBehaviour = next;
            if (current is not null && next.Key.Equals(current.Key) || next.BlockCrossFade)
            {
                PlayFpvAnim(_currentHandAnimationBehaviour.HashedKey);
            }
            else
            {
                CrossFpvAnim(_currentHandAnimationBehaviour.HashedKey);
            }
            _rigProvider.SetRigStatus(false,"firstGrip");
            _rigProvider.SetRigStatus(false,"secondGrip");
            _nextHandAnimationBehaviour = null;
            if (_currentHandAnimationBehaviour.ReturnToIdle)
            {
                if (current == next)
                {
                    PlayAnim(GetIdleHash());
                    return;
                }
                _nextHandAnimationBehaviour = current;
            }
        }

        private void PlayFpvAnim(int hash)
        {
            if (_handAnimator.HasState(0, hash))
            {
                _handAnimator.Play(hash,0, 0f);
            }
            if (_legAnimator.HasState(0, hash))
            {
                _legAnimator.Play(hash,0,0f);
            }
            
            _currentHash = hash;
        }
        
        private void CrossFpvAnim(int hash)
        {
            Debug.Log("Crossfade");
            if (_handAnimator.HasState(0, hash))
            {
                var stateInfo = _handAnimator.GetCurrentAnimatorStateInfo(0);
                var currentLength = stateInfo.length;
                _handAnimator.CrossFade(stateHashName: hash, 0.25f / currentLength, 0, 0.1f);
            }
            if (_legAnimator.HasState(0, hash))
            {
                _legAnimator.CrossFade(hash,0.1f,0);
            }
            
            _currentHash = hash;
        }
        
        private void UpdateBehaviour(AnimationBehaviour current, AnimationBehaviour next, float delta)
        {
            if (current is null)
            {
                if(next is null) return;
                PlayNext(next);
                return;
            }
            current.CurrentExitTime -= delta;
            if (next is null) 
                return;
            if (current.CurrentExitTime > 0 && !current.CanCancel) 
                return;
            PlayNext(next);
        }
        
        public void Update(float delta)
        {
            if(!_isPlaying)
                return;
            if(!_isInited) 
                return;
            UpdateBehaviour(_currentHandAnimationBehaviour,_nextHandAnimationBehaviour, delta);
            UpdateBehaviour(_currentItemAnimationBehaviour,_nextItemAnimationBehaviour, delta);
        }

        public void Stop(bool playEmpty = true)
        {
            _itemAnimator.Stop();
            _itemAnimator.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            if (playEmpty)
            {
                _handAnimator.Play(AHash.Empty);
            }
            _isPlaying = false;
            _nextHandAnimationBehaviour = null;
            _nextItemAnimationBehaviour = null;
            _currentHandAnimationBehaviour = null;
            _currentItemAnimationBehaviour = null;
        }

        public void Pause(bool playEmpty = true)
        {
            _itemAnimator.Stop();
            _itemAnimator.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            if (playEmpty)
            {
                _handAnimator.Play(AHash.Empty);
            }
            _isPlaying = false;
            _nextHandAnimationBehaviour = null;
            _nextItemAnimationBehaviour = null;
        }

        public void Dispose()
        {
            _handAnimatorObserver?.Dispose();
            _legAnimatorObserver?.Dispose();
        }
    }
}