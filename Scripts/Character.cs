using Godot;
using System;

public partial class Character : CharacterBody2D
{
    public const float Speed = 300.0f;
    public const float JumpVelocity = -400.0f;
    public const float CameraSpeed = 300.0f;

    private Tween cameraTween;

    private AnimatedSprite2D blobster;
    private Camera2D camera;

    public override void _Ready()
    {
        blobster = GetNode<AnimatedSprite2D>("Blobster");
        camera = GetParent().GetNode<Camera2D>("Camera2D");

        camera.MakeCurrent();
    }

    public float cameraEdgeThreshold = 16.0f;
    private bool isTrackingPlayer = false;
    public override void _Process(double delta)
    {
        Vector2 screenSize = GetViewportRect().Size;
        Vector2 camPos = camera.GlobalPosition;
        Vector2 halfScreen = screenSize / 2f;

        float leftEdge = camPos.X - halfScreen.X + cameraEdgeThreshold;
        float rightEdge = camPos.X + halfScreen.X - cameraEdgeThreshold;

        if ((GlobalPosition.X < leftEdge || GlobalPosition.X > rightEdge) && !isTrackingPlayer)
        {
            isTrackingPlayer = true;

            Vector2 targetPos = new Vector2(GlobalPosition.X + (GlobalPosition.X < leftEdge ? 500 : -500), camPos.Y);

            cameraTween?.Kill();

            cameraTween = GetTree().CreateTween();
            cameraTween.TweenProperty(camera, "global_position", targetPos, 0.2f)
                        .SetTrans(Tween.TransitionType.Linear)
                        .SetEase(Tween.EaseType.InOut);

            
            cameraTween.Finished += () => isTrackingPlayer = false;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        blobster.Play("bounce");

        if (Input.IsActionJustPressed("Jump"))
        {
            if (IsOnFloor() && blobster.Frame == 0)
            {
                Velocity = new Vector2(Velocity.X, JumpVelocity);
                GD.Print("JUMPED");
            }
            else
            {
                GD.Print($"Is on floor = {IsOnFloor()} while blobster frame = {blobster.Frame}");
            }
        }

        float direction = Input.GetActionStrength("Right") - Input.GetActionStrength("Left");

        if (Mathf.Abs(direction) > 0.01f)
        {
            Velocity = new Vector2(direction * Speed, Velocity.Y);
            blobster.FlipH = direction < 0;
        }
        else
        {
            Velocity = new Vector2(Mathf.MoveToward(Velocity.X, 0, Speed), Velocity.Y);
        }


        MoveAndSlide();
    }
}
