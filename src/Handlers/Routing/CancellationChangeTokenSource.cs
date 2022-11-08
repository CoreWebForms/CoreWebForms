// MIT License.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Primitives;

namespace System.Web.Routing;

internal sealed class CancellationChangeTokenSource : IChangeToken, IDisposable
{
    private CancellationChangeToken _token;
    private CancellationTokenSource _cts;

    private State _state;

    public CancellationChangeTokenSource()
    {
        Init();
    }

    bool IChangeToken.ActiveChangeCallbacks => _token.ActiveChangeCallbacks;

    bool IChangeToken.HasChanged => _token.HasChanged;

    IDisposable IChangeToken.RegisterChangeCallback(Action<object> callback, object state)
        => _token.RegisterChangeCallback(callback, state);

    public IDisposable Pause()
    {
        _state = State.Paused;
        return this;
    }

    public void Reset()
    {
        if (_state is State.Paused)
        {
            _state = State.PausedChanges;
            return;
        }

        if (_state is State.PausedChanges)
        {
            return;
        }

        ResetInternal();
    }

    private void ResetInternal()
    {
        var previous = _cts;

        Init();

        previous.Cancel();
        previous.Dispose();
    }

    [MemberNotNull(nameof(_token), nameof(_cts))]
    private void Init()
    {
        _cts = new CancellationTokenSource();
        _token = new CancellationChangeToken(_cts.Token);
    }

    public void Dispose()
    {
        if (_state is State.PausedChanges)
        {
            ResetInternal();
        }

        _state = State.None;
    }

    private enum State
    {
        None,
        Paused,
        PausedChanges,
    }
}
