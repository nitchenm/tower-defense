extends GridMap

# 1. CARGAMOS LA ESCENA DE LA TORRE
# Arrastra tu archivo 'torre_base.tscn' desde el sistema de archivos 
# y suéltalo aquí donde dice "res://..." si el nombre es diferente.
var escena_torre = preload("res://torre_base.tscn")

var propiedades_terreno = {
	0: { "nombre": "Suelo Nieve", "caminable": false, "construible": true },
	1: { "nombre": "Camino Recto", "caminable": true, "construible": false },
	2: { "nombre": "Curva", "caminable": true, "construible": false },
}

func _unhandled_input(event):
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		var camera = get_viewport().get_camera_3d()
		var origen = camera.project_ray_origin(event.position)
		var direccion = camera.project_ray_normal(event.position)
		var plano_suelo = Plane(Vector3.UP, 0)
		var punto_choque = plano_suelo.intersects_ray(origen, direccion)
		
		if punto_choque:
			var celda = local_to_map(punto_choque)
			var id_tile = get_cell_item(celda)
			
			if propiedades_terreno.has(id_tile):
				var info = propiedades_terreno[id_tile]
				if info.construible:
					# Llamamos a la función para construir
					construir_torre(celda)
				else:
					print("Aquí no se puede construir")

func construir_torre(celda_grilla):
	var nueva_torre = escena_torre.instantiate()
	
	# Obtenemos el centro de la celda
	var posicion_mundo = map_to_local(celda_grilla)
	
	# --- CORRECCIÓN DE ALTURA ---
	# Sumamos valor al eje Y. Prueba con 0.5 o 1.0 dependiendo de tus assets.
	# Si con 0.5 sigue parpadeando (z-fighting), prueba 0.6.
	posicion_mundo.y += 0.5 
	
	nueva_torre.position = posicion_mundo
	add_child(nueva_torre)
	
	print("Torre construida en: ", celda_grilla)
