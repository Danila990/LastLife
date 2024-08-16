using System;
using Adv.Services.Interfaces;
using Core.ResourcesSystem;
using Core.ResourcesSystem.Interfaces;
using Core.Timer;
using Ticket;
using Ui.Widget;
using UniRx;
using UnityEngine.UI;

namespace Core.Entity.InteractionLogic.Interactions
{
    public class WorldButtonPresenter : IDisposable
    {
        protected readonly Action Callback;
        private IDisposable _disposable;

        public WorldButtonPresenter(in Action callback)
        {
            Callback = callback;
        }

        public void Attach(ButtonWidget widget)
        {
            _disposable?.Dispose();
            _disposable = widget.Button.OnClickAsObservable().Subscribe(OnClick);
        }

        private void OnClick(Unit _)
        {
            OnCallback();
        }

        protected virtual void OnCallback()
        {
            Callback?.Invoke();
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
    
    public class AdvWorldButtonPresenter : WorldButtonPresenter
    {
        private readonly IAdvService _advService;
        private readonly string _advMeta;

        public AdvWorldButtonPresenter(
            IAdvService advService,
            Action callbackListener,
            string advMeta
        ) : base(callbackListener)
        {
            _advService = advService;
            _advMeta = advMeta;
        }

        protected override void OnCallback()
        {
            _advService.RewardRequest(Callback, _advMeta);
        }
    }
    
    public class TicketWorldButtonPresenter : WorldButtonPresenter
    {
        private readonly ITicketService _ticketService;
        private readonly string _advMeta;

        public TicketWorldButtonPresenter(
            ITicketService ticketService,
            Action callbackListener,
            string advMeta
        ) : base(callbackListener)
        {
            _ticketService = ticketService;
            _advMeta = advMeta;
        }

        protected override void OnCallback()
        {
            _ticketService.TryUseTicket(Callback, _advMeta);
        }
    }
    
    public class GoldenTicketWorldButtonPresenter : WorldButtonPresenter
    {
        private readonly IResourcesService _resourcesService;
        private readonly string _singleWordMeta;
        private readonly int _amount;

        public GoldenTicketWorldButtonPresenter(
            IResourcesService resourcesService,
            Action callbackListener,
            string singleWordMeta,
            int amount
        ) : base(callbackListener)
        {
            _resourcesService = resourcesService;
            _singleWordMeta = singleWordMeta;
            _amount = amount;
        }

        protected override void OnCallback()
        {
            var meta = new ResourceEventMetaData(ResourceItemTypes.OTHERS, _singleWordMeta);
            
            if (_resourcesService.TrySpendResource(ResourceType.GoldTicket, _amount, meta))
                Callback();
        }
    }
    
    public class WorldTimerPresenter : IDisposable
    {
        protected readonly Action Callback;
        private CompositeDisposable _disposable;
        private ITimer _timer;
        private Image _filledImage;

        public WorldTimerPresenter(in Action callback)
        {
            Callback = callback;
        }

        public void Attach(Image filledImage, ITimer timer)
        {
            _filledImage = filledImage;
            _timer = timer;
            _disposable?.Dispose();
            _disposable = new CompositeDisposable();
            _timer.ElapsedTime.Subscribe(OnTimerTick).AddTo(_disposable);
            _timer.OnEnd.Subscribe(_ => OnCallback()).AddTo(_disposable);
        }

        private void OnTimerTick(TimeSpan timeSpan)
        {
            _filledImage.fillAmount = (float)(timeSpan.TotalSeconds / _timer.TotalTime.TotalSeconds);
        }

        protected virtual void OnCallback()
        {
            Callback?.Invoke();
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}