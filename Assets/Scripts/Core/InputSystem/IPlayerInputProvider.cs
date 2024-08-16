using Core.Actions;
using Core.Services.Input;

namespace Core.InputSystem
{
    public interface IPlayerInputProvider
    {
        void UseListener(IPlayerInputListener listener);
    }

    public interface IPlayerInputListener
    {
        PlayerInputDto InputDto { get; }
        void OnSprintDown();
        void OnJumpDown();
        void OnJumpUp();
        void OnMoveDown();
        void OnAimDown();
        
        void OnAction(ActionKey action, InputKeyType keyType);
        void OnActionGet(ActionKey action, bool status);
    }
}