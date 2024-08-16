using Cysharp.Threading.Tasks;
using DG.Tweening;
using RootMotion.Dynamics;

namespace Core.Entity.Characters
{
    public class CharacterRagDollManager : IRagdollManager
    {
        private readonly CharacterContext _context;
        private Tweener _puppetWeightTween;
        private bool _death;

        public CharacterRagDollManager(CharacterContext context)
        {
            _context = context;
        }

        public void SetState(RagdollState ragdollState)
        {
            
        }
        public void EnableRagDoll()
        {
            _context.BehaviourPuppet.SetState(BehaviourPuppet.State.Unpinned);
            SetPuppetWeight(0,0, true);
        }
		
        public void DisableRagDoll()
        {
            if(_death) return;
            _context.BehaviourPuppet.SetState(BehaviourPuppet.State.Puppet);
            SetPuppetWeight(1,1, true);
        }
        
        public async UniTaskVoid EnableRagDollWithFrameDelay(int frames = 10)
        {
            await UniTask.DelayFrame(frames, cancellationToken: _context.destroyCancellationToken);

            _context.BehaviourPuppet.SetState(BehaviourPuppet.State.Unpinned);
            SetPuppetWeight(0,0, true);
        }
		
        public async UniTaskVoid DisableRagDollWithFrameDelay(int frames = 10)
        {
            if(_death) return;
            await UniTask.DelayFrame(frames, cancellationToken: _context.destroyCancellationToken);
            _context.BehaviourPuppet.SetState(BehaviourPuppet.State.Puppet);
            SetPuppetWeight(1,1, true);
        }

        public void Death()
        {
            if(_death) return;
            _death = true;
            _context.PuppetMaster.state = PuppetMaster.State.Dead;
            EnableRagDoll();
        }
		
        protected virtual void SetPuppetWeight(int pinWeight, int muscleWeight, bool smooth = false, float lerpDuration = 0.35f)
        {
            if (smooth)
            {
                _puppetWeightTween.Kill();
                _puppetWeightTween = DOVirtual.Float(_context.PuppetMaster.pinWeight, pinWeight, lerpDuration, value =>
                {
                    _context.PuppetMaster.pinWeight = value;
                    _context.PuppetMaster.muscleWeight = value;
                }).SetRecyclable().SetLink(_context.gameObject);
            }
            else
            {
                _context.PuppetMaster.muscleWeight = muscleWeight;
                _context.PuppetMaster.pinWeight = pinWeight;
            }
        }
    }
}