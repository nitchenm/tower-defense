using Godot;
using System;

public partial class Spawner : Path3D
{
	[Export] public PackedScene EnemigoPrefab; // Arrastra tu escena de Enemigo aquí
	[Export] public Path3D CaminoNivel;        // Asigna el nodo Path3D de la escena

	// Ejemplo simple: Spawnear cada 2 segundos
	private Timer _timer;

	public override void _Ready()
	{
		_timer = new Timer();
		AddChild(_timer);
		_timer.WaitTime = 2.0f;
		_timer.Timeout += SpawnearEnemigo;
		_timer.Start();
	}

	private void SpawnearEnemigo()
	{
		if (EnemigoPrefab == null || CaminoNivel == null) return;

		// 1. Instanciar la escena del enemigo (que es un PathFollow3D)
		var nuevoEnemigo = EnemigoPrefab.Instantiate<PathFollow3D>();

		// 2. Añadirlo COMO HIJO del Path3D.
		// Esto automáticamente lo coloca al inicio de la línea.
		CaminoNivel.AddChild(nuevoEnemigo);
		
		// 3. Asegurar que empiece en el metro 0
		nuevoEnemigo.Progress = 0;
	}
}
