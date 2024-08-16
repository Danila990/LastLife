using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Core.Entity.InteractionLogic.Interactions
{
    public class MonoInteractProvider : MonoBehaviour, IInteractableProvider, IContextAcceptor
    {
        [SerializeField] protected EntityContext _context;
        [SerializeField] private List<InteractionData> _interactions = new();
        [SerializeField] private bool _unionInteractions;
        public uint Uid => GetUid();
        public EntityContext Context => _context;
        public void Inject(IObjectResolver resolver)
        {
            foreach (var interaction in _interactions)
            {
                if(interaction.InteractionType != InteractionType.MonoInteraction) 
                    continue;
                resolver.Inject(interaction.GetInteraction());
            }
        }
        
        private void Start()
        {
            UnionInteractions();
        }
        
        private void UnionInteractions()
        {
            if (!_unionInteractions)
                return;
            if (!_context || _context.EntityInteractions.Count <= 0)
                return;
            
            _interactions.AddRange(_context.EntityInteractions);
            _interactions.Sort(Comparison);
        }

        private static int Comparison(InteractionData x, InteractionData y)
        {
            return y.Priority.CompareTo(x.Priority);
        }

        public InteractionResultMeta Visit(IInteractorVisiter visiter, ref InteractionCallMeta meta)
        {
            foreach (var interaction in _interactions)
            {
                //Debug.Log($"Proxy Visit {interaction}");
                var inter = interaction.GetInteraction();
                SetContext(inter);
                var res = inter.Visit(visiter, ref meta);
                if (res.Interacted) 
                    return res;
            }
            
            if (_context && !_unionInteractions)
            {
                return _context.Visit(visiter, ref meta);
            }
            return StaticInteractionResultMeta.Default;
        }

        protected virtual uint GetUid()
        {
            return _context != null ? _context.Uid : 0;
        }

        protected virtual void SetContext(IInteractableContexted inter)
        {
            inter.SetEntityContext(_context);
        }
        
        public void SetContext(EntityContext context)
        {
            _context = context;
        }
        
        #if UNITY_EDITOR
        [Button]
        private void BindInteractions()
        {
            var empty = _interactions.FindAll(x => x.GetInteraction().Equals(null));
            foreach (var mt in empty)
            {
                _interactions.Remove(mt);
            }
            
            var interactions = GetComponents<AbstractMonoInteraction>();
            foreach (var interaction in interactions)
            {
                if(_interactions.Count(x=> x != null && x.GetInteraction().Equals(interaction))>0) continue;
                _interactions.Add(new InteractionData {MonoInteraction = interaction,InteractionType = InteractionType.MonoInteraction});
            }
            
            
        }
        
        [Button]
        private void ClearEmpty()
        {
            _interactions.RemoveAll(x =>  x.GetInteraction() == null);
        }
        #endif
    }
}