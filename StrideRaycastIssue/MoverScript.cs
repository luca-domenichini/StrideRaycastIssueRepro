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
        // on left mouse click, move the sphere with BodyComponent to the mouse position
        if (Input.IsMouseButtonPressed(Stride.Input.MouseButton.Left))
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
                _targetPositionSphere2 = new Vector3(hitInfo.Point.X, 0.5f, hitInfo.Point.Z);
                _startPositionSphere2 = Sphere2Body.Transform.Position;
                _elapsed2 = TimeSpan.Zero;
            }
        }

        // move spheres to random positions
        if (_targetPositionSphere1 == Vector3.Zero)
        {
            _targetPositionSphere1 = new Vector3((float)(Random.Shared.NextDouble() * 10 - 5), 0.5f, (float)(Random.Shared.NextDouble() * 10 - 5));
            _startPositionSphere1 = Sphere1Static.Transform.Position;
            _elapsed1 = TimeSpan.Zero;
        }

        if (_targetPositionSphere2 == Vector3.Zero)
        {
            _targetPositionSphere2 = new Vector3((float)(Random.Shared.NextDouble() * 10 - 5), 0.5f, (float)(Random.Shared.NextDouble() * 10 - 5));
            _startPositionSphere2 = Sphere2Body.Transform.Position;
            _elapsed2 = TimeSpan.Zero;
        }

        // update sphere positions
        _elapsed1 += Game.UpdateTime.Elapsed;
        if (_elapsed1 >= TranslationTime)
        {
            Sphere1Static.Transform.Position = _targetPositionSphere1;
            _targetPositionSphere1 = Vector3.Zero;
        }
        else
        {
            Sphere1Static.Transform.Position = Vector3.Lerp(_startPositionSphere1, _targetPositionSphere1, (float)((float)(_elapsed1.TotalMilliseconds) / TranslationTime.TotalMilliseconds));
        }

        _elapsed2 += Game.UpdateTime.Elapsed;
        if (_elapsed2 >= TranslationTime)
        {
            Sphere2Body.Get<BodyComponent>().Position = _targetPositionSphere2;
            _targetPositionSphere2 = Vector3.Zero;
        }
        else
        {
            Sphere2Body.Get<BodyComponent>().Position = Vector3.Lerp(_startPositionSphere2, _targetPositionSphere2, (float)((float)(_elapsed2.TotalMilliseconds) / TranslationTime.TotalMilliseconds));
        }
    }
}
