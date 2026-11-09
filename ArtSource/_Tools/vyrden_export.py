"""
Vyrden FBX export for Unity.

Exports the objects in the blend file's "Export" collection with the project's standard FBX
settings, after checking them against the modeling rules in ARCHITECTURE.md (Asset pipeline).

Usage (from the repository root):
    blender --background ArtSource/Enemies/SK_BasicEnemy.blend \
        --python ArtSource/_Tools/vyrden_export.py -- \
        --output Assets/Art/Characters/BasicEnemy/SK_BasicEnemy.fbx [--animations] [--collection Export]

Prints one line starting with "VYRDEN_EXPORT " followed by a JSON report.
Exit code 1 means a rule was broken and nothing was written.
"""
import argparse
import json
import math
import os
import sys

import bpy

FBX_SETTINGS = dict(
    apply_unit_scale=True,
    apply_scale_options='FBX_SCALE_UNITS',
    axis_forward='-Z',
    axis_up='Y',
    bake_space_transform=True,
    use_mesh_modifiers=True,
    mesh_smooth_type='FACE',
    use_tspace=False,
    add_leaf_bones=False,
    use_armature_deform_only=True,
    armature_nodetype='NULL',
    path_mode='STRIP',
)


def parse_args():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser(description="Vyrden FBX export")
    parser.add_argument("--output", required=True, help="Target .fbx path")
    parser.add_argument("--collection", default="Export", help="Collection holding the objects to export")
    parser.add_argument("--animations", action="store_true", help="Bake and export animation actions")
    return parser.parse_args(argv)


def is_identity_scale(obj):
    return all(math.isclose(s, 1.0, abs_tol=1e-4) for s in obj.scale)


def is_identity_rotation(obj):
    if obj.rotation_mode == 'QUATERNION':
        return math.isclose(obj.rotation_quaternion.angle, 0.0, abs_tol=1e-4)
    if obj.rotation_mode == 'AXIS_ANGLE':
        return math.isclose(obj.rotation_axis_angle[0], 0.0, abs_tol=1e-4)
    return all(math.isclose(r, 0.0, abs_tol=1e-4) for r in obj.rotation_euler)


def check_objects(objects):
    errors, warnings, stats = [], [], {"objects": [], "triangles": 0}
    for obj in objects:
        info = {"name": obj.name, "type": obj.type}
        if not is_identity_scale(obj):
            errors.append(f"{obj.name}: scale not applied {tuple(round(s, 4) for s in obj.scale)}")
        if not is_identity_rotation(obj):
            errors.append(f"{obj.name}: rotation not applied")
        if obj.parent is None and obj.location.length > 1e-4:
            warnings.append(f"{obj.name}: root object is not at the world origin {tuple(round(v, 3) for v in obj.location)}")
        if obj.type == 'MESH':
            mesh = obj.data
            tris = sum(len(p.vertices) - 2 for p in mesh.polygons)
            info["triangles"] = tris
            stats["triangles"] += tris
            if not mesh.uv_layers:
                warnings.append(f"{obj.name}: no UV map")
            if not mesh.materials:
                warnings.append(f"{obj.name}: no material slot")
            if len(mesh.materials) > 2:
                warnings.append(f"{obj.name}: {len(mesh.materials)} material slots (budget is 2)")
        stats["objects"].append(info)
    return errors, warnings, stats


def main():
    args = parse_args()
    collection = bpy.data.collections.get(args.collection)
    if collection is None:
        report = {"ok": False, "errors": [f"collection '{args.collection}' not found"]}
        print("VYRDEN_EXPORT " + json.dumps(report))
        sys.exit(1)

    objects = [o for o in collection.all_objects if o.type in {'MESH', 'ARMATURE', 'EMPTY'}]
    errors, warnings, stats = check_objects(objects)
    report = {"ok": not errors, "blend": bpy.data.filepath, "output": os.path.abspath(args.output),
              "errors": errors, "warnings": warnings, **stats}

    if errors:
        print("VYRDEN_EXPORT " + json.dumps(report))
        sys.exit(1)

    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:
        obj.hide_set(False)
        obj.select_set(True)

    os.makedirs(os.path.dirname(os.path.abspath(args.output)), exist_ok=True)
    bpy.ops.export_scene.fbx(
        filepath=args.output,
        use_selection=True,
        object_types={'MESH', 'ARMATURE', 'EMPTY'},
        bake_anim=args.animations,
        bake_anim_use_all_actions=args.animations,
        bake_anim_use_nla_strips=False,
        bake_anim_force_startend_keying=True,
        bake_anim_simplify_factor=0.0,
        **FBX_SETTINGS,
    )
    report["bytes"] = os.path.getsize(args.output)
    print("VYRDEN_EXPORT " + json.dumps(report))


main()
