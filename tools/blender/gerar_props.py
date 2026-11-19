"""Gera os props low-poly da fase (um FBX por prop) e salva a fonte editavel.

Uso:
    blender -b --factory-startup --python tools/blender/gerar_props.py

Saidas:
    Art/Modelos3D/Props.blend
    Between fire and water/Assets/Models/Props/<Nome>.fbx
        Caixa       caixa de madeira empurravel (1 m)
        Pista       papiro selado com o simbolo da ASDA
        Portao      folha do portao da base (2.2 x 3.4 m), pivo na dobradica
        Barraca     barraca do acampamento
        Braseiro    base de pedra das barreiras de fogo
        Torre       torre de vigia da base
"""
import math
import os

import bpy

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
BLEND_OUT = os.path.join(ROOT, "Art", "Modelos3D", "Props.blend")
FBX_DIR = os.path.join(ROOT, "Between fire and water", "Assets", "Models", "Props")

bpy.ops.wm.read_factory_settings(use_empty=True)


def material(name, rgb, rough=0.8, metal=0.0):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*rgb, 1.0)
    bsdf.inputs["Roughness"].default_value = rough
    bsdf.inputs["Metallic"].default_value = metal
    mat.diffuse_color = (*rgb, 1.0)
    return mat


M = {
    "madeira": material("Prop_Madeira", (0.55, 0.36, 0.18)),
    "madeira_escura": material("Prop_MadeiraEscura", (0.30, 0.18, 0.09)),
    "papiro": material("Prop_Papiro", (0.93, 0.85, 0.62)),
    "selo": material("Prop_SeloASDA", (0.70, 0.05, 0.08)),
    "metal": material("Prop_MetalASDA", (0.16, 0.18, 0.24), 0.45, 0.8),
    "detalhe": material("Prop_DetalheASDA", (0.85, 0.65, 0.12), 0.4, 0.6),
    "lona": material("Prop_Lona", (0.42, 0.45, 0.30)),
    "pedra": material("Prop_Pedra", (0.42, 0.40, 0.38)),
    "brasa": material("Prop_Brasa", (0.35, 0.08, 0.02)),
}

collections = {}


def new_group(name):
    col = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(col)
    collections[name] = col
    return col


def prim(kind, col, name, loc, scale, mat, rot=(0, 0, 0), **kw):
    if kind == "cube":
        bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    elif kind == "cyl":
        bpy.ops.mesh.primitive_cylinder_add(vertices=kw.get("v", 12), radius=0.5, depth=1, location=loc, rotation=rot)
    elif kind == "cone":
        bpy.ops.mesh.primitive_cone_add(vertices=kw.get("v", 4), radius1=0.5, depth=1, location=loc, rotation=rot)
    o = bpy.context.active_object
    o.name = name
    o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    o.data.materials.append(mat)
    for c in o.users_collection:
        c.objects.unlink(o)
    col.objects.link(o)
    return o


def bevel(o, w=0.03):
    m = o.modifiers.new("Bevel", "BEVEL")
    m.width = w
    m.segments = 1


# ---------------- Caixa (1 m, origem no centro da base)
c = new_group("Caixa")
bevel(prim("cube", c, "Caixa_Corpo", (0, 0, 0.5), (0.96, 0.96, 0.96), M["madeira"]), 0.02)
for i, (x, y, sx, sy) in enumerate([(0, -0.49, 1.0, 0.04), (0, 0.49, 1.0, 0.04), (-0.49, 0, 0.04, 1.0), (0.49, 0, 0.04, 1.0)]):
    prim("cube", c, f"Caixa_Ripa_Alta{i}", (x, y, 0.94), (sx, sy, 0.1), M["madeira_escura"])
    prim("cube", c, f"Caixa_Ripa_Baixa{i}", (x, y, 0.06), (sx, sy, 0.1), M["madeira_escura"])
for i, ang in enumerate((45, -45)):
    prim("cube", c, f"Caixa_X_Frente{i}", (0, -0.49, 0.5), (1.25, 0.04, 0.09), M["madeira_escura"], rot=(0, math.radians(ang), 0))
    prim("cube", c, f"Caixa_X_Tras{i}", (0, 0.49, 0.5), (1.25, 0.04, 0.09), M["madeira_escura"], rot=(0, math.radians(ang), 0))

