using Godot;
using System;

public partial class Enemy : CharacterBody3D
{
	[Export] public float Velocidad = 5.0f;
	[Export] public int SaludMaxima = 30;

	[Export] public Sprite3D BarraVidaVisual;
	private Vector3 _escalaOriginalBarra;
	private int _saludActual;
	private PathFollow3D _padrePathFollow;

	public override void _Ready()
	{
		_saludActual = SaludMaxima;
		_padrePathFollow = GetParentOrNull<PathFollow3D>();

		// Guardamos el tamaño inicial de la barra verde (ej: el 1.0 en X)
		if (BarraVidaVisual != null)
		{
			_escalaOriginalBarra = BarraVidaVisual.Scale;
		}

		if (_padrePathFollow == null)
		{
			SetPhysicsProcess(false);
		}
		
		if (!IsInGroup("Enemigos")) AddToGroup("Enemigos");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_padrePathFollow != null)
		{
			// Moverse a lo largo del camino.
			// "Progress" es la distancia en metros recorrida en el camino.
			_padrePathFollow.Progress += Velocidad * (float)delta;

			// Si llegamos al final del camino (Ratio es 1.0 significa 100%)
			if (_padrePathFollow.ProgressRatio >= 1.0f)
			{
				LlegarAlFinal();
			}
		}
	}

	// Este método es llamado por la Bala cuando impacta
	public void RecibirDaño(int cantidad)
	{
		_saludActual -= cantidad;
		
		// --- ACTUALIZAR BARRA ---
		ActualizarBarraVida();

		if (_saludActual <= 0)
		{
			Morir();
		}
	}
	private void ActualizarBarraVida()
	{
		if (BarraVidaVisual == null) return;

		// Calculamos porcentaje (0.0 a 1.0)
		// Casteamos a float porque int/int da 0
		float porcentaje = (float)_saludActual / (float)SaludMaxima;
		
		// Evitamos valores negativos
		if (porcentaje < 0) porcentaje = 0;

		// Modificamos SOLO la escala X, manteniendo Y y Z originales
		BarraVidaVisual.Scale = new Vector3(
			_escalaOriginalBarra.X * porcentaje, 
			_escalaOriginalBarra.Y, 
			_escalaOriginalBarra.Z
		);
		
		// Opcional: Cambiar color a amarillo/rojo si está baja
		if (porcentaje < 0.3f)
			BarraVidaVisual.Modulate = new Color(1, 0, 0); // Rojo crítico
		else if (porcentaje < 0.6f)
			BarraVidaVisual.Modulate = new Color(1, 1, 0); // Amarillo
	}

	private void Morir()
	{
		// Aquí podrías instanciar una explosión o dar dinero al jugador
		
		// Importante: Queremos borrar TODO el enemigo, incluyendo el PathFollow padre.
		// Si solo borramos "this" (el cuerpo), el PathFollow vacío seguirá moviéndose.
		_padrePathFollow.QueueFree(); 
	}

	private void LlegarAlFinal()
	{
		// Lógica de restar vidas al jugador
		GD.Print("¡El enemigo llegó a la base!");
		
		// Destruimos al enemigo
		_padrePathFollow.QueueFree();
	}
}
