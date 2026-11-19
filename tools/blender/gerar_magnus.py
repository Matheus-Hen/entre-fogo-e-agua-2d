"""Gera o modelo 3D low-poly de Magnus, com armature e animacoes (Idle, Walk, Cast).

Uso (o Blender nao precisa estar aberto):
    blender -b --factory-startup --python tools/blender/gerar_magnus.py

Saidas:
    Art/Modelos3D/Magnus.blend                       (fonte editavel)
    Between fire and water/Assets/Models/Magnus.fbx  (importado pela Unity)

Referencia visual: Art/ModelSheets/Magnus/magnus-vistas-construcao.png
(jaqueta azul-marinho com listras laranja e ciano, camiseta clara, calca cinza,
 tenis brancos, cabelo castanho bagunçado).
"""
import math
import os
import random

import bpy

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
BLEND_OUT = os.path.join(ROOT, "Art", "Modelos3D", "Magnus.blend")
FBX_OUT = os.path.join(ROOT, "Between fire and water", "Assets", "Models", "Magnus.fbx")

random.seed(7)

# ---------------------------------------------------------------- cena limpa
bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.render.fps = 30


def material(name, rgb, rough=0.8, emit=None):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*rgb, 1.0)
    bsdf.inputs["Roughness"].default_value = rough
    mat.diffuse_color = (*rgb, 1.0)
    return mat


M = {
    "pele": material("Magnus_Pele", (0.93, 0.70, 0.55)),
    "cabelo": material("Magnus_Cabelo", (0.20, 0.11, 0.06)),
    "jaqueta": material("Magnus_Jaqueta", (0.05, 0.17, 0.33)),
    "camiseta": material("Magnus_Camiseta", (0.92, 0.90, 0.84)),
    "calca": material("Magnus_Calca", (0.25, 0.27, 0.31)),
    "tenis": material("Magnus_Tenis", (0.95, 0.95, 0.93)),
    "solado": material("Magnus_Solado", (0.45, 0.55, 0.68)),
    "laranja": material("Magnus_ListraLaranja", (0.95, 0.42, 0.08)),
    "ciano": material("Magnus_ListraCiano", (0.15, 0.75, 0.95)),
    "olho": material("Magnus_Olho", (0.08, 0.05, 0.04)),
}

parts = []


def add_part(prim, name, loc, scale, mat, bone, rot=(0, 0, 0), segments=10):
    if prim == "cube":
        bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    elif prim == "cyl":
        bpy.ops.mesh.primitive_cylinder_add(vertices=segments, radius=0.5, depth=1, location=loc, rotation=rot)
    elif prim == "sphere":
        bpy.ops.mesh.primitive_uv_sphere_add(segments=segments + 2, ring_count=segments - 2, radius=0.5, location=loc)
    elif prim == "ico":
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=0.5, location=loc)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    obj.data.materials.append(mat)
    vg = obj.vertex_groups.new(name=bone)
    vg.add(list(range(len(obj.data.vertices))), 1.0, "REPLACE")
    if prim in ("sphere", "ico", "cyl"):
        for p in obj.data.polygons:
            p.use_smooth = prim != "cyl"
    parts.append(obj)
    return obj


# Magnus encara -Y (vista frontal do Blender). Esquerda dele = +X.
for side, sx in (("L", 1), ("R", -1)):
    x = 0.09 * sx
    # pernas
    add_part("cyl", f"Coxa.{side}", (x, 0, 0.68), (0.15, 0.16, 0.42), M["calca"], f"thigh.{side}")
    add_part("cyl", f"Canela.{side}", (x, 0, 0.30), (0.13, 0.14, 0.42), M["calca"], f"shin.{side}")
    add_part("cube", f"Tenis.{side}", (x, -0.04, 0.05), (0.13, 0.27, 0.09), M["tenis"], f"shin.{side}")
    add_part("cube", f"Solado.{side}", (x, -0.04, 0.008), (0.135, 0.28, 0.02), M["solado"], f"shin.{side}")
    # bracos (leve pose A)
    ax = 0.235 * sx
    tilt = math.radians(8) * sx
    add_part("cyl", f"Braco.{side}", (ax, 0, 1.20), (0.12, 0.12, 0.30), M["jaqueta"], f"upper_arm.{side}", rot=(0, tilt, 0))
    add_part("cyl", f"Antebraco.{side}", (ax + 0.035 * sx, 0, 0.94), (0.105, 0.105, 0.26), M["jaqueta"], f"forearm.{side}", rot=(0, tilt, 0))
    add_part("cyl", f"Punho.{side}", (ax + 0.05 * sx, 0, 0.82), (0.115, 0.115, 0.05), M["jaqueta"], f"forearm.{side}")
    add_part("sphere", f"Mao.{side}", (ax + 0.055 * sx, 0, 0.74), (0.085, 0.06, 0.12), M["pele"], f"forearm.{side}")
    # olhos
    add_part("sphere", f"Olho.{side}", (0.045 * sx, -0.105, 1.535), (0.03, 0.015, 0.038), M["olho"], "head")

