using Godot;
using System;
using System.Collections.Generic;


public partial class Turret : Node
{
	[Export] public PackedScene BalaPrefab;
	[Export] public float CadenciaDisparo = 1.0f;
	// En 3D usamos LookAt que es instantáneo, 
	// pero podemos usar interpolación para suavizarlo.
	[Export] public float VelocidadRotacion = 10.0f; 

	[Export]public Area3D _rangoVision;
	[Export]public Marker3D _puntoDisparo;
	[Export]public Node3D _cabeza; // La parte que gira
	private Node3D _objetivoActual = null;
	private float _tiempoSiguienteDisparo = 0.0f;
	private List<Node3D> _enemigosEnRango = new List<Node3D>();

	public override void _Ready()
	{
		_rangoVision = GetNode<Area3D>("RangoVision");
		_cabeza = GetNode<Node3D>("Cabeza");
		_puntoDisparo = GetNode<Marker3D>("Cabeza/PuntoDisparo");

		// Conectamos señales
		_rangoVision.BodyEntered += OnCuerpoEntro;
		_rangoVision.BodyExited += OnCuerpoSalio;
	}

	public override void _Process(double delta)
	{
		ActualizarObjetivo();

		if (_objetivoActual != null && IsInstanceValid(_objetivoActual))
		{
			
			Apuntar(delta);
			
			if (Time.GetTicksMsec() / 1000.0f > _tiempoSiguienteDisparo)
			{
				Disparar();
				_tiempoSiguienteDisparo = (Time.GetTicksMsec() / 1000.0f) + CadenciaDisparo;
			}
		}
	}

	private void ActualizarObjetivo()
	{
		// Limpieza básica si el objetivo murió
		if (_objetivoActual != null && !IsInstanceValid(_objetivoActual))
		{
			 _enemigosEnRango.Remove(_objetivoActual);
			 _objetivoActual = null;
		}

		if (_objetivoActual == null && _enemigosEnRango.Count > 0)
		{
			_objetivoActual = _enemigosEnRango[0];
		}
	}

	private void Apuntar(double delta)
	{
		Vector3 posicionObjetivo = _objetivoActual.GlobalPosition;
		
		// Mantenemos la altura del objetivo igual a la cabeza para que no se incline 
		// hacia arriba/abajo si solo quieres rotación en eje Y (tipo tanque clásico).
		// Si quieres que apunte arriba/abajo (antiaéreo), comenta la siguiente línea:
		posicionObjetivo.Y = _cabeza.GlobalPosition.Y; 

		// OPCIÓN A: Rotación Instantánea (Muy fácil)
		// _cabeza.LookAt(posicionObjetivo, Vector3.Up);

		// OPCIÓN B: Rotación Suave (Más profesional)
		// Calculamos la transformación a la que queremos llegar
		Transform3D transformacionActual = _cabeza.GlobalTransform;
		Transform3D transformacionDeseada = transformacionActual.LookingAt(posicionObjetivo, Vector3.Up);
		
		// Interpolamos (Lerp) la transformación actual hacia la deseada
		_cabeza.GlobalTransform = transformacionActual.InterpolateWith(transformacionDeseada, VelocidadRotacion * (float)delta);
	}

	private void Disparar()
	{
		if (BalaPrefab == null) return;

		var bala = BalaPrefab.Instantiate<Node3D>();
		GetTree().Root.AddChild(bala);
		
		bala.GlobalPosition = _puntoDisparo.GlobalPosition;
		// Importante: Copiamos la rotación del punto de disparo
		bala.GlobalRotation = _puntoDisparo.GlobalRotation;
	}

	private void OnCuerpoEntro(Node3D body)
	{
		GD.Print($"Algo entró en el rango: {body.Name}");
		if (body.IsInGroup("Enemigos"))
		{
			GD.Print("¡Y es un enemigo! Agregado a la lista.");
			_enemigosEnRango.Add(body);
		}
		else
		{
			GD.Print("Pero NO está en el grupo 'Enemigos'. Revisa la pestaña Node.");
		}
	}

	private void OnCuerpoSalio(Node3D body)
	{
		if (_enemigosEnRango.Contains(body))
		{
			_enemigosEnRango.Remove(body);
			if (_objetivoActual == body) _objetivoActual = null;
		}
	}
}
