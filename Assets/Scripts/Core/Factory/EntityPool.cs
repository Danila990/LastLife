using Core.Entity;
using Core.Entity.Repository;
using uPools;
using Object = UnityEngine.Object;

namespace Core.Factory
{
    public sealed class EntityPool<T> : ObjectPoolBase<T> where T : EntityContext
    {
        private readonly IObjectFactory _factory;
        private readonly IEntityRepository _repository;
        private readonly string _key;

        public EntityPool(
            IObjectFactory factory,
            IEntityRepository repository,
            string key
        )
        {
            _factory = factory;
            _repository = repository;
            _key = key;
        }

        protected override T CreateInstance()
        {
            return _factory.CreateObject(_key) as T;
        }

        protected override void OnDestroy(T instance)
        {
            //_repository.RemoveEntity(instance);
            Object.Destroy(instance.gameObject);
        }

        protected override void OnRent(T instance)
        {
            instance.gameObject.SetActive(true);
            //_repository.AddEntity(instance);
        }

        protected override void OnReturn(T instance)
        {
            //_repository.RemoveEntity(instance);
            instance.gameObject.SetActive(false);
        }
    }
}