# quadril e tronco
add_part("cube", "Quadril", (0, 0, 0.90), (0.30, 0.19, 0.16), M["calca"], "hips")
add_part("cyl", "Camiseta", (0, 0, 1.08), (0.30, 0.20, 0.30), M["camiseta"], "spine")
add_part("cube", "Jaqueta.Costas", (0, 0.025, 1.17), (0.37, 0.17, 0.36), M["jaqueta"], "chest")
add_part("cube", "Jaqueta.Lapela.L", (0.12, -0.075, 1.17), (0.12, 0.05, 0.36), M["jaqueta"], "chest")
add_part("cube", "Jaqueta.Lapela.R", (-0.12, -0.075, 1.17), (0.12, 0.05, 0.36), M["jaqueta"], "chest")
add_part("cube", "Jaqueta.Barra", (0, 0.0, 1.0), (0.38, 0.21, 0.05), M["jaqueta"], "spine")
add_part("cube", "Listra.Laranja", (0.07, -0.103, 1.17), (0.016, 0.01, 0.34), M["laranja"], "chest")
add_part("cube", "Listra.Ciano", (0.095, -0.103, 1.17), (0.016, 0.01, 0.34), M["ciano"], "chest")
add_part("cyl", "Gola", (0, 0.01, 1.37), (0.20, 0.17, 0.08), M["jaqueta"], "chest")
add_part("cube", "Ombros", (0, 0.02, 1.32), (0.48, 0.17, 0.07), M["jaqueta"], "chest")
add_part("cyl", "Pescoco", (0, 0, 1.40), (0.09, 0.09, 0.08), M["pele"], "neck")
head = add_part("sphere", "Cabeca", (0, -0.01, 1.52), (0.21, 0.22, 0.25), M["pele"], "head")
add_part("sphere", "Orelha.L", (0.105, 0.0, 1.51), (0.04, 0.05, 0.06), M["pele"], "head")
add_part("sphere", "Orelha.R", (-0.105, 0.0, 1.51), (0.04, 0.05, 0.06), M["pele"], "head")

# cabelo bagunçado: icosfera deslocada aleatoriamente
hair = add_part("ico", "Cabelo", (0, 0.02, 1.585), (0.25, 0.26, 0.22), M["cabelo"], "head")
for v in hair.data.vertices:
    co = v.co
    if co.z < -0.03 and co.y < 0.0:
        co.z += 0.03  # libera a testa
    if co.z > -0.02:
        bump = random.uniform(0.0, 0.045)
        co.x *= 1 + bump
        co.y *= 1 + bump
        co.z += bump * 0.8
# franja
add_part("cube", "Franja", (0, -0.095, 1.625), (0.20, 0.05, 0.06), M["cabelo"], "head", rot=(math.radians(-20), 0, 0))

# ---------------------------------------------------------------- junta malha
bpy.ops.object.select_all(action="DESELECT")
for p in parts:
    p.select_set(True)
bpy.context.view_layer.objects.active = parts[0]
bpy.ops.object.join()
body = bpy.context.active_object
body.name = "Magnus_Corpo"
body.data.name = "Magnus_Corpo"

# ---------------------------------------------------------------- armature
bpy.ops.object.armature_add(location=(0, 0, 0))
arm = bpy.context.active_object
arm.name = "Magnus_Rig"
arm.data.name = "Magnus_Rig"
bpy.ops.object.mode_set(mode="EDIT")
eb = arm.data.edit_bones
eb.remove(eb[0])


def bone(name, head, tail, parent=None):
    b = eb.new(name)
    b.head = head
    b.tail = tail
    b.roll = 0
    if parent:
        b.parent = eb[parent]
    return b


bone("root", (0, 0, 0), (0, 0.25, 0))
bone("hips", (0, 0, 0.84), (0, 0, 0.98), "root")
bone("spine", (0, 0, 0.98), (0, 0, 1.14), "hips")
bone("chest", (0, 0, 1.14), (0, 0, 1.36), "spine")
bone("neck", (0, 0, 1.36), (0, 0, 1.43), "chest")
bone("head", (0, 0, 1.43), (0, 0, 1.70), "neck")
for side, sx in (("L", 1), ("R", -1)):
    bone(f"upper_arm.{side}", (0.21 * sx, 0, 1.34), (0.255 * sx, 0, 1.06), "chest")
    bone(f"forearm.{side}", (0.255 * sx, 0, 1.06), (0.29 * sx, 0, 0.70), f"upper_arm.{side}")
    bone(f"thigh.{side}", (0.09 * sx, 0, 0.88), (0.09 * sx, 0, 0.49), "hips")
    bone(f"shin.{side}", (0.09 * sx, 0, 0.49), (0.09 * sx, 0, 0.06), f"thigh.{side}")
