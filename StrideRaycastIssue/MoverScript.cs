using Stride.BepuPhysics;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using System;
using System.Linq;

namespace StrideRaycastIssue;

public class MoverScript : SyncScript
{
    public Entity Sphere1Static { get; set; }
    public Entity Sphere2Body { get; set; }

    public TimeSpan TranslationTime { get; set; }

    private TimeSpan _elapsed1 = TimeSpan.Zero;
    private TimeSpan _elapsed2 = TimeSpan.Zero;
    private Vector3 _startPositionSphere1 = Vector3.Zero;
    private Vector3 _startPositionSphere2 = Vector3.Zero;
    private Vector3 _targetPositionSphere1 = Vector3.Zero;
    private Vector3 _targetPositionSphere2 = Vector3.Zero;

    public override void Update()
    {
        DebugText.Print("""
            Left click to move golden "Body" sphere. Middle click to move metal "Static" sphere.
            Hovering on "Static" sphere will display the hovering wherever the sphere is located.
            Hovering on "Body" sphere will display the hovering only when the sphere is located at its initial position (near golden cylinder).
            """, new Int2(100, 15));

        // on left mouse click, move the sphere with BodyComponent to the mouse position
        // on middle mouse click, move the sphere with StaticComponent to the mouse position
        if (Input.IsMouseButtonPressed(Stride.Input.MouseButton.Left))
            MoveSphere(Sphere2Body, ref _startPositionSphere2, ref _targetPositionSphere2, ref _elapsed2);
        else if (Input.IsMouseButtonPressed(Stride.Input.MouseButton.Middle))
            MoveSphere(Sphere1Static, ref _startPositionSphere1, ref _targetPositionSphere1, ref _elapsed1);

        // update sphere positions
        if (_targetPositionSphere1 != Vector3.Zero)
        {
            _elapsed1 += Game.UpdateTime.Elapsed;
            if (_elapsed1 >= TranslationTime)
            {
                Sphere1Static.Transform.Position = _targetPositionSphere1;
                _targetPositionSphere1 = Vector3.Zero;
            }
            else
            {
                Sphere1Static.Transform.Position = Vector3.Lerp(_startPositionSphere1, _targetPositionSphere1, (float)(_elapsed1 / TranslationTime));
            }
        }

        if (_targetPositionSphere2 != Vector3.Zero)
        {
            var bodyComponent = Sphere2Body.Get<BodyComponent>();

            _elapsed2 += Game.UpdateTime.Elapsed;
            if (_elapsed2 >= TranslationTime)
            {
                Sphere2Body.Get<BodyComponent>().Position = _targetPositionSphere2;
                _targetPositionSphere2 = Vector3.Zero;
            }
            else
            {
                // maybe this is the issue? how to correctly move a BodyComponent?
                // Do I need to set the linear-velocity instead? like this?
                ////var currentPosition = bodyComponent.Position;
                ////var direction = Vector3.Normalize(_targetPositionSphere2 - currentPosition);
                ////var distance = Vector3.Distance(currentPosition, _targetPositionSphere2);
                ////var velocity = direction * distance / (float)TranslationTime.TotalSeconds;
                ////bodyComponent.LinearVelocity = velocity;
                bodyComponent.Position = Vector3.Lerp(_startPositionSphere2, _targetPositionSphere2, (float)(_elapsed2 / TranslationTime));
            }
        }
    }

    private void MoveSphere(Entity sphere, ref Vector3 startPosition, ref Vector3 targetPosition, ref TimeSpan elapsedTime)
    {
        var camera = this.SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(x => x.Get<CameraComponent>() != null).Get<CameraComponent>();

        var simulation = Entity.GetSimulation();
        if (simulation is null)
            throw new InvalidOperationException("Bepu Physics not initialized. Add at least one collider to the scene.");

        var backBuffer = GraphicsDevice.Presenter.BackBuffer;
        var viewPort = new Viewport(0, 0, backBuffer.Width, backBuffer.Height);
        var nearPosition = viewPort.Unproject(new Vector3(Input.AbsoluteMousePosition, 0), camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);
        var farPosition = viewPort.Unproject(new Vector3(Input.AbsoluteMousePosition, 1.0f), camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);
        if (simulation.RayCast(nearPosition, farPosition, 1000, out var hitInfo))
        {
            targetPosition = new Vector3(hitInfo.Point.X, 0.5f, hitInfo.Point.Z);
            startPosition = sphere.Transform.Position;
            elapsedTime = TimeSpan.Zero;
        }
    }
}
