"""Exporta o model sheet do Soldado ASDA (Art/ModelSheets/Soldado-ASDA.blend) para a Unity.

Uso:
    blender -b Art/ModelSheets/Soldado-ASDA.blend --python tools/blender/exportar_soldado.py

Aplica os modificadores, junta todas as pecas em uma unica malha (Soldado_ASDA),
coloca a origem nos pes e exporta Between fire and water/Assets/Models/SoldadoASDA.fbx.
O arquivo .blend original nao e alterado.
"""
import os

import bpy
from mathutils import Vector

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
FBX_OUT = os.path.join(ROOT, "Between fire and water", "Assets", "Models", "SoldadoASDA.fbx")

meshes = [o for o in bpy.context.scene.objects if o.type == "MESH" and o.visible_get()]
bpy.ops.object.select_all(action="DESELECT")
for o in meshes:
    o.hide_select = False
    o.select_set(True)
bpy.context.view_layer.objects.active = meshes[0]
bpy.ops.object.convert(target="MESH")  # aplica bevel/weighted normal
bpy.ops.object.join()
body = bpy.context.active_object
body.name = "Soldado_ASDA"
body.parent = None

# origem nos pes, centralizado em X/Y
pts = [body.matrix_world @ Vector(c) for c in body.bound_box]
min_z = min(p.z for p in pts)
cx = (min(p.x for p in pts) + max(p.x for p in pts)) / 2
cy = (min(p.y for p in pts) + max(p.y for p in pts)) / 2
bpy.context.scene.cursor.location = (cx, cy, min_z)
bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
body.location = (0, 0, 0)
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

bpy.ops.object.select_all(action="DESELECT")
body.select_set(True)
os.makedirs(os.path.dirname(FBX_OUT), exist_ok=True)
bpy.ops.export_scene.fbx(
    filepath=FBX_OUT,
    use_selection=True,
    apply_unit_scale=True,
    apply_scale_options="FBX_SCALE_ALL",
    axis_forward="-Z",
    axis_up="Y",
    object_types={"MESH"},
    bake_anim=False,
)
print("OK_SOLDADO", len(body.data.vertices), [round(d, 2) for d in body.dimensions], [m.name for m in body.data.materials])