bpy.ops.object.mode_set(mode="OBJECT")

body.parent = arm
mod = body.modifiers.new("Armature", "ARMATURE")
mod.object = arm

# ---------------------------------------------------------------- animacoes
for pb in arm.pose.bones:
    pb.rotation_mode = "XYZ"


def key_pose(frame, rots, hips_z=0.0):
    for pb in arm.pose.bones:
        pb.rotation_euler = (0, 0, 0)
        pb.location = (0, 0, 0)
    for name, (rx, ry, rz) in rots.items():
        arm.pose.bones[name].rotation_euler = (math.radians(rx), math.radians(ry), math.radians(rz))
    arm.pose.bones["hips"].location = (0, hips_z, 0)  # eixo Y local do osso = cima
    for pb in arm.pose.bones:
        pb.keyframe_insert("rotation_euler", frame=frame)
        pb.keyframe_insert("location", frame=frame)


def new_action(name):
    arm.animation_data_create()
    act = bpy.data.actions.new(name)
    act.use_fake_user = True
    arm.animation_data.action = act
    return act


actions = []

# Idle: respiracao leve, 60 quadros
act = new_action("Idle")
for f in range(0, 61, 10):
    t = f / 60 * 2 * math.pi
    breathe = math.sin(t)
    key_pose(f, {
        "chest": (breathe * 2.0, 0, 0),
        "head": (-breathe * 1.5, 0, 0),
        "upper_arm.L": (0, 0, breathe * 2.0),
        "upper_arm.R": (0, 0, -breathe * 2.0),
        "forearm.L": (6, 0, 0),
        "forearm.R": (6, 0, 0),
    }, hips_z=breathe * 0.006)
actions.append(act)

# Walk: ciclo de 24 quadros
act = new_action("Walk")
for f in range(0, 25, 3):
    t = f / 24 * 2 * math.pi
    s = math.sin(t)
    knee_l = max(0.0, -math.sin(t - 0.6)) * 45
    knee_r = max(0.0, math.sin(t - 0.6)) * 45
    key_pose(f, {
        "thigh.L": (s * 30, 0, 0),
        "thigh.R": (-s * 30, 0, 0),
        "shin.L": (-knee_l, 0, 0),
        "shin.R": (-knee_r, 0, 0),
        "upper_arm.L": (-s * 28, 0, 0),
        "upper_arm.R": (s * 28, 0, 0),
        "forearm.L": (12, 0, 0),
        "forearm.R": (12, 0, 0),
        "chest": (0, s * 6, 0),
        "spine": (4, 0, 0),
    }, hips_z=abs(math.cos(t)) * 0.035)
actions.append(act)

# Cast: braco direito lanca o elemento, 18 quadros
act = new_action("Cast")
cast_keys = [(0, 0), (5, 85), (9, 95), (18, 0)]
for f, a in cast_keys:
    key_pose(f, {
        "upper_arm.R": (a, 0, 0),
        "forearm.R": (a * 0.15, 0, 0),
        "chest": (0, a * 0.18, 0),
        "upper_arm.L": (a * 0.25, 0, 0),
    })
actions.append(act)

# empilha as acoes em trilhas NLA para exportar todas
arm.animation_data.action = None
for act in actions:
    track = arm.animation_data.nla_tracks.new()
    track.name = act.name
    strip = track.strips.new(act.name, int(act.frame_range[0]), act)
    track.mute = True

scene.frame_start = 0
scene.frame_end = 60

# ---------------------------------------------------------------- salvar
os.makedirs(os.path.dirname(BLEND_OUT), exist_ok=True)
os.makedirs(os.path.dirname(FBX_OUT), exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=BLEND_OUT)

bpy.ops.object.select_all(action="SELECT")
bpy.ops.export_scene.fbx(
    filepath=FBX_OUT,
    use_selection=False,
    apply_unit_scale=True,
    apply_scale_options="FBX_SCALE_ALL",
    axis_forward="-Z",
    axis_up="Y",
    object_types={"ARMATURE", "MESH"},
    add_leaf_bones=False,
    bake_anim=True,
    bake_anim_use_all_actions=True,
    bake_anim_use_nla_strips=False,
    bake_anim_force_startend_keying=True,
    bake_anim_simplify_factor=0.0,
)
print("OK_MAGNUS", len(body.data.vertices), [a.name for a in actions])