# ---------------- Pista (papiro com selo), origem no centro
c = new_group("Pista")
prim("cyl", c, "Pista_Rolo", (0, 0, 0), (0.16, 0.16, 0.6), M["papiro"], rot=(0, math.radians(90), 0), v=12)
prim("cyl", c, "Pista_Tampa_E", (-0.33, 0, 0), (0.2, 0.2, 0.06), M["madeira_escura"], rot=(0, math.radians(90), 0), v=12)
prim("cyl", c, "Pista_Tampa_D", (0.33, 0, 0), (0.2, 0.2, 0.06), M["madeira_escura"], rot=(0, math.radians(90), 0), v=12)
prim("cyl", c, "Pista_Selo", (0, -0.085, 0), (0.12, 0.12, 0.03), M["selo"], rot=(math.radians(90), 0, 0), v=16)

# ---------------- Portao (folha), pivo na dobradica esquerda (x=0), abre girando em Z
c = new_group("Portao")
W, H = 2.2, 3.4
bevel(prim("cube", c, "Portao_Folha", (W / 2, 0, H / 2), (W, 0.18, H), M["metal"]), 0.03)
for i, z in enumerate((0.5, 1.7, 2.9)):
    prim("cube", c, f"Portao_Faixa{i}", (W / 2, -0.1, z), (W - 0.1, 0.04, 0.16), M["detalhe"])
prim("cyl", c, "Portao_Emblema", (W / 2, -0.11, 2.3), (0.7, 0.7, 0.04), M["detalhe"], rot=(math.radians(90), 0, 0), v=6)
prim("cyl", c, "Portao_Dobradica", (0.05, 0, H / 2), (0.16, 0.16, H), M["metal"], v=8)

# ---------------- Barraca do acampamento
c = new_group("Barraca")
prim("cone", c, "Barraca_Lona", (0, 0, 1.1), (3.2, 3.2, 2.2), M["lona"], rot=(0, 0, math.radians(45)), v=4)
prim("cyl", c, "Barraca_Mastro", (0, 0, 1.3), (0.08, 0.08, 2.6), M["madeira_escura"], v=6)
prim("cube", c, "Barraca_Entrada", (0, -1.12, 0.55), (0.8, 0.05, 1.1), M["madeira_escura"])

# ---------------- Braseiro (base das barreiras de fogo), 2.4 m de largura
c = new_group("Braseiro")
bevel(prim("cube", c, "Braseiro_Base", (0, 0, 0.2), (2.6, 0.9, 0.4), M["pedra"]), 0.05)
prim("cube", c, "Braseiro_Brasas", (0, 0, 0.42), (2.3, 0.65, 0.06), M["brasa"])
for i, x in enumerate((-1.2, 1.2)):
    prim("cyl", c, f"Braseiro_Pilar{i}", (x, 0, 0.5), (0.35, 0.35, 1.0), M["pedra"], v=8)

# ---------------- Torre de vigia
c = new_group("Torre")
for i, (x, y) in enumerate([(-0.9, -0.9), (0.9, -0.9), (-0.9, 0.9), (0.9, 0.9)]):
    prim("cyl", c, f"Torre_Pe{i}", (x, y, 2.0), (0.22, 0.22, 4.0), M["madeira_escura"], v=6)
prim("cube", c, "Torre_Piso", (0, 0, 4.0), (2.4, 2.4, 0.2), M["madeira"])
for i, (x, y, sx, sy) in enumerate([(0, -1.15, 2.4, 0.1), (0, 1.15, 2.4, 0.1), (-1.15, 0, 0.1, 2.4), (1.15, 0, 0.1, 2.4)]):
    prim("cube", c, f"Torre_Parapeito{i}", (x, y, 4.45), (sx, sy, 0.7), M["madeira"])
prim("cone", c, "Torre_Telhado", (0, 0, 5.6), (3.2, 3.2, 1.2), M["metal"], rot=(0, 0, math.radians(45)), v=4)
for i, (x, y) in enumerate([(-1.1, -1.1), (1.1, -1.1), (-1.1, 1.1), (1.1, 1.1)]):
    prim("cyl", c, f"Torre_Coluna{i}", (x, y, 5.0), (0.12, 0.12, 1.0), M["madeira_escura"], v=6)

# ---------------- salvar e exportar
os.makedirs(os.path.dirname(BLEND_OUT), exist_ok=True)
os.makedirs(FBX_DIR, exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=BLEND_OUT)

for name, col in collections.items():
    bpy.ops.object.select_all(action="DESELECT")
    objs = list(col.objects)
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.object.convert(target="MESH")
    bpy.ops.object.join()
    joined = bpy.context.active_object
    joined.name = name
    # origem no ponto (0,0,0) da cena: base do prop / dobradiça do portão
    bpy.context.scene.cursor.location = (0, 0, 0)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(FBX_DIR, name + ".fbx"),
        use_selection=True,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        object_types={"MESH"},
        bake_anim=False,
    )
    print("OK_PROP", name, len(joined.data.vertices))
