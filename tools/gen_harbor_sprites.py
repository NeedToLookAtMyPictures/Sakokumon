import struct, zlib, os, sys
sys.path.insert(0, os.path.dirname(__file__))
from gen_sprites import make_png, sprite

OUT = r'c:\Users\bmmah\OneDrive\Desktop\Classes\Summer 2026\3D Engines in Practice\Godot\Sakokumon\assets\sprites\scenery\harborView'

T, F = True, False

# Each building sprite grid is sized to be close to its collision-box dimensions.
# At cell=64px the generated pixel sizes are:
#   bldgNW:    256 x 192  (collision 280 x 169)
#   bldgNE:    384 x 192  (collision 395 x 164)
#   bldgSW:    320 x 192  (collision 314 x 212)
#   bldgSE:    384 x 192  (collision 357 x 209)
#   keepWallL: 256 x  64  (collision 259 x  77)
#   keepWallR: 384 x  64  (collision 351 x  74)

buildings = {
    'bldgNW': (
        [[T, T, T, T],
         [T, T, T, T],
         [T, T, T, T]],
        (195, 160, 110),   # warm sandstone — merchant quarter north-west
    ),
    'bldgNE': (
        [[T, T, T, T, T, T],
         [T, T, T, T, T, T],
         [T, T, T, T, T, T]],
        (170, 148, 108),   # cooler sandstone — merchant quarter north-east
    ),
    'bldgSW': (
        [[T, T, T, T, T],
         [T, T, T, T, T],
         [T, T, T, T, T]],
        (128,  98,  62),   # dark timber — warehouse south-west
    ),
    'bldgSE': (
        [[T, T, T, T, T, T],
         [T, T, T, T, T, T],
         [T, T, T, T, T, T]],
        (140, 110,  72),   # slightly lighter timber — warehouse south-east
    ),
    'keepWallL': (
        [[T, T, T, T]],
        (130, 126, 118),   # stone gray — keep wall left
    ),
    'keepWallR': (
        [[T, T, T, T, T, T]],
        (130, 126, 118),   # stone gray — keep wall right
    ),
}

for name, (grid, color) in buildings.items():
    data = sprite(grid, color)
    path = os.path.join(OUT, f'{name}.png')
    with open(path, 'wb') as f:
        f.write(data)
    cols, rows = len(grid[0]), len(grid)
    print(f'  {name}.png  ({cols*64}x{rows*64})')

print(f'\nDone — {len(buildings)} sprites written to {OUT}')
