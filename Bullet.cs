using Godot;
using System;

public partial class Bullet : Area3D
{
	[Export] public float Velocidad = 20.0f; // Unidades por segundo
	[Export] public int Daño = 10;
	
	// Tiempo de vida para que no se acumulen infinitamente si fallan
	[Export] public float TiempoVida = 3.0f; 

	public override void _Ready()
	{
		// Destruir la bala después de X segundos
		GetTree().CreateTimer(TiempoVida).Timeout += () => QueueFree();
		
		// Conectar choque
		BodyEntered += OnImpacto;
	}

	public override void _Process(double delta)
	{
		// MOVER HACIA ADELANTE EN 3D
		// En Godot, Forward es -Z. 
		// GlobalTransform.Basis.Z es el vector Z local.
		// Restamos para ir hacia el -Z local del objeto.
		
		GlobalPosition -= GlobalTransform.Basis.Z * Velocidad * (float)delta;
	}

	private void OnImpacto(Node3D body)
	{
		if (body.IsInGroup("Enemigos"))
		{
			if (body.HasMethod("RecibirDaño"))
			{
				body.Call("RecibirDaño", Daño);
			}
			QueueFree();
		}
	}
}
