using Cysharp.Threading.Tasks;
using System.Threading;
using SharedUtils;
using UnityEngine;

public class TransformFollower
{
    private readonly Transform _from;
    private readonly Transform _to;
    private CancellationTokenSource _cts;

    public TransformFollower(Transform from, Transform to)
    {
        _from = from;
        _to = to;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(to.GetCancellationTokenOnDestroy());
        Internal().Forget();
    }

    public void Cancel()
    {
        if (_cts.IsCancellationRequested) return;
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public async UniTaskVoid Cancel(float delay)
    {
        var ct = _cts.Token;
        await UniTask.Delay(delay.ToSec(), cancellationToken: ct);
        Cancel();
    }
    
    private async UniTaskVoid Internal()
    {
        var ct = _cts.Token;
        while (!ct.IsCancellationRequested)
        {
            _from.position = _to.position;
            await UniTask.NextFrame(ct);
        }
    }
}